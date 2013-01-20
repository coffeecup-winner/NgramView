using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NgramView.Data;

namespace NgramView.Providers.Google.Offline.OptimizedData {
    public class OptimizedNgramData : BaseNgramData {
        public OptimizedNgramData(string filepath)
            : base(filepath) {
        }
        public void Optimize() {
            using(FileStream stream = File.OpenRead(FilePath)) {
                GZipStream gzStream = new GZipStream(stream, CompressionMode.Decompress);
                StreamReader reader = new StreamReader(gzStream);
                OptimizedNgramHeader header = new OptimizedNgramHeader();
                List<List<int>> years = new List<List<int>>();
                using(Stream outStream = File.Create(Path.ChangeExtension(FilePath, ".dat"))) {
                    string line = null;
                    while(!reader.EndOfStream) {
                        var dataEntry = ReadEntry(reader, ref line);
                        Debug.Assert(outStream.Position <= uint.MaxValue);
                        //var prevHeaderEntry = header.Add(dataEntry.Ngram, (uint)outStream.Position);
                        //foreach(var yearEntry in dataEntry.YearEntries)
                        //    WriteEntry(outStream, yearEntry);
                        years.Add(dataEntry.YearEntries.Select(e => e.Year).ToList());
                        //prevHeaderEntry.EndOffset = (uint)outStream.Position;
                    }
                }
                //header.Build();
                //using(Stream headerStream = File.Create(Path.ChangeExtension(FilePath, ".idx"))) {
                //    header.WriteTo(headerStream);
                //}
                using(Stream outStream = File.Create(Path.ChangeExtension(FilePath, ".stat"))) {
                    StreamWriter writer = new StreamWriter(outStream);
                    writer.WriteLine("entries count:   " + years.Count);
                    writer.WriteLine("years count:     " + years.Select(list => list.Count).Sum());
                    writer.WriteLine("avg years count: " + years.Select(list => list.Count).Average());
                    writer.WriteLine("min year:        " + years.Select(list => list.Min()).Min());
                    writer.WriteLine("avg min year:    " + years.Select(list => list.Min()).Average());
                    writer.WriteLine("max min year:    " + years.Select(list => list.Min()).Max());
                    writer.Close();
                }
            }
        }
        public override NgramDataEntry Query(string ngram) {
            NgramDataEntry dataEntry = new NgramDataEntry(ngram);
            OptimizedNgramHeaderEntry headerEntry;
            using(FileStream stream = File.OpenRead(FilePath)) {
                OptimizedNgramHeader header = new OptimizedNgramHeader(stream);
                headerEntry = header.Find(ngram, stream);
            }
            using(FileStream stream = File.OpenRead(FilePath.Replace(".idx", ".dat"))) {
                stream.Seek(headerEntry.Offset, SeekOrigin.Begin);
                do {
                    dataEntry.Add(ReadEntry(stream));
                } while(stream.Position < stream.Length && stream.Position < headerEntry.EndOffset);
            }
            return dataEntry;
        }
        void WriteEntry(Stream stream, NgramYearEntry yearEntry) {
            new OptimizedNgramYearEntry(yearEntry).WriteTo(stream);
        }
        NgramYearEntry ReadEntry(Stream stream) {
            return new OptimizedNgramYearEntry(stream).Entry;
        }
    }
}