using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NgramView.Controls.Chart.ViewInfo;

namespace NgramView.Controls.Chart {
    public class ChartControl : Control {
        readonly ChartViewInfo viewInfo;
        DataTable table;
        int argumentDelta;

        public ChartControl() {
            this.viewInfo = new ChartViewInfo(this);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }
        protected ChartViewInfo ViewInfo { get { return viewInfo; } }
        protected override void OnPaint(PaintEventArgs e) {
            ViewInfo.Draw(e);
        }
        public DataTable Table { get { return table; } }
        public int ArgumentDelta { get { return argumentDelta; } set { argumentDelta = value; } }
        public void ShowData(DataTable table) {
            this.table = table;
            ViewInfo.Update();
            Refresh();
        }
    }
}
