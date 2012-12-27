using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NgramView.Providers.Google.Offline.Grabber;

namespace ngram {
    class GrabberCallbackObject : IGrabberCallbackObject {
        long currentFileSize;

        public void ReportDownloadStarted(string name) {
            Console.Write(name);
        }
        public void ReportDownloadFinished(string name) {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine(new string(' ', 79));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }
        public void ReportDownloadProgress(string name, long total) {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine(new string(' ', 79));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine("Downloaded: {0}", FormatFileSize(total), FormatFileSize(currentFileSize));
        }
        public void ReportFileSize(string name, long size) {
            currentFileSize = size;
            Console.WriteLine(" (" + FormatFileSize(size) + ")");
            Console.WriteLine();
        }
        public string FormatFileSize(long size) {
            double fsize = size;
            if(fsize < 1024)
                return fsize.ToString("#.##") + "b";
            fsize /= 1024;
            if(fsize < 1024)
                return fsize.ToString("#.##") + "Kb";
            fsize /= 1024;
            if(fsize < 1024)
                return fsize.ToString("#.##") + "Mb";
            fsize /= 1024;
            return fsize.ToString("#.##") + "Gb";
        }
    }
}
