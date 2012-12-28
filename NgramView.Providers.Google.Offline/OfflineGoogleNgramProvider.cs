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
            int offset = 0;
            int nextOffset = -1;
            using(FileStream stream = File.OpenRead(Path.Combine(dataFolder, filename))) {
                GZipStream gzStream = new GZipStream(stream, CompressionMode.Decompress);
                StreamReader reader = new StreamReader(gzStream);
                string line;
                while(!(line = reader.ReadLine()).StartsWith(ngram + '\t') && !reader.EndOfStream) { }
                offset = int.Parse(line.Split('\t')[1]);
                line = reader.ReadLine();
                if(line != null)
                    nextOffset = int.Parse(line.Split('\t')[1]);
            }
            using(FileStream stream = File.OpenRead(Path.Combine(dataFolder, filename.Replace(".idx.gz", ".dat")))) {
                stream.Seek(offset, SeekOrigin.Begin);
                do {
                    const int count = 2 + 2 + 2;
                    byte[] bytes = new byte[count];
                    int check = stream.Read(bytes, 0, count);
                    System.Diagnostics.Debug.Assert(count == check || count == 0);
                    int year = (bytes[1] << 8) | bytes[0];
                    int occurencesCount = (bytes[3] << 8) | bytes[2];
                    int distinctBooksCount = (bytes[5] << 8) | bytes[4];
                    dataEntry.Add(year, occurencesCount, distinctBooksCount);
                } while(stream.Position < stream.Length && stream.Position < nextOffset);
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
                        ngrams.Add(dataEntry.Ngram + "\t" + outStream.Position);
                        foreach(var yearEntry in dataEntry.YearEntries) {
                            const int bytesCount = 2 + 2 + 2;
                            byte[] bytes = new byte[bytesCount];
                            byte[] buffer = BitConverter.GetBytes(yearEntry.Year);
                            bytes[0] = buffer[0];
                            bytes[1] = buffer[1];
                            buffer = BitConverter.GetBytes(yearEntry.OccurencesCount);
                            bytes[2] = buffer[0];
                            bytes[3] = buffer[1];
                            buffer = BitConverter.GetBytes(yearEntry.DistinctBooksCount);
                            bytes[4] = buffer[0];
                            bytes[5] = buffer[1];
                            outStream.Write(bytes, 0, bytesCount);
                        }
                    }
                }
                using(StreamWriter writer = new StreamWriter(new GZipStream(File.Create(Path.Combine(dataFolder, Path.ChangeExtension(filename, ".idx.gz"))), CompressionMode.Compress))) {
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
