using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NgramView.Providers;
using NgramView.Providers.Google.Offline;
using NgramView.Providers.Google.Offline.Grabber;
using System.Runtime.InteropServices;

namespace ngram {
    class Program {
        static void Main(string[] args) {
            args = new string[] { "-e", "systemcontrolled_VERB" };
            //args = new string[] { "-u", "1", "s" };
            if(args.Length == 2 && args[0] == "-d") {
                NgramDataGrabber.DownloadTo(args[1], new GrabberCallbackObject());
            } else if(args.Length == 2 && args[0] == "-e") {
                INgramProvider ngrams = new OfflineGoogleNgramProvider("E:\\ngramdata");
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                //var dataEntry = ngrams.Query(args[1]);
                NgramDataFormat.Optimize(@"E:\ngramdata\googlebooks-eng-all-1gram-20120701-s.gz");
                sw.Stop();
                //Console.WriteLine("Ngram: " + dataEntry.Ngram);
                //foreach(var entry in dataEntry.YearEntries)
                //    Console.WriteLine("  Year: {0}\tTotal: {1}\tBooks: {2}", entry.Year, entry.OccurencesCount, entry.DistinctBooksCount);
                Console.WriteLine("Fetched in " + ((double)sw.ElapsedMilliseconds / 1000).ToString("#.###") + " seconds.");
            } else if(args.Length == 3 && args[0] == "-u") {
                OfflineGoogleNgramProvider ngrams = new OfflineGoogleNgramProvider("E:\\ngramdata");
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                ngrams.Optimize(args[1], args[2]);
                sw.Stop();
                Console.WriteLine("Optimized in " + ((double)sw.ElapsedMilliseconds / 1000).ToString("#.###") + " seconds.");
            }
            Console.Beep();
            Console.ReadKey(true);
        }
    }

    class NgramDataFormat {
        [DllImport("NgramDataFormat.dll")]
        public static extern int Optimize(string filename);
    }
}
