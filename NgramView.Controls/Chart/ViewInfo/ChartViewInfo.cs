using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NgramView.Controls.ViewInfo;

namespace NgramView.Controls.Chart.ViewInfo {
    public class ChartViewInfo : TopLevelViewInfo<ChartControl> {
        public ChartViewInfo(ChartControl owner)
            :base(owner) {
        }
        protected override void OnDraw(PaintEventArgs e) {
            e.Graphics.Clear(Color.White);
            using(Pen pen = new Pen(Color.DarkGray)) {
                Rectangle bounds = new Rectangle(0, 0, Width, Height);
                bounds.Inflate(-5, -5);
                //e.Graphics.DrawRectangle(pen, bounds);
            }
        }
        public override void Update() {
            Children.Clear();
            GridViewInfo gridViewInfo = new GridViewInfo(this) {
                Margin = new Padding(30, 5, 30, 6)
            };
            Children.Add(gridViewInfo);
            gridViewInfo.Update();
        }
    }
}
