#include "stdafx.h"

#include "..\ngram_data_entry.h"
#include "..\optimized_ngram_year_entry.h"
#include "..\optimized_ngram_data_entry.h"

namespace NgramDataFormat {
    BOOST_AUTO_TEST_CASE(CompressionTest1Byte) {
        NgramYearEntry entry(2008, 6, 3);
        OptimizedNgramYearEntry optimizedEntry(entry);
        BOOST_CHECK_EQUAL(1, optimizedEntry.GetBytesCount());
        BOOST_CHECK_EQUAL(0xb6, optimizedEntry.GetBytes()[0]);
    }
    BOOST_AUTO_TEST_CASE(CompressionTest6Bytes) {
        NgramYearEntry entry(2004, 122446, 7330);
        OptimizedNgramYearEntry optimizedEntry(entry);
        BOOST_CHECK_EQUAL(6, optimizedEntry.GetBytesCount());
        BOOST_CHECK_EQUAL(0x22, optimizedEntry.GetBytes()[0]);
        BOOST_CHECK_EQUAL(0xe5, optimizedEntry.GetBytes()[1]);
        BOOST_CHECK_EQUAL(0x00, optimizedEntry.GetBytes()[2]);
        BOOST_CHECK_EQUAL(0x4e, optimizedEntry.GetBytes()[3]);
        BOOST_CHECK_EQUAL(0xde, optimizedEntry.GetBytes()[4]);
        BOOST_CHECK_EQUAL(0x01, optimizedEntry.GetBytes()[5]);
    }
    BOOST_AUTO_TEST_CASE(CompressionTestEntry) {
        NgramDataEntry entry;
        entry.Add("abc\t2004\t122446\t7330");
        entry.Add("abc\t2008\t6\t3");
        OptimizedNgramDataEntry optimizedEntry(entry);
        BOOST_CHECK_EQUAL(2 + 1 + 6, optimizedEntry.GetBytesCount());
        BOOST_CHECK_EQUAL(0xf8, optimizedEntry.GetBytes()[0]);
        BOOST_CHECK_EQUAL(0x11, optimizedEntry.GetBytes()[1]);
        BOOST_CHECK_EQUAL(0x22, optimizedEntry.GetBytes()[2]);
        BOOST_CHECK_EQUAL(0xe5, optimizedEntry.GetBytes()[3]);
        BOOST_CHECK_EQUAL(0x00, optimizedEntry.GetBytes()[4]);
        BOOST_CHECK_EQUAL(0x4e, optimizedEntry.GetBytes()[5]);
        BOOST_CHECK_EQUAL(0xde, optimizedEntry.GetBytes()[6]);
        BOOST_CHECK_EQUAL(0x01, optimizedEntry.GetBytes()[7]);
        BOOST_CHECK_EQUAL(0xb6, optimizedEntry.GetBytes()[8]);
    }
}
