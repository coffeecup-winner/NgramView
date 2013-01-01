using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NgramView.Controls.Chart.Data;
using NgramView.Controls.ViewInfo;

namespace NgramView.Controls.Chart.ViewInfo {
    public class SeriesViewInfo : BaseViewInfo {
        readonly string name;
        readonly Series series;

        public SeriesViewInfo(BaseViewInfo parent, Series series)
            : base(parent) {
            this.series = series;
        }
        protected new ChartViewInfo Root { get { return (ChartViewInfo)base.Root; } }
        public string Name { get { return name; } }
        protected override void OnDraw(PaintEventArgs e) {
            var dataPoints = series.Values.Zip(Root.Series.Arguments.Values, (v, y) => new Point(y, v));
            Point[] chartPoints = dataPoints.Select(p => new Point(
                ToHorz(LinearScale(p.X, Root.Series.Arguments.Min, Root.Series.Arguments.Max)),
                ToVert(LogarithmicScale(p.Y, Root.Series.ValueLogMin, Root.Series.ValueLogMax))
            )).ToArray();
            using(Pen pen = new Pen(Color.CadetBlue, 1.5f)) {
                var oldSmoothingMode = e.Graphics.SmoothingMode;
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.DrawLines(pen, chartPoints);
                e.Graphics.SmoothingMode = oldSmoothingMode;
            }
        }
        double LinearScale(int value, int min, int max) {
            return (double)(value - min) / (max - min);
        }
        double LogarithmicScale(int value, double logMin, double logMax) {
            if(value == 0) return 0;
            return (Math.Log10(value) - logMin) / (logMax - logMin);
        }
        int ToHorz(double scaledValue) {
            return (int)(Left + scaledValue * Width);
        }
        int ToVert(double scaledValue) {
            return (int)(Bottom - scaledValue * Height);
        }
    }
}
