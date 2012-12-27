using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NgramView.Data;

namespace NgramView.Providers {
    public interface INgramProvider {
        NgramDataEntry Query(string ngram);
        void Optimize(string type, string name);
    }
}
