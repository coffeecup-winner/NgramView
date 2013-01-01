using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NgramView.Controls.ViewInfo;

namespace NgramView.Controls.Chart.ViewInfo {
    public class SeriesViewInfo : BaseViewInfo {
        readonly string name;

        public SeriesViewInfo(BaseViewInfo parent, string name)
            : base(parent) {
            this.name = name;
        }
        protected new ChartViewInfo Root { get { return (ChartViewInfo)base.Root; } }
        public string Name { get { return name; } }
        protected override void OnDraw(PaintEventArgs e) {
            var dataPoints = Root.Owner.Table.Rows.Cast<DataRow>().Select(r => new Point((int)r[0], (int)r[name])).OrderBy(p => p.X);
            Point min, max;
            CalculateDataBounds(dataPoints, out min, out max);
            Point[] chartPoints = dataPoints.Select(p => new Point(ToHorz(LinearScale(p.X, min.X, max.X)), ToVert(LogarithmicScale(p.Y, min.Y, max.Y)))).ToArray();
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
        double LogarithmicScale(int value, int min, int max) {
            if(value == 0) return 0;
            double logMin = min == 0 ? 0 : Math.Log10(min);
            return (Math.Log10(value) - logMin) / (Math.Log10(max) - logMin);
        }
        int ToHorz(double scaledValue) {
            return (int)(Left + scaledValue * Width);
        }
        int ToVert(double scaledValue) {
            return (int)(Bottom - scaledValue * Height);
        }
        void CalculateDataBounds(IEnumerable<Point> data, out Point min, out Point max) {
            int xMin = int.MaxValue, xMax = int.MinValue,
                yMin = int.MaxValue, yMax = int.MinValue;
            foreach(var point in data) {
                if(point.X < xMin)
                    xMin = point.X;
                if(point.Y > xMax)
                    xMax = point.X;
                if(point.Y < yMin)
                    yMin = point.Y;
                if(point.Y > yMax)
                    yMax = point.Y;
            }
            min = new Point(xMin, yMin);
            max = new Point(xMax, yMax);
        }
        List<int> GetData() {
            return Root.Owner.Table.Rows.Cast<DataRow>().Select(r => r[name]).Cast<int>().OrderBy(_ => _).ToList();
        }
    }
}
