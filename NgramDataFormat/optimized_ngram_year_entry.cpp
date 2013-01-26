#include "stdafx.h"

#include "optimized_ngram_year_entry.h"

namespace NgramDataFormat {
    BYTE* OptimizedNgramYearEntry::PackShort(int* bytesCount) {
        /*this.bytesCount = 1;
        this.bytes = new byte[BytesCount];
        byte[] buffer = BitConverter.GetBytes(entry.DistinctBooksCount);
        Bytes[0] = FlagShortEntry;
        Bytes[0] |= (byte)(buffer[0] << 4);
        buffer = BitConverter.GetBytes(entry.OccurencesCount);
        Bytes[0] |= buffer[0];*/
        BYTE packed;
        packed = FLAG_SHORT_ENTRY;
        packed |= entry.books_count << 4;
        packed |= entry.occurences_count;
        *bytesCount = 1;
        BYTE* buffer = (BYTE*)malloc(*bytesCount);
        buffer[0] = packed;
        return buffer;
    }
    BYTE* OptimizedNgramYearEntry::PackNormal(int* bytesCount) {
        /*this.bytesCount = minBytes * 2;
        this.bytes = new byte[BytesCount];
        byte[] buffer = BitConverter.GetBytes(entry.DistinctBooksCount);
        bytes[0] = (byte)(buffer[0] & ClearFlagsMask);
        if(minBytes % 2 == 0)
            bytes[0] |= FlagAddByte;
        if(minBytes >= 3)
            bytes[0] |= FlagAddTwoBytes;
        for(int i = 1; i < minBytes; i++)
            bytes[i] = (byte)((byte)(buffer[i - 1] >> 5) | (buffer[i] << 3));
        buffer = BitConverter.GetBytes(entry.OccurencesCount);
        for(int i = 0; i < minBytes; i++)
            bytes[minBytes + i] = buffer[i];*/
        BYTE* buffer = (BYTE*)malloc(*bytesCount);
        BYTE* bookBytes = (BYTE*)&entry.books_count;
        buffer[0] = CLEAR_FLAG_BITS(bookBytes[0]);
        int minBytes = *bytesCount / 2;
        if(minBytes % 2 == 0)
            buffer[0] |= FLAG_PLUS_BYTE;
        if(minBytes >= 3)
            buffer[0] |= FLAG_PLUS_TWO_BYTES;
        for(int i = 1; i < minBytes; ++i)
            buffer[i] = (bookBytes[i - 1] >> 5) | (bookBytes[i] << 3);
        BYTE* occurencesBytes = (BYTE*)&entry.occurences_count;
        for(int i = 0; i < minBytes; ++i)
            buffer[minBytes + i] = occurencesBytes[i];
        return buffer;
    }
}