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
         * Short bitmap fbbb oooy | yyyy yyyy
         * Bitmap: fffb bbbb | <additional books bytes> | yyyy yyyy | yooo oooo | <additional occurences bytes>
         * In ~94% of cases (1gram, s) the format will be 3 bytes: ffbb bbbb | yyyy yyyy | yooo oooo
         * Min year is 1500, max year is 2008, 9 bits is enough
         * f is flags
         * b is books count
         * y is year
         * o is occurences count
         */
        const byte FlagShortEntry = 0x80;
        const byte FlagAddByte = 0x40;
        const byte FlagAddTwoBytes = 0x20;
        const byte ClearFlagsMask = 0x1F;
        const byte ClearHighBitMask = 0x7F;
        const byte Clear5HighBitsMask = 0x07;
        const byte Clear7HighBitsMask = 0x01;
        const int MinYear = 1500;
        readonly NgramYearEntry entry;
        int bytesCount;
        byte[] bytes;

        public OptimizedNgramDataEntry(NgramYearEntry entry) {
            this.entry = entry;
            int minBitsForOccurencesCount = (int)Math.Ceiling(Math.Log(entry.OccurencesCount + 1, 2));
            int minBitsForBooksCount = (int)Math.Ceiling(Math.Log(entry.DistinctBooksCount + 1, 2));
            Debug.Assert(minBitsForBooksCount <= 29);
            Debug.Assert(minBitsForOccurencesCount <= 31);
            if(minBitsForOccurencesCount <= 3 && minBitsForBooksCount <= 3)
                PackShort(entry);
            else
                PackNormal(entry, (int)Math.Ceiling((double)Math.Max(minBitsForOccurencesCount, minBitsForBooksCount) / 8));
        }
        public OptimizedNgramDataEntry(Stream stream) {
            byte[] firstByte = new byte[1];
            int check = stream.Read(firstByte, 0, 1);
            Debug.Assert(1 == check);
            if((firstByte[0] & FlagShortEntry) == FlagShortEntry)
                this.entry = UnpackShort(stream, firstByte[0]);
            else
                this.entry = UnpackNormal(stream, firstByte[0]);
        }
        void PackShort(NgramYearEntry entry) {
            this.bytesCount = 2;
            this.bytes = new byte[BytesCount];
            byte[] buffer = BitConverter.GetBytes(entry.DistinctBooksCount);
            Bytes[0] = FlagShortEntry;
            Bytes[0] |= (byte)(buffer[0] << 4);
            buffer = BitConverter.GetBytes(entry.OccurencesCount);
            Bytes[0] |= (byte)(buffer[0] << 1);
            buffer = BitConverter.GetBytes((short)(entry.Year - MinYear));
            Bytes[0] |= buffer[1];
            Bytes[1] = buffer[0];
        }
        void PackNormal(NgramYearEntry entry, int minBytes) {
            this.bytesCount = 1 + minBytes * 2;
            this.bytes = new byte[BytesCount];
            byte[] buffer = BitConverter.GetBytes(entry.DistinctBooksCount);
            bytes[0] = (byte)(buffer[0] & ClearFlagsMask);
            if(minBytes % 2 == 0)
                bytes[0] |= FlagAddByte;
            if(minBytes >= 3)
                bytes[0] |= FlagAddTwoBytes;
            for(int i = 1; i < minBytes; i++)
                bytes[i] = (byte)((byte)(buffer[i - 1] >> 5) | (buffer[i] << 3));
            buffer = BitConverter.GetBytes((short)(entry.Year - MinYear));
            bytes[minBytes] = buffer[0];
            byte highYearBit = (byte)(buffer[1] << 7);
            buffer = BitConverter.GetBytes(entry.OccurencesCount);
            bytes[minBytes + 1] = (byte)((buffer[0] & ClearHighBitMask) | highYearBit);
            for(int i = 1; i < minBytes; i++)
                bytes[minBytes + 1 + i] = (byte)((byte)(buffer[i - 1] >> 7) | (buffer[i] << 1));
        }
        NgramYearEntry UnpackShort(Stream stream, byte firstByte) {
            this.bytesCount = 2;
            this.bytes = new byte[BytesCount];
            this.bytes[0] = firstByte;
            byte[] buffer = new byte[sizeof(int)];
            firstByte &= ClearHighBitMask;
            buffer[0] = (byte)(firstByte >> 4);
            int distinctBooksCount = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[sizeof(int)];
            buffer[0] = (byte)((firstByte >> 1) & Clear5HighBitsMask);
            int occurencesCount = BitConverter.ToInt32(buffer, 0);
            int check = stream.Read(Bytes, 1, 1);
            Debug.Assert(1 == check);
            buffer = new byte[sizeof(short)];
            buffer[1] = (byte)(Bytes[0] & Clear7HighBitsMask);
            buffer[0] = Bytes[1];
            int year = BitConverter.ToInt16(buffer, 0) + MinYear;
            return new NgramYearEntry(year, occurencesCount, distinctBooksCount);
        }
        NgramYearEntry UnpackNormal(Stream stream, byte firstByte) {
            int minBytes = 1;
            if((firstByte & FlagAddByte) == FlagAddByte)
                minBytes++;
            if((firstByte & FlagAddTwoBytes) == FlagAddTwoBytes)
                minBytes += 2;
            this.bytesCount = 1 + minBytes * 2;
            this.bytes = new byte[BytesCount];
            bytes[0] = firstByte;
            int check = stream.Read(bytes, 1, BytesCount - 1);
            Debug.Assert(BytesCount - 1 == check);
            byte[] buffer = new byte[sizeof(int)];
            buffer[0] = (byte)(bytes[0] & ClearFlagsMask);
            for(int i = 1; i < minBytes; i++) {
                buffer[i - 1] |= (byte)(bytes[i] << 5);
                buffer[i] = (byte)((bytes[i] >> 3) & ClearFlagsMask);
            }
            int distinctBooksCount = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[sizeof(short)];
            buffer[0] = bytes[minBytes];
            buffer[1] = (byte)(bytes[minBytes + 1] >> 7);
            int year = BitConverter.ToInt16(buffer, 0) + MinYear;
            buffer = new byte[sizeof(int)];
            buffer[0] = (byte)(bytes[minBytes + 1] & ClearHighBitMask);
            for(int i = 1; i < minBytes; i++) {
                buffer[i - 1] |= (byte)(bytes[minBytes + 1 + i] << 7);
                buffer[i] = (byte)((bytes[minBytes + 1 + i] >> 1) & ClearHighBitMask);
            }
            int occurencesCount = BitConverter.ToInt32(buffer, 0);
            return new NgramYearEntry(year, occurencesCount, distinctBooksCount);
        }
        public int BytesCount { get { return bytesCount; } }
        public byte[] Bytes { get { return bytes; } }
        public NgramYearEntry Entry { get { return entry; } }
        public void WriteTo(Stream stream) {
            stream.Write(Bytes, 0, BytesCount);
        }
    }
}
