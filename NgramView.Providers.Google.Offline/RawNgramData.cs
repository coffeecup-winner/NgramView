using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NgramView.Data;

namespace NgramView.Providers.Google.Offline {
    class RawNgramData : BaseNgramData {
        public RawNgramData(string filepath)
            : base(filepath) {
        }
        public override Data.NgramDataEntry Query(string ngram) {
            using(FileStream stream = File.OpenRead(FilePath)) {
                GZipStream gzStream = new GZipStream(stream, CompressionMode.Decompress);
                StreamReader reader = new StreamReader(gzStream);
                return FindAndReadEntry(reader, ngram);
            }
        }
        NgramDataEntry FindAndReadEntry(StreamReader reader, string ngram) {
            string line = FindEntry(reader, ngram);
            return ReadEntry(reader, ngram, ref line);
        }
        string FindEntry(StreamReader reader, string ngram) {
            string line;
            do {
                line = reader.ReadLine();
            } while(!line.StartsWith(ngram + '\t'));
            return line;
        }
    }
}
