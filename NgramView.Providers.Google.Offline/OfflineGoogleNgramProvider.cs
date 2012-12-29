using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NgramView.Data;
using NgramView.Providers;
using NgramView.Providers.Google.Offline.Grabber;
using NgramView.Providers.Google.Offline.OptimizedData;

namespace NgramView.Providers.Google.Offline {
    public class OfflineGoogleNgramProvider : INgramProvider {
        readonly string dataFolder;

        public OfflineGoogleNgramProvider(string dataFolder) {
            this.dataFolder = dataFolder;
        }
        public NgramDataEntry Query(string ngram) {
            string filename = FindFileName(ngram);
            string filepath = Path.Combine(this.dataFolder, filename);
            string optimizedFilepath = Path.ChangeExtension(filepath, "idx");
            INgramProvider data = File.Exists(optimizedFilepath) ? (INgramProvider)new OptimizedNgramData(optimizedFilepath) : new RawNgramData(filepath);
            return data.Query(ngram);
        }
        public void Optimize(string type, string name) {
            string filename = NgramDataGrabber.GetNgramFullFileName(type, name);
            OptimizedNgramData data = new OptimizedNgramData(Path.Combine(dataFolder, filename));
            data.Optimize();
        }
        string FindFileName(string ngram) {
#warning will work only for 1gram
            return NgramDataGrabber.GetNgramFullFileName((ngram.Count(c => c == ' ') + 1).ToString(), ngram[0].ToString());
        }
    }
}
