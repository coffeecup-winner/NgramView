using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NgramView.Providers.Google.Offline.OptimizedData {
    unsafe public class OptimizedNgramHeader {
        List<OptimizedNgramHeaderEntry> entries = new List<OptimizedNgramHeaderEntry>();
        HeaderTableEntry[] headerTable;
        List<HeaderTableEntry> searchableHeaderTable;

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
            this.entries = this.entries.OrderBy(e => e.Ngram).ToList();
            int headerTableSize = this.entries.Count;
            headerTable = new HeaderTableEntry[headerTableSize];
            uint pos = (uint)(headerTableSize * sizeof(HeaderTableEntry) + sizeof(int));
            for(int i = 0; i < this.entries.Count; i++) {
                OptimizedNgramHeaderEntry entry = this.entries[i];
                HeaderTableEntry tableEntry = new HeaderTableEntry();
                tableEntry.NgramOffset = pos;
                tableEntry.NgramLength = (uint)Encoding.UTF8.GetByteCount(entry.Ngram);
                pos += tableEntry.NgramLength;
                tableEntry.YearsDataOffset = entry.Offset;
                tableEntry.YearsDataLength = entry.EndOffset;
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
            foreach(var entry in this.entries) {
                buffer = Encoding.UTF8.GetBytes(entry.Ngram);
                stream.Write(buffer, 0, buffer.Length);
            }
            stream.Flush();
        }
        public OptimizedNgramHeaderEntry Find(string ngram, Stream stream) {
            if(this.searchableHeaderTable == null)
                BuildSearchableHeaderTable();
            int result = this.searchableHeaderTable.BinarySearch(new HeaderTableEntry { }, new HeaderTableEntryComparer(ngram, stream));
            var tableEntry = this.searchableHeaderTable[result];
            return new OptimizedNgramHeaderEntry(ngram, tableEntry.YearsDataOffset) { EndOffset = tableEntry.YearsDataLength };
        }
        void BuildSearchableHeaderTable() {
            this.searchableHeaderTable = new List<HeaderTableEntry>(this.headerTable);
        }
    }

    unsafe public struct HeaderTableEntry {
        public uint NgramOffset;
        public uint NgramLength;
        public uint YearsDataOffset;
        public uint YearsDataLength;
    }

    public class HeaderTableEntryComparer : IComparer<HeaderTableEntry> {
        readonly string ngram;
        readonly Stream stream;

        public HeaderTableEntryComparer(string ngram, Stream stream) {
            this.ngram = ngram;
            this.stream = stream;
        }
        public int Compare(HeaderTableEntry element, HeaderTableEntry _) {
            stream.Seek(element.NgramOffset, SeekOrigin.Begin);
            byte[] buffer = new byte[element.NgramLength];
            int check = stream.Read(buffer, 0, buffer.Length);
            Debug.Assert(buffer.Length == check);
            string ngram = Encoding.UTF8.GetString(buffer);
            return ngram.CompareTo(this.ngram);
        }
    }

    public class OptimizedNgramHeaderEntry {
        readonly string ngram;
        readonly uint offset;

        public OptimizedNgramHeaderEntry(string ngram, uint offset) {
            this.ngram = ngram;
            this.offset = offset;
        }
        public string Ngram { get { return ngram; } }
        public uint Offset { get { return offset; } }
        public uint EndOffset { get; set; }
    }
}
