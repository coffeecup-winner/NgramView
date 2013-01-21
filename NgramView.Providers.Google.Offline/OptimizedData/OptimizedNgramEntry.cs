using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NgramView.Data;
using System.Diagnostics;
using System.IO;

namespace NgramView.Providers.Google.Offline.OptimizedData {
    public class OptimizedNgramDataEntry {
        const int MaxYear = 2008;
        const int MinYear = 1500;
        static readonly byte[] OneBitBytes = new byte[] { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
        readonly NgramDataEntry entry;
        byte[] bytes;

        public OptimizedNgramDataEntry(NgramDataEntry entry) {
            this.entry = entry;
            Pack();
        }
        public OptimizedNgramDataEntry(string ngram, Stream stream, int length) {
            this.bytes = new byte[length];
            int check = stream.Read(this.bytes, 0, length);
            Debug.Assert(length == check);
            this.entry = Unpack(ngram);
        }
        public NgramDataEntry Entry { get { return entry; } }
        public byte[] Bytes { get { return bytes; } }
        public int BytesCount { get { return bytes.Length; } }
        void Pack() {
            byte[] yearBytes = PackYears();
            var optimizedYearEntries = Entry.YearEntries.Select(e => new OptimizedNgramYearEntry(e)).ToList();
            int bytesCount = yearBytes.Length + optimizedYearEntries.Select(e => e.BytesCount).Sum();
            this.bytes = new byte[bytesCount];
            Array.Copy(yearBytes, 0, this.bytes, 0, yearBytes.Length);
            int index = yearBytes.Length;
            foreach(var entry in optimizedYearEntries) {
                Array.Copy(entry.Bytes, 0, this.bytes, index, entry.BytesCount);
                index += entry.BytesCount;
            }
        }
        NgramDataEntry Unpack(string ngram) {
            NgramDataEntry entry = new NgramDataEntry(ngram);
            int index;
            List<int> years = UnpackYears(out index);
            index++;
            foreach(var year in years) {
                var optimizedYearEntry = new OptimizedNgramYearEntry(bytes, index);
                entry.Add(year, optimizedYearEntry.Entry.OccurencesCount, optimizedYearEntry.Entry.DistinctBooksCount);
                index += optimizedYearEntry.BytesCount;
            }
            return entry;
        }
        byte[] PackYears() {
            int[] years = Entry.YearEntries.Select(e => e.Year).ToArray();
            int yearBits = 9 + (MaxYear - years[0]);
            int yearBytes = (int)Math.Ceiling((double)yearBits / 8);
            byte[] buffer = new byte[yearBytes];
            byte[] firstYearBytes = BitConverter.GetBytes((short)(years[0] - MinYear));
            buffer[0] = firstYearBytes[0];
            buffer[1] = firstYearBytes[1];
            Debug.Assert((buffer[1] & 0xFE) == 0x00); //no more than 9 bits in a year
            for(int i = 1; i < years.Length; i++) {
                int currentBit = 8 + (years[i] - years[0]);
                buffer[currentBit / 8] |= OneBitBytes[currentBit % 8];
            }
            return buffer;
        }
        List<int> UnpackYears(out int lastYearByte) {
            List<int> years = new List<int>();
            byte[] firstBytes = new byte[2];
            firstBytes[0] = Bytes[0];
            firstBytes[1] = (byte)(Bytes[1] & 0x01);
            int firstYear = BitConverter.ToInt16(firstBytes, 0) + MinYear;
            years.Add(firstYear);
            int currentYear = firstYear;
            int currentBit = 0;
            while(++currentYear <= MaxYear) {
                currentBit = currentYear - firstYear + 8;
                if((Bytes[currentBit / 8] & OneBitBytes[currentBit % 8]) != 0)
                    years.Add(currentYear);
            }
            lastYearByte = currentBit / 8;
            return years;
        }
        public void WriteTo(Stream stream) {
            stream.Write(Bytes, 0, BytesCount);
        }
    }
}
