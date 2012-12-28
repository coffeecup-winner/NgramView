using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NgramView.Data;
using NgramView.Providers;
using NgramView.Providers.Google.Offline.Grabber;

namespace NgramView.Providers.Google.Offline {
    public class OfflineGoogleNgramProvider : INgramProvider {
        readonly string dataFolder;

        public OfflineGoogleNgramProvider(string dataFolder) {
            this.dataFolder = dataFolder;
        }
        public NgramDataEntry Query(string ngram) {
            string filename = FindFileName(ngram);
            if(File.Exists(Path.Combine(dataFolder, Path.ChangeExtension(filename, ".idx.gz"))))
                return QueryOptimized(ngram, Path.ChangeExtension(filename, ".idx.gz"));
            return QueryRaw(ngram, filename);
        }
        NgramDataEntry QueryRaw(string ngram, string filename) {
#warning Move the next call to a separate class
            using(FileStream stream = File.OpenRead(Path.Combine(dataFolder, filename))) {
                GZipStream gzStream = new GZipStream(stream, CompressionMode.Decompress);
                StreamReader reader = new StreamReader(gzStream);
                return FindAndReadEntry(reader, ngram);
            }
        }
        NgramDataEntry QueryOptimized(string ngram, string filename) {
            NgramDataEntry dataEntry = new NgramDataEntry(ngram);
            int index = 0;
            using(FileStream stream = File.OpenRead(Path.Combine(dataFolder, filename))) {
                GZipStream gzStream = new GZipStream(stream, CompressionMode.Decompress);
                StreamReader reader = new StreamReader(gzStream);
                while(!reader.EndOfStream && reader.ReadLine() != ngram)
                    index++;
            }
            using(FileStream stream = File.OpenRead(Path.Combine(dataFolder, filename.Replace(".idx.gz", ".dat")))) {
                const int count = 3 + 2 + 2 + 2;
                byte[] bytes = new byte[count];
                int entryIndex;
                do {
                    int check = stream.Read(bytes, 0, count);
                    System.Diagnostics.Debug.Assert(count == check);
                    entryIndex = (bytes[2] << 16) | (bytes[1] << 8) | bytes[0];
                } while(entryIndex != index);
                do {
                    int year = (bytes[4] << 8) | bytes[3];
                    int occurencesCount = (bytes[6] << 8) | bytes[5];
                    int distinctBooksCount = (bytes[8] << 8) | bytes[7];
                    dataEntry.Add(year, occurencesCount, distinctBooksCount);
                    
                    int check = stream.Read(bytes, 0, count);
                    System.Diagnostics.Debug.Assert(count == check || count == 0);
                    entryIndex = (bytes[2] << 16) | (bytes[1] << 8) | bytes[0];
                } while(entryIndex == index && count != 0);
            }
            return dataEntry;
        }
        public void Optimize(string type, string name) {
            string filename = NgramDataGrabber.GetNgramFullFileName(type, name);
#warning Move the next call to a separate class
            using(FileStream stream = File.OpenRead(Path.Combine(dataFolder, filename))) {
                GZipStream gzStream = new GZipStream(stream, CompressionMode.Decompress);
                StreamReader reader = new StreamReader(gzStream);
                List<string> ngrams = new List<string>();
                //using(GZipStream outStream = new GZipStream(File.Create(Path.Combine(dataFolder, Path.ChangeExtension(filename, ".dat.gz"))), CompressionMode.Compress)) {
                using(Stream outStream = File.Create(Path.Combine(dataFolder, Path.ChangeExtension(filename, ".dat")))) {
                    string line = null;
                    while(!reader.EndOfStream) {
                        var dataEntry = ReadEntry(reader, ref line);
                        int index = ngrams.Count;
                        ngrams.Add(dataEntry.Ngram);
                        foreach(var yearEntry in dataEntry.YearEntries) {
                            const int bytesCount = 3 + 2 + 2 + 2;
                            byte[] bytes = new byte[bytesCount];
                            byte[] buffer = BitConverter.GetBytes(index);
                            bytes[0] = buffer[0];
                            bytes[1] = buffer[1];
                            bytes[2] = buffer[2];
                            buffer = BitConverter.GetBytes(yearEntry.Year);
                            bytes[3] = buffer[0];
                            bytes[4] = buffer[1];
                            buffer = BitConverter.GetBytes(yearEntry.OccurencesCount);
                            bytes[5] = buffer[0];
                            bytes[6] = buffer[1];
                            buffer = BitConverter.GetBytes(yearEntry.DistinctBooksCount);
                            bytes[7] = buffer[0];
                            bytes[8] = buffer[1];
                            outStream.Write(bytes, 0, bytesCount);
                        }
                    }
                }
                using(GZipStream outStream = new GZipStream(File.Create(Path.Combine(dataFolder, Path.ChangeExtension(filename, ".idx.gz"))), CompressionMode.Compress)) {
                    StreamWriter writer = new StreamWriter(outStream);
                    foreach(var ngram in ngrams)
                        writer.WriteLine(ngram);
                }
            }
        }
        static NgramDataEntry ReadEntry(StreamReader reader, ref string line) {
            if(line == null)
                line = reader.ReadLine();
            return ReadEntry(reader, line.Split('\t')[0], ref line);
        }
        static NgramDataEntry FindAndReadEntry(StreamReader reader, string ngram) {
            string line = FindEntry(reader, ngram);
            return ReadEntry(reader, ngram, ref line);
        }
        static NgramDataEntry ReadEntry(StreamReader reader, string ngram, ref string line) {
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
        static string FindEntry(StreamReader reader, string ngram) {
            string line;
            do {
                line = reader.ReadLine();
            } while(!line.StartsWith(ngram + '\t'));
            return line;
        }
        string FindFileName(string ngram) {
#warning will work only for 1gram
            return NgramDataGrabber.GetNgramFullFileName((ngram.Count(c => c == ' ') + 1).ToString(), ngram[0].ToString());
        }
    }
}
