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
        const int bytesCount = 2 + 2 + 2;
        readonly byte[] bytes = new byte[bytesCount];
        readonly NgramYearEntry entry;

        public OptimizedNgramDataEntry(NgramYearEntry entry) {
            this.entry = entry;
            byte[] buffer = BitConverter.GetBytes(entry.Year);
            bytes[0] = buffer[0];
            bytes[1] = buffer[1];
            buffer = BitConverter.GetBytes(entry.OccurencesCount);
            bytes[2] = buffer[0];
            bytes[3] = buffer[1];
            buffer = BitConverter.GetBytes(entry.DistinctBooksCount);
            bytes[4] = buffer[0];
            bytes[5] = buffer[1];
        }
        public OptimizedNgramDataEntry(Stream stream) {
            int check = stream.Read(bytes, 0, BytesCount);
            System.Diagnostics.Debug.Assert(BytesCount == check);
            int year = (bytes[1] << 8) | bytes[0];
            int occurencesCount = (bytes[3] << 8) | bytes[2];
            int distinctBooksCount = (bytes[5] << 8) | bytes[4];
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
