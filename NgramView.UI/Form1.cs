using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NgramView.Data;
using NgramView.Providers.Google.Offline;
using NgramView.Providers;

namespace NgramView.UI
{
    public partial class Form1 : Form {
        readonly INgramProvider ngramProvider;

        public Form1() {
            this.ngramProvider = new OfflineGoogleNgramProvider("C:\\ngramdata\\");
            InitializeComponent();
            tbQuery.Select();
        }
        protected INgramProvider NgramProvider { get { return ngramProvider; } }
        void Form1_Load(object sender, EventArgs e) {
            DataTable table = new DataTable();
            table.Columns.Add("x", typeof(int));
            for(int i = 1500; i <= 2008; i++)
                table.Rows.Add(i);
            chartControl1.ArgumentDelta = 50;
            chartControl1.ValueLogDelta = 10;
            chartControl1.ShowData(table);
        }
        void tbQuery_KeyDown(object sender, KeyEventArgs e) {
            if(e.KeyCode != Keys.Enter) return;
            var data = NgramProvider.Query(tbQuery.Text);
            chartControl1.ShowData(FillDataTable(data));
        }
        DataTable FillDataTable(NgramDataEntry data) {
            DataTable table = new DataTable();
            table.Columns.Add("years", typeof(int));
            table.Columns.Add("occurences", typeof(int));
            foreach(var entry in data.YearEntries)
                table.Rows.Add(entry.Year, entry.DistinctBooksCount);
            foreach(var year in Enumerable.Range(1500, 509).Except(data.YearEntries.Select(e => e.Year)))
                table.Rows.Add(year, 0);
            return table;
        }
    }
}
