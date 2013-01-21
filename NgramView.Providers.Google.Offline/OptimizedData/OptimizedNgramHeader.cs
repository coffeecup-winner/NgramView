using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading;

namespace NgramView.Providers.Google.Offline.OptimizedData {
    unsafe public class OptimizedNgramHeader {
        List<OptimizedNgramHeaderEntry> entries = new List<OptimizedNgramHeaderEntry>();
        HeaderTableEntry[] headerTable;

        public OptimizedNgramHeader() { }
        public OptimizedNgramHeader(Stream stream) {
            byte[] buffer = new byte[sizeof(int)];
            int check = stream.Read(buffer, 0, sizeof(int));
            Debug.Assert(sizeof(int) == check);
            int headerTableSize = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[headerTableSize];
            check = stream.Read(buffer, 0, headerTableSize);
            Debug.Assert(headerTableSize == check);
            headerTable = new HeaderTableEntry[headerTableSize / sizeof(HeaderTableEntry)];
            Marshal.Copy(buffer, 0, Marshal.UnsafeAddrOfPinnedArrayElement(this.headerTable, 0), buffer.Length);
        }
        public OptimizedNgramHeaderEntry Add(string ngram, uint offset) {
            OptimizedNgramHeaderEntry entry = new OptimizedNgramHeaderEntry(ngram, offset);
            this.entries.Add(entry);
            return entry;
        }
        public void Build() {
            this.entries = this.entries.OrderBy(e => e.NgramHash).ToList();
#if DEBUG
            for(int i = 1; i < entries.Count; i++) {
                if(entries[i - 1].NgramHash == entries[i].NgramHash)
                    Debugger.Break();
            }
#endif
            int headerTableSize = this.entries.Count;
            headerTable = new HeaderTableEntry[headerTableSize];
            for(int i = 0; i < this.entries.Count; i++) {
                OptimizedNgramHeaderEntry entry = this.entries[i];
                HeaderTableEntry tableEntry = new HeaderTableEntry();
                tableEntry.NgramHash = entry.NgramHash;
                tableEntry.DataOffset = entry.Offset;
                tableEntry.DataLength = entry.Length;
                headerTable[i] = tableEntry;
            }
        }
        public void WriteTo(Stream stream) {
            if(this.headerTable == null)
                throw new InvalidOperationException("This header is not built.");
            byte[] buffer = BitConverter.GetBytes(this.headerTable.Length * sizeof(HeaderTableEntry));
            stream.Write(buffer, 0, buffer.Length);
            buffer = new byte[this.headerTable.Length * sizeof(HeaderTableEntry)];
            Marshal.Copy(Marshal.UnsafeAddrOfPinnedArrayElement(this.headerTable, 0), (byte[])buffer, 0, this.headerTable.Length * sizeof(HeaderTableEntry));
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }
        public OptimizedNgramHeaderEntry Find(string ngram, Stream stream) {
            HeaderTableEntry dummy = new HeaderTableEntry();
            int result = Array.BinarySearch(this.headerTable, 0, this.headerTable.Length, dummy, new HeaderTableEntryComparer(ngram, stream));
            var tableEntry = this.headerTable[result];
            return new OptimizedNgramHeaderEntry(ngram, tableEntry.DataOffset) { Length = tableEntry.DataLength };
        }
        public static long Hash(string str) {
            byte[] bytes = Encoding.Unicode.GetBytes(str);
            SHA1 hash = new SHA1CryptoServiceProvider();
            byte[] hashText = hash.ComputeHash(bytes);
            long hashCodeStart = BitConverter.ToInt64(hashText, 0);
            long hashCodeMedium = BitConverter.ToInt64(hashText, 6);
            long hashCodeEnd = BitConverter.ToInt64(hashText, 12);
            return hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct HeaderTableEntry {
        [FieldOffset(0)]
        public long NgramHash;
        [FieldOffset(8)]
        public uint DataOffset;
        [FieldOffset(12)]
        public uint DataLength;
    }

    public class HeaderTableEntryComparer : IComparer<HeaderTableEntry> {
        readonly long ngramHash;
        readonly Stream stream;

        public HeaderTableEntryComparer(string ngram, Stream stream) {
            this.ngramHash = OptimizedNgramHeader.Hash(ngram);
            this.stream = stream;
        }
        public int Compare(HeaderTableEntry element, HeaderTableEntry _) {
            return element.NgramHash.CompareTo(this.ngramHash);
        }
    }

    public class OptimizedNgramHeaderEntry {
#if DEBUG
        readonly string ngram;
#endif
        ManualResetEvent evt;
        long ngramHash;
        readonly uint offset;

        public OptimizedNgramHeaderEntry(string ngram, uint offset) {
#if DEBUG
            this.ngram = ngram;
#endif
            this.evt = new ManualResetEvent(false);
            Task.Factory.StartNew(() => {
                this.ngramHash = OptimizedNgramHeader.Hash(ngram);
                evt.Set();
            });
            this.offset = offset;
        }
        public long NgramHash {
            get {
                evt.WaitOne();
                return ngramHash;
            }
        }
        public uint Offset { get { return offset; } }
        public uint Length { get; set; }
    }
}
