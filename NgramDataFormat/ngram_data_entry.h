#pragma once

#include "stdafx.h"
#include <string>
#include <vector>

using namespace std;

namespace NgramDataFormat {
    struct NgramYearEntry {
        int year;
        int occurences_count;
        int books_count;

        NgramYearEntry(int year, int occurences_count, int books_count) :
            year(year),
            occurences_count(occurences_count),
            books_count(books_count) {
        }
        bool operator < (const NgramYearEntry entry) const {
            return year < entry.year;
        }
    };

    class NgramDataEntry {
        string ngram;
        vector<NgramYearEntry> year_entries;

    public:
        NgramDataEntry() : ngram(), year_entries() { }
        vector<NgramYearEntry> const * GetYearEntries() const { return &year_entries; }
        bool Add(string line);
        void Sort();
    };
}