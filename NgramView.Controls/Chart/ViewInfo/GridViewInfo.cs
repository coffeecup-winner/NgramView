using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NgramView.Controls.ViewInfo;
using System.Data;

namespace NgramView.Controls.Chart.ViewInfo {
    public class GridViewInfo : BaseViewInfo {
        const int MinHorzDelta = 50;
        int horzDelta;
        List<int> vertLineXs; 

        public GridViewInfo(BaseViewInfo parent)
            : base(parent) {
        }
        protected new ChartViewInfo Root { get { return (ChartViewInfo)base.Root; } }
        public override void Update() {
            var data = GetArguments();
            horzDelta = Math.Max(Width / data.Count(), MinHorzDelta);
            //UpdateLines();
            for(int i = 1; i < Root.Owner.Table.Columns.Count; i++) {
                SeriesViewInfo series = new SeriesViewInfo(this, Root.Owner.Table.Columns[i].ColumnName);
                series.Margin = new Padding(0, 5, 0, 17);
                Children.Add(series);
            }
        }
        List<int> GetArguments() {
            return Root.Owner.Table.Rows.Cast<DataRow>().Select(r => r[0]).Cast<int>().OrderBy(_ => _).ToList();
        }
        protected override void OnParentSizeChanged() {
            UpdateLines();
        }
        void UpdateLines() {
            if(horzDelta < MinHorzDelta) return;
            this.vertLineXs = new List<int>();
            for (int x = 0; x < Width; x += horzDelta)
                this.vertLineXs.Add(Left + x);
        }
        protected override void OnDraw(PaintEventArgs e) {
            using(Pen pen = new Pen(Color.LightGray))
            using(Font font = new Font("Verdana", 8)){
                var data = GetArguments();
                for(int i = 0; i < data.Count; i++) {
                    if(data[i] % Root.Owner.ArgumentDelta != 0)
                        continue;
                    int x = Left + i * Width / (data.Count - 1);
                    string label = data[i].ToString();
                    var size = e.Graphics.MeasureString(label, font, 100, StringFormat.GenericTypographic);
                    e.Graphics.DrawString(label, font, Brushes.Black, x - size.Width / 2 - 1, Top + Height - size.Height);
                    e.Graphics.DrawLine(pen, x, Top, x, Top + Height - size.Height - 5);
                }
            }
            using(Pen borderPen = new Pen(Color.Black)) {
#warning Fix hard-coded values
                e.Graphics.DrawLine(borderPen, Left, Bottom - 17, Right, Bottom - 17);
                e.Graphics.DrawLine(borderPen, Left, Top, Left, Bottom - 17);
            }
        }
    }
}
