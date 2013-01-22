// NgramDataFormat.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "ngramdata.h"

namespace NgramDataFormat {
    extern "C" __declspec(dllexport) int __stdcall Optimize(const char *filename) {
         return NgramData::Optimize(filename);
    }
}