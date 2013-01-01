using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace NgramView.Controls.Chart.Data {
    public class Series {
        readonly int[] values;
        readonly int min, max;
        readonly double logMin, logMax;
 
        public Series(IEnumerable<DataRow> rows, string name) {
            this.values = rows.Select(r => (int)r[name]).ToArray();
            min = int.MaxValue;
            max = int.MinValue;
            foreach(var item in Values) {
                if(item < min)
                    min = item;
                if(item > max)
                    max = item;
            }
            this.logMin = Min == 0 ? 0 : Math.Log10(Min);
            this.logMax = Math.Log10(Max);
        }
        public IEnumerable<int> Values { get { return values; } }
        public int Min { get { return min; } }
        public int Max { get { return max; } }
        public double LogMin { get { return logMin; } }
        public double LogMax { get { return logMax; } }
    }
}
