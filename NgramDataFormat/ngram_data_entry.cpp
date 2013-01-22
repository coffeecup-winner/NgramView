#include "stdafx.h"
#include <stdlib.h>

#include "ngram_data_entry.h"

#include <boost\algorithm\string.hpp>

namespace NgramDataFormat {
    bool NgramDataEntry::Add(string line) {
        vector<string> parts;
        boost::split(parts, line, boost::is_any_of("\t"));
        if(!this->ngram.empty() && this->ngram != parts[0])
            return false;
        this->ngram = parts[0];
        this->year_entries.push_back(NgramYearEntry(atoi(parts[1].c_str()), atoi(parts[2].c_str()), atoi(parts[3].c_str())));
        return true;
    }
}