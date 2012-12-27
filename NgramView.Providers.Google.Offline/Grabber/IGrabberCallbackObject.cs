using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NgramView.Providers.Google.Offline.Grabber {
    public interface IGrabberCallbackObject {
        void ReportDownloadStarted(string name);
        void ReportDownloadFinished(string name);
        void ReportDownloadProgress(string name, long total);
        void ReportFileSize(string name, long size);
    }
}
