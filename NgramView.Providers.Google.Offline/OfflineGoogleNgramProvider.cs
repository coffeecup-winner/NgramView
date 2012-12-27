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
            NgramDataEntry dataEntry = null;
            string filename = FindFileName(ngram);
#warning Move the next call to a separate class
            using(FileStream stream = File.OpenRead(Path.Combine(dataFolder, filename))) {
                GZipStream gzStream = new GZipStream(stream, CompressionMode.Decompress);
                StreamReader reader = new StreamReader(gzStream);
                return FindAndReadEntry(reader, ngram);
            }
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
                    while(!reader.EndOfStream) {
                        var dataEntry = ReadEntry(reader);
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
        static NgramDataEntry ReadEntry(StreamReader reader) {
            string line = reader.ReadLine();
            return ReadEntry(reader, line.Split('\t')[0], line);
        }
        static NgramDataEntry FindAndReadEntry(StreamReader reader, string ngram) {
            return ReadEntry(reader, ngram, FindEntry(reader, ngram));
        }
        static NgramDataEntry ReadEntry(StreamReader reader, string ngram, string line) {
            NgramDataEntry dataEntry = new NgramDataEntry(ngram);
            do {
                string[] parts = line.Split('\t');
                dataEntry.Add(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                line = reader.ReadLine();
            } while(!reader.EndOfStream && line.StartsWith(ngram));
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
