using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NgramView.Controls.Chart.Data;
using NgramView.Controls.ViewInfo;
using System.Data;

namespace NgramView.Controls.Chart.ViewInfo {
    public class GridViewInfo : BaseViewInfo {
        public GridViewInfo(BaseViewInfo parent)
            : base(parent) {
        }
        protected new ChartViewInfo Root { get { return (ChartViewInfo)base.Root; } }
        public override void Update() {
            foreach(Series valueSeries in Root.Series.Values) {
                SeriesViewInfo series = new SeriesViewInfo(this, valueSeries);
                series.Margin = new Padding(0, 5, 0, 17);
                Children.Add(series);
            }
        }
        protected override void OnParentSizeChanged() {
            
        }
        protected override void OnDraw(PaintEventArgs e) {
            using(Pen pen = new Pen(Color.LightGray))
            using(Font font = new Font("Verdana", 8)){
                var data = Root.Series.Arguments.Values;
                for(int i = 0; i < data.Count(); i++) {
                    if(data.ElementAt(i) % Root.Owner.ArgumentDelta != 0)
                        continue;
                    int x = Left + i * Width / (data.Count() - 1);
                    string label = data.ElementAt(i).ToString();
                    var size = e.Graphics.MeasureString(label, font, 100, StringFormat.GenericTypographic);
                    e.Graphics.DrawString(label, font, Brushes.Black, x - size.Width / 2 - 1, Top + Height - size.Height);
                    e.Graphics.DrawLine(pen, x, Top, x, Top + Height - size.Height - 5);
                }
                if(Root.Series.Values.Count() > 0) {
                    for(int i = 0; i < Root.Series.ValueLogMax; i++) {
                        int y = ToVert(LinearScale(i, Root.Series.ValueLogMin, Root.Series.ValueLogMax)) - 17;
                        e.Graphics.DrawLine(pen, Left, y, Right, y);
                        string label = Math.Pow(10, i).ToString();
                        var size = e.Graphics.MeasureString(label, font, 100, StringFormat.GenericTypographic);
                        e.Graphics.DrawString(label, font, Brushes.Black, Left - size.Width - 5, y - size.Height / 2);
                    }
                }
            }
            using(Pen borderPen = new Pen(Color.Black)) {
#warning Fix hard-coded values
                e.Graphics.DrawLine(borderPen, Left, Bottom - 17, Right, Bottom - 17);
                e.Graphics.DrawLine(borderPen, Left, Top, Left, Bottom - 17);
            }
        }
#warning Remove duplicates
        double LinearScale(int value, double min, double max)
        {
            return (double)(value - min) / (max - min);
        }
        double LogarithmicScale(int value, double logMin, double logMax)
        {
            if (value == 0) return 0;
            return (Math.Log10(value) - logMin) / (logMax - logMin);
        }
        int ToHorz(double scaledValue)
        {
            return (int)(Left + scaledValue * Width);
        }
        int ToVert(double scaledValue)
        {
            return (int)(Bottom - scaledValue * Height);
        }
    }
}
