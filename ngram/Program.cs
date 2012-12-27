using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NgramView.Providers.Google.Offline.Grabber;

namespace ngram {
    class Program {
        static void Main(string[] args) {
            NgramDataGrabber.DownloadTo("E:\\ngramdata\\", new GrabberCallbackObject());
            Console.Beep();
            Console.ReadKey(true);
        }
    }
}
