#include "stdafx.h"
#include <iostream>
#include <fstream>
#include <vector>

#include "ngramdata.h"
#include "ngram_data_entry.h"
#include "optimized_ngram_data_entry.h"

#include <boost\iostreams\filtering_stream.hpp>
#pragma warning(disable: 4244)
#include <boost\iostreams\filter\gzip.hpp>
#pragma warning(default: 4244)

using namespace std;
namespace io = boost::iostreams;

namespace NgramDataFormat {
    int NgramData::Optimize(const char *filename) {
        ifstream stream(filename, ios_base::in | ios_base::binary);
        if(!stream.is_open())
            return -1;

        io::filtering_istream in;
        in.push(io::gzip_decompressor());
        in.push(stream);
        string line;
        NgramDataEntry entry;
        ofstream out("F:\\test.dat", ios_base::out | ios_base::binary);
        for(; getline(in, line);) {
            if(!entry.Add(line)) {
                entry.Sort();
                OptimizedNgramDataEntry optimizedEntry(entry);
                out.write((char*)optimizedEntry.GetBytes(), optimizedEntry.GetBytesCount());
                entry = NgramDataEntry();
                entry.Add(line);
            }
        }
        return 0;
    }
}