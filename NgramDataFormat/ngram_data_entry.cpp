#include "stdafx.h"
#include <stdlib.h>

#include "ngram_data_entry.h"

#include <boost\algorithm\string.hpp>

namespace NgramDataFormat {
    bool NgramDataEntry::Add(string line) {
        const char *parts[4];
        const char *const_cline = line.c_str();
        char *cline_orig = reinterpret_cast<char *>(malloc(line.size() + 1));
        memcpy(cline_orig, const_cline, line.size());
        cline_orig[line.size()] = 0;
        char *cline = cline_orig;
        parts[0] = cline;
        int index = 0;
        while(*++cline) {
            if(cline[0] == '\t') {
                cline[0] = 0;
                parts[++index] = cline + 1;
            }
        }
        if(!this->ngram.empty() && this->ngram != parts[0])
            return false;
        this->ngram = parts[0];
        this->year_entries.push_back(NgramYearEntry(atoi(parts[1]), atoi(parts[2]), atoi(parts[3])));
        free(cline_orig);
        return true;
    }
    void NgramDataEntry::Sort() {
        std::sort(this->year_entries.begin(), this->year_entries.end());
    }
}