using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NgramView.Data;
using System.Diagnostics;

namespace NgramView.Providers.Google.Offline {
    public class OptimizedNgramData : BaseNgramData {
        public OptimizedNgramData(string filepath)
            : base(filepath) {
        }
        public void Optimize() {
            using(FileStream stream = File.OpenRead(FilePath)) {
                GZipStream gzStream = new GZipStream(stream, CompressionMode.Decompress);
                StreamReader reader = new StreamReader(gzStream);
                List<string> ngrams = new List<string>();
                using(Stream outStream = File.Create(Path.ChangeExtension(FilePath, ".dat"))) {
                    string line = null;
                    while(!reader.EndOfStream) {
                        var dataEntry = ReadEntry(reader, ref line);
                        ngrams.Add(dataEntry.Ngram + "\t" + outStream.Position);
                        foreach(var yearEntry in dataEntry.YearEntries)
                            WriteEntry(outStream, yearEntry);
                    }
                }
                using(StreamWriter writer = new StreamWriter(new GZipStream(File.Create(Path.ChangeExtension(FilePath, ".idx.gz")), CompressionMode.Compress))) {
                    foreach(var ngram in ngrams)
                        writer.WriteLine(ngram);
                }
            }
        }
        public override NgramDataEntry Query(string ngram) {
            NgramDataEntry dataEntry = new NgramDataEntry(ngram);
            int offset = 0;
            int nextOffset = -1;
            using(FileStream stream = File.OpenRead(FilePath)) {
                GZipStream gzStream = new GZipStream(stream, CompressionMode.Decompress);
                StreamReader reader = new StreamReader(gzStream);
                string line;
                while(!(line = reader.ReadLine()).StartsWith(ngram + '\t') && !reader.EndOfStream) { }
                offset = int.Parse(line.Split('\t')[1]);
                line = reader.ReadLine();
                if(line != null)
                    nextOffset = int.Parse(line.Split('\t')[1]);
            }
            using(FileStream stream = File.OpenRead(FilePath.Replace(".idx.gz", ".dat"))) {
                stream.Seek(offset, SeekOrigin.Begin);
                do {
                    dataEntry.Add(ReadEntry(stream));
                } while(stream.Position < stream.Length && stream.Position < nextOffset);
            }
            return dataEntry;
        }
        void WriteEntry(Stream stream, NgramYearEntry yearEntry) {
            new OptimizedNgramDataEntry(yearEntry).WriteTo(stream);
        }
        NgramYearEntry ReadEntry(Stream stream) {
            return new OptimizedNgramDataEntry(stream).Entry;
        }
    }

    public class OptimizedNgramDataEntry {
        /*
         * Bitmap: ffbb bbbb | <additional books bytes> | yyyy yyyy | yooo oooo | <additional occurences bytes>
         * In ~94% of cases (1gram, s) the format will be 3 bytes: ffbb bbbb | yyyy yyyy | yooo oooo
         * Min year is 1500, max year is 2008, 9 bits is enough
         * f is flags
         * b is books count
         * y is year
         * o is occurences count
         */
        const byte FlagAddByte = 0x80;
        const byte FlagAddTwoBytes = 0x40;
        const byte ClearFlagsMask = 0x3F;
        const byte ClearHighBitMask = 0x7F;
        readonly int bytesCount;
        readonly byte[] bytes;
        readonly NgramYearEntry entry;

        public OptimizedNgramDataEntry(NgramYearEntry entry) {
            this.entry = entry;
            int minBitsForOccurencesCount = (int)Math.Ceiling(Math.Log(entry.OccurencesCount + 1, 2));
            int minBitsForBooksCount = (int)Math.Ceiling(Math.Log(entry.DistinctBooksCount + 1, 2));
            Debug.Assert(minBitsForBooksCount <= 30);
            Debug.Assert(minBitsForOccurencesCount <= 31);
            int minBytes = (int)Math.Ceiling((double)Math.Max(minBitsForOccurencesCount, minBitsForBooksCount) / 8);
            this.bytesCount = 1 + minBytes * 2;
            this.bytes = new byte[BytesCount];
            byte[] buffer = BitConverter.GetBytes(entry.DistinctBooksCount);
            bytes[0] = (byte)(buffer[0] & ClearHighBitMask);
            if(minBytes % 2 == 0)
                bytes[0] |= FlagAddByte;
            if(minBytes >= 3)
                bytes[0] |= FlagAddTwoBytes;
            for(int i = 1; i < minBytes; i++)
                bytes[i] = (byte)((byte)(buffer[i - 1] >> 6) | (buffer[i] << 2));
            buffer = BitConverter.GetBytes((short)(entry.Year - 1500));
            bytes[minBytes] = buffer[0];
            byte highYearBit = (byte)(buffer[1] << 7);
            buffer = BitConverter.GetBytes(entry.OccurencesCount);
            bytes[minBytes + 1] = (byte)((buffer[0] & ClearHighBitMask) | highYearBit);
            for(int i = 1; i < minBytes; i++)
                bytes[minBytes + 1 + i] = (byte)((byte)(buffer[i - 1] >> 7) | (buffer[i] << 1));
        }
        public OptimizedNgramDataEntry(Stream stream) {
            byte[] firstByte = new byte[1];
            int check = stream.Read(firstByte, 0, 1);
            Debug.Assert(1 == check);
            int minBytes = 1;
            if((firstByte[0] & FlagAddByte) == FlagAddByte)
                minBytes++;
            if((firstByte[0] & FlagAddTwoBytes) == FlagAddTwoBytes)
                minBytes += 2;
            this.bytesCount = 1 + minBytes * 2;
            this.bytes = new byte[BytesCount];
            bytes[0] = firstByte[0];
            check = stream.Read(bytes, 1, BytesCount - 1);
            Debug.Assert(BytesCount - 1 == check);
            byte[] buffer = new byte[sizeof(int)];
            buffer[0] = (byte)(bytes[0] & ClearFlagsMask);
            for(int i = 1; i < minBytes; i++) {
                buffer[i - 1] |= (byte)(bytes[i] << 6);
                buffer[i] = (byte)((bytes[i] >> 2) & ClearFlagsMask);
            }
            int distinctBooksCount = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[sizeof(short)];
            buffer[0] = bytes[minBytes];
            buffer[1] = (byte)(bytes[minBytes + 1] >> 7);
            int year = BitConverter.ToInt16(buffer, 0) + 1500;
            buffer = new byte[sizeof(int)];
            buffer[0] = (byte)(bytes[minBytes + 1] & ClearHighBitMask);
            for(int i = 1; i < minBytes; i++) {
                buffer[i - 1] |= (byte)(bytes[minBytes + 1 + i] << 7);
                buffer[i] = (byte)((bytes[minBytes + 1 + i] >> 1) & ClearHighBitMask);
            }
            int occurencesCount = BitConverter.ToInt32(buffer, 0);
            this.entry = new NgramYearEntry(year, occurencesCount, distinctBooksCount);
        }
        public int BytesCount { get { return bytesCount; } }
        public byte[] Bytes { get { return bytes; } }
        public NgramYearEntry Entry { get { return entry; } }
        public void WriteTo(Stream stream) {
            stream.Write(Bytes, 0, BytesCount);
        }
    }
}
