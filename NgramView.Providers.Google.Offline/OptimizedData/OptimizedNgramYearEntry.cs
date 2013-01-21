using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NgramView.Data;

namespace NgramView.Providers.Google.Offline.OptimizedData {
    public class OptimizedNgramYearEntry {
        /*
         * Short bitmap fbbb oooo
         * Bitmap: fffb bbbb | <additional books bytes> | oooo oooo | <additional occurences bytes>
         * In more than 94% of cases (1gram, s) the format will be 3 bytes: ffbb bbbb | oooo oooo
         * f is flags
         * b is books count
         * o is occurences count
         */
        const byte FlagShortEntry = 0x80;
        const byte FlagAddByte = 0x40;
        const byte FlagAddTwoBytes = 0x20;
        const byte ClearFlagsMask = 0x1F;
        const byte ClearHighBitMask = 0x7F;
        const byte Clear4HighBitsMask = 0x0F;
        const byte Clear7HighBitsMask = 0x01;
        const int MinYear = 1500;
        readonly NgramYearEntry entry;
        int bytesCount;
        byte[] bytes;

        public OptimizedNgramYearEntry(NgramYearEntry entry) {
            this.entry = entry;
            int minBitsForOccurencesCount = (int)Math.Ceiling(Math.Log(entry.OccurencesCount + 1, 2));
            int minBitsForBooksCount = (int)Math.Ceiling(Math.Log(entry.DistinctBooksCount + 1, 2));
            Debug.Assert(minBitsForBooksCount <= 29);
            Debug.Assert(minBitsForOccurencesCount <= 32);
            if(minBitsForOccurencesCount <= 4 && minBitsForBooksCount <= 3)
                PackShort(entry);
            else
                PackNormal(entry, (int)Math.Ceiling((double)Math.Max(minBitsForOccurencesCount, minBitsForBooksCount) / 8));
        }
        public OptimizedNgramYearEntry(byte[] bytes, int index) {
            byte firstByte = bytes[index];
            if((firstByte & FlagShortEntry) == FlagShortEntry)
                this.entry = UnpackShort(bytes, index);
            else
                this.entry = UnpackNormal(bytes, index);
        }
        void PackShort(NgramYearEntry entry) {
            this.bytesCount = 1;
            this.bytes = new byte[BytesCount];
            byte[] buffer = BitConverter.GetBytes(entry.DistinctBooksCount);
            Bytes[0] = FlagShortEntry;
            Bytes[0] |= (byte)(buffer[0] << 4);
            buffer = BitConverter.GetBytes(entry.OccurencesCount);
            Bytes[0] |= buffer[0];
        }
        void PackNormal(NgramYearEntry entry, int minBytes) {
            this.bytesCount = minBytes * 2;
            this.bytes = new byte[BytesCount];
            byte[] buffer = BitConverter.GetBytes(entry.DistinctBooksCount);
            bytes[0] = (byte)(buffer[0] & ClearFlagsMask);
            if(minBytes % 2 == 0)
                bytes[0] |= FlagAddByte;
            if(minBytes >= 3)
                bytes[0] |= FlagAddTwoBytes;
            for(int i = 1; i < minBytes; i++)
                bytes[i] = (byte)((byte)(buffer[i - 1] >> 5) | (buffer[i] << 3));
            buffer = BitConverter.GetBytes(entry.OccurencesCount);
            for(int i = 0; i < minBytes; i++)
                bytes[minBytes + i] = buffer[i];
        }
        NgramYearEntry UnpackShort(byte[] bytes, int index) {
            this.bytesCount = 1;
            this.bytes = new byte[BytesCount];
            byte firstByte = bytes[index];
            this.bytes[0] = firstByte;
            byte[] buffer = new byte[sizeof(int)];
            firstByte &= ClearHighBitMask;
            buffer[0] = (byte)(firstByte >> 4);
            int distinctBooksCount = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[sizeof(int)];
            buffer[0] = (byte)(firstByte & Clear4HighBitsMask);
            int occurencesCount = BitConverter.ToInt32(buffer, 0);
            return new NgramYearEntry(-1, occurencesCount, distinctBooksCount);
        }
        NgramYearEntry UnpackNormal(byte[] bytes, int index) {
            int minBytes = 1;
            byte firstByte = bytes[index];
            if((firstByte & FlagAddByte) == FlagAddByte)
                minBytes++;
            if((firstByte & FlagAddTwoBytes) == FlagAddTwoBytes)
                minBytes += 2;
            this.bytesCount = minBytes * 2;
            this.bytes = new byte[BytesCount];
            Array.Copy(bytes, index, this.bytes, 0, BytesCount);
            byte[] buffer = new byte[sizeof(int)];
            buffer[0] = (byte)(this.bytes[0] & ClearFlagsMask);
            for(int i = 1; i < minBytes; i++) {
                buffer[i - 1] |= (byte)(this.bytes[i] << 5);
                buffer[i] = (byte)((this.bytes[i] >> 3) & ClearFlagsMask);
            }
            int distinctBooksCount = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[sizeof(int)];
            for(int i = 0; i < minBytes; i++)
                buffer[i] = this.bytes[minBytes + i];
            int occurencesCount = BitConverter.ToInt32(buffer, 0);
            return new NgramYearEntry(-1, occurencesCount, distinctBooksCount);
        }
        public int BytesCount { get { return bytesCount; } }
        public byte[] Bytes { get { return bytes; } }
        public NgramYearEntry Entry { get { return entry; } }
        public void WriteTo(Stream stream) {
            stream.Write(Bytes, 0, BytesCount);
        }
    }
}
