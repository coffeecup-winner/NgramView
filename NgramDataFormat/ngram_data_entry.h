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
    };

    class NgramDataEntry {
        string ngram;
        vector<NgramYearEntry> year_entries;

    public:
        NgramDataEntry() : ngram(), year_entries() { }
        bool Add(string line);
    };
}