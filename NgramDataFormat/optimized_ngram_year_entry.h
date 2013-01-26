#pragma once

#include "stdafx.h"
#include <math.h>

#include "ngram_data_entry.h"

#include <boost\assert.hpp>

#define FLAG_SHORT_ENTRY 0x80
#define FLAG_PLUS_BYTE 0x40
#define FLAG_PLUS_TWO_BYTES 0x20

#define IS_SHORT_ENTRY(x) ((x) & FLAG_SHORT_ENTRY)
#define IS_PLUS_BYTE(x) ((x) & FLAG_ADD_BYTE)
#define IS_PLUS_TWO_BYTES(x) ((x) & FLAG_ADD_TWO_BYTES)

#define CLEAR_FLAG_BITS(x) ((x) & 0x1F)

#define MIN_BITS_FOR(x) ((int)ceil(log((double)x + 1)))

namespace NgramDataFormat {
    class OptimizedNgramYearEntry {
        NgramYearEntry entry;
        BYTE* bytes;
        int bytesCount;

        BYTE* PackShort(int* bytesCount);
        BYTE* PackNormal(int* bytesCount);
        static inline DWORD log2(DWORD x) {
            int result = 0;
            while (x >>= 1) ++result;
            return result;
        }
    public:
        OptimizedNgramYearEntry(NgramYearEntry entry)
            : entry(entry) {
            /*
            int minBitsForOccurencesCount = (int)Math.Ceiling(Math.Log(entry.OccurencesCount + 1, 2));
            int minBitsForBooksCount = (int)Math.Ceiling(Math.Log(entry.DistinctBooksCount + 1, 2));
            Debug.Assert(minBitsForBooksCount <= 29);
            Debug.Assert(minBitsForOccurencesCount <= 32);
            if(minBitsForOccurencesCount <= 4 && minBitsForBooksCount <= 3)
                PackShort(entry);
            else
                PackNormal(entry, (int)Math.Ceiling((double)Math.Max(minBitsForOccurencesCount, minBitsForBooksCount) / 8));*/
            int minBitsForBooksCount = log2(entry.books_count);
            int minBitsForOccurencesCount = log2(entry.occurences_count);

            BOOST_ASSERT(minBitsForBooksCount <= 29);
            BOOST_ASSERT(minBitsForOccurencesCount <= 32);
            if(minBitsForOccurencesCount <= 4 && minBitsForBooksCount <= 3)
                bytes = PackShort(&bytesCount);
            else {
                bytesCount = MIN_BYTES(max(minBitsForBooksCount + 4, minBitsForOccurencesCount + 1)) * 2;
                bytes = PackNormal(&bytesCount);
            }
        }
        BYTE* GetBytes() const { return bytes; }
        int GetBytesCount() const { return bytesCount; }
        ~OptimizedNgramYearEntry() {
            free(bytes);
        }
    };
}