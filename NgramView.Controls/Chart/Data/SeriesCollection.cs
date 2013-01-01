using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace NgramView.Controls.Chart.Data {
    public class SeriesCollection {
        readonly List<Series> series = new List<Series>();
 
        public SeriesCollection(DataTable table) {
            var rows = table.Rows.Cast<DataRow>().OrderBy(r => r[0]);
            foreach (DataColumn column in table.Columns)
                series.Add(new Series(rows, column.ColumnName));
        }
        public Series Arguments { get { return series[0]; } }
        public IEnumerable<Series> Values { get { return series.Skip(1); } }
        public int ValueMin { get { return Values.Select(s => s.Min).Min(); } }
        public int ValueMax { get { return Values.Select(s => s.Max).Max(); } }
        public double ValueLogMin { get { return Values.Select(s => s.LogMin).Min(); } }
        public double ValueLogMax { get { return Values.Select(s => s.LogMax).Max(); } }
    }
}
