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
                using(Stream outStream = File.Create(Path.ChangeExtension(FilePath, ".dat"))) {
                    string line = null;
                    while(!reader.EndOfStream) {
                        var dataEntry = ReadEntry(reader, ref line);
                        Debug.Assert(outStream.Position <= uint.MaxValue);
                        var prevHeaderEntry = header.Add(dataEntry.Ngram, (uint)outStream.Position);
                        new OptimizedNgramDataEntry(dataEntry).WriteTo(outStream);
                        //foreach(var yearEntry in dataEntry.YearEntries)
                        //    WriteEntry(outStream, yearEntry);
                        prevHeaderEntry.EndOffset = (uint)outStream.Position;
                    }
                }
                header.Build();
                using(Stream headerStream = File.Create(Path.ChangeExtension(FilePath, ".idx")))
                    header.WriteTo(headerStream);
            }
        }
        public override NgramDataEntry Query(string ngram) {
            OptimizedNgramHeaderEntry headerEntry;
            using(FileStream stream = File.OpenRead(FilePath)) {
                OptimizedNgramHeader header = new OptimizedNgramHeader(stream);
                headerEntry = header.Find(ngram, stream);
            }
            using(FileStream stream = File.OpenRead(FilePath.Replace(".idx", ".dat"))) {
                stream.Seek(headerEntry.Offset, SeekOrigin.Begin);
                return new OptimizedNgramDataEntry(ngram, stream, (int)(headerEntry.EndOffset - headerEntry.Offset)).Entry;
            }
        }
        void WriteEntry(Stream stream, NgramYearEntry yearEntry) {
            new OptimizedNgramYearEntry(yearEntry).WriteTo(stream);
        }
    }
}