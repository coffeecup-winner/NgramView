#pragma once

#include "stdafx.h"

#include "ngram_data_entry.h"

namespace NgramDataFormat {
    class OptimizedNgramDataEntry {
        static const int MAX_YEAR = 2008;
        static const int MIN_YEAR = 1500;
        NgramDataEntry entry;
        BYTE* bytes;
        int bytesCount;

        BYTE* PackYears(int* bytesCount);
        void Pack();

    public:
        OptimizedNgramDataEntry(NgramDataEntry entry)
            : entry(entry) {
            Pack();
        }
        BYTE* GetBytes() const { return bytes; }
        int GetBytesCount() const { return bytesCount; }
        ~OptimizedNgramDataEntry() {
            free(bytes);
        }
    };
}