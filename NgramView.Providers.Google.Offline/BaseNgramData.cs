using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NgramView.Data;

namespace NgramView.Providers.Google.Offline {
    abstract class BaseNgramData : INgramProvider {
        readonly string filepath;

        public BaseNgramData(string filepath) {
            this.filepath = filepath;
        }
        public string FilePath { get { return filepath; } }
        public abstract NgramDataEntry Query(string ngram);
        protected NgramDataEntry ReadEntry(StreamReader reader, ref string line) {
            if(line == null)
                line = reader.ReadLine();
            return ReadEntry(reader, line.Split('\t')[0], ref line);
        }
        protected NgramDataEntry ReadEntry(StreamReader reader, string ngram, ref string line) {
            NgramDataEntry dataEntry = new NgramDataEntry(ngram);
            do {
                string[] parts = line.Split('\t');
                dataEntry.Add(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                if(reader.EndOfStream) {
                    line = null;
                    break;
                }
                line = reader.ReadLine();
            } while(line.StartsWith(ngram));
            return dataEntry;
        }
    }
}
