using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NgramView.Data {
    public class NgramYearEntry {
        public NgramYearEntry(int year, int occurencesCount, int distinctBooksCount) {
            this.Year = year;
            this.OccurencesCount = occurencesCount;
            this.DistinctBooksCount = distinctBooksCount;
        }
        public int Year { get; private set; }
        public int OccurencesCount { get; private set; }
        public int DistinctBooksCount { get; private set; }
    }

    public class NgramDataEntry {
        readonly List<NgramYearEntry> yearEntries = new List<NgramYearEntry>();

        public NgramDataEntry(string ngram) {
            Ngram = ngram;
            
        }
        public string Ngram { get; private set; }
        public IEnumerable<NgramYearEntry> YearEntries { get { return yearEntries; } }
        public void Add(int year, int occurencesCount, int distinctBooksCount) {
            this.yearEntries.Add(new NgramYearEntry(year, occurencesCount, distinctBooksCount));
        }
    }
}
