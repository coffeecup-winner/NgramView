using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NgramView.Data;

namespace NgramView.Providers.Google.Offline {
    class OptimizedNgramData : BaseNgramData {
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

    class OptimizedNgramDataEntry {
        const byte FlagAddByte = 0x80;
        const byte FlagAddTwoBytes = 0x40;
        const byte ClearFlagsMask = 0x01;
        readonly int bytesCount;
        readonly byte[] bytes;
        readonly NgramYearEntry entry;

        public OptimizedNgramDataEntry(NgramYearEntry entry) {
            this.entry = entry;
            int minBytesForOccurencesCount = (int)Math.Ceiling(Math.Log(entry.OccurencesCount + 1, 2) / 8);
            int minBytesForBooksCount = (int)Math.Ceiling(Math.Log(entry.DistinctBooksCount + 1, 2) / 8);
            int minBytes = Math.Max(minBytesForOccurencesCount, minBytesForBooksCount);
            this.bytesCount = 2 + minBytes * 2;
            this.bytes = new byte[BytesCount];
            byte[] buffer = BitConverter.GetBytes((short)(entry.Year - 1500)); //min year is 1500, max year is 2008, 9 bits is enough
            bytes[0] = buffer[1];
            bytes[1] = buffer[0];
            if(minBytes % 2 == 0)
                bytes[0] |= FlagAddByte;
            if(minBytes >= 3)
                bytes[0] |= FlagAddTwoBytes;
            //first two bytes are: ffxx xxxy yyyy yyyy
            //f is flag
            //x is unused
            //y is year value
            buffer = BitConverter.GetBytes(entry.OccurencesCount);
            for(int i = 0; i < minBytes; i++)
                bytes[2 + i] = buffer[i];
            buffer = BitConverter.GetBytes(entry.DistinctBooksCount);
            for(int i = 0; i < minBytes; i++)
                bytes[2 + minBytes + i] = buffer[i];
        }
        public OptimizedNgramDataEntry(Stream stream) {
            byte[] firstBytes = new byte[2];
            int check = stream.Read(firstBytes, 0, 2);
            System.Diagnostics.Debug.Assert(2 == check);
            int minBytes = 1;
            if((firstBytes[0] & FlagAddByte) == FlagAddByte)
                minBytes++;
            if((firstBytes[0] & FlagAddTwoBytes) == FlagAddTwoBytes)
                minBytes += 2;
            this.bytesCount = 2 + minBytes * 2;
            this.bytes = new byte[BytesCount];
            bytes[1] = (byte)(firstBytes[0] & ClearFlagsMask);
            bytes[0] = firstBytes[1];
            check = stream.Read(bytes, 2, minBytes * 2);
            int year = BitConverter.ToInt16(bytes, 0) + 1500;
            int occurencesCount = 0;
            for(int i = 0; i < minBytes; i++)
                occurencesCount = (occurencesCount << 8) | bytes[2 + i];
            int distinctBooksCount = 0;
            for(int i = 0; i < minBytes; i++)
                distinctBooksCount = (distinctBooksCount << 8) | bytes[2 + minBytes + i];
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
