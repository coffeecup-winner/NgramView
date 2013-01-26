#include "stdafx.h"

#include "optimized_ngram_data_entry.h"
#include "optimized_ngram_year_entry.h"

#include <boost\assert.hpp>

namespace NgramDataFormat {
    BYTE* OptimizedNgramDataEntry::PackYears(int* bytesCount) {
        //int[] years = Entry.YearEntries.Select(e => e.Year).ToArray();
        //int yearBits = 9 + (MaxYear - years[0]);
        //int yearBytes = (int)Math.Ceiling((double)yearBits / 8);
        //byte[] buffer = new byte[yearBytes];
        //byte[] firstYearBytes = BitConverter.GetBytes((short)(years[0] - MinYear));
        //buffer[0] = firstYearBytes[0];
        //buffer[1] = firstYearBytes[1];
        //Debug.Assert((buffer[1] & 0xFE) == 0x00); //no more than 9 bits in a year
        //for(int i = 1; i < years.Length; i++) {
        //    int currentBit = 8 + (years[i] - years[0]);
        //    buffer[currentBit / 8] |= OneBitBytes[currentBit % 8];
        //}
        //return buffer;
        auto yearEntries = this->entry.GetYearEntries();
        int firstYear = yearEntries->at(0).year;
        int yearBits = 9 + (MAX_YEAR - firstYear);
        *bytesCount = MIN_BYTES(yearBits);
        BYTE* buffer = (BYTE*)calloc(*bytesCount, sizeof(BYTE));
        *((WORD*)buffer) = (WORD)(firstYear - MIN_YEAR);
        BOOST_ASSERT((buffer[1] & 0xFE) == 0x00); //no more than 9 bits in a year
        for(auto item = ++yearEntries->begin(); item != yearEntries->end(); ++item) {
            int currentBit = 8 + (item->year - firstYear);
            buffer[currentBit / 8] |= 1 << (currentBit % 8);
        }
        return buffer;
    }
    void OptimizedNgramDataEntry::Pack() {
        /*var optimizedYearEntries = Entry.YearEntries.Select(e => new OptimizedNgramYearEntry(e)).ToList();
        int bytesCount = yearBytes.Length + optimizedYearEntries.Select(e => e.BytesCount).Sum();
        this.bytes = new byte[bytesCount];
        Array.Copy(yearBytes, 0, this.bytes, 0, yearBytes.Length);
        int index = yearBytes.Length;
        foreach(var entry in optimizedYearEntries) {
            Array.Copy(entry.Bytes, 0, this.bytes, index, entry.BytesCount);
            index += entry.BytesCount;
        }*/
        int bytesCount;
        BYTE* bytes = PackYears(&bytesCount);
        auto yearEntries = this->entry.GetYearEntries();
        int size = 1024;
        bytes = (BYTE*)realloc(bytes, size);
        for(auto item = yearEntries->begin(); item != yearEntries->end(); ++item) {
            OptimizedNgramYearEntry entry(*item);
            int count = entry.GetBytesCount();
            if(size < bytesCount + count) {
                size += size >> 2;
                bytes = (BYTE*)realloc(bytes, size);
            }
            memcpy((void*)&bytes[bytesCount], entry.GetBytes(), count);
            bytesCount += count;
        }
        bytes = (BYTE*)realloc(bytes, bytesCount);
        this->bytes = bytes;
        this->bytesCount = bytesCount;
    }
}