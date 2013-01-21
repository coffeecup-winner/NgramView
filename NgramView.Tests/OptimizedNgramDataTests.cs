using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NgramView.Data;
using NgramView.Providers.Google.Offline.OptimizedData;

namespace NgramView.Tests {
    [TestFixture]
    public class OptimizedNgramDataTests {
        [Test]
        public void CompressionTest1Byte() {
            NgramYearEntry entry = new NgramYearEntry(2008, 6, 3);
            OptimizedNgramYearEntry optimizedEntry = new OptimizedNgramYearEntry(entry);
            Assert.That(optimizedEntry.BytesCount, Is.EqualTo(1));
            Assert.That(optimizedEntry.Bytes.Length, Is.EqualTo(1));
            Assert.That(optimizedEntry.Bytes[0], Is.EqualTo(0xb6));
        }
        [Test]
        public void CompressionDecompressionTest1Byte() {
            NgramYearEntry entry = new NgramYearEntry(2008, 6, 3);
            OptimizedNgramYearEntry optimizedEntry = new OptimizedNgramYearEntry(entry);
            optimizedEntry = new OptimizedNgramYearEntry(optimizedEntry.Bytes, 0);
            Assert.That(optimizedEntry.Entry.Year, Is.EqualTo(-1));
            Assert.That(optimizedEntry.Entry.OccurencesCount, Is.EqualTo(entry.OccurencesCount));
            Assert.That(optimizedEntry.Entry.DistinctBooksCount, Is.EqualTo(entry.DistinctBooksCount));
        }
        [Test]
        public void CompressionTest6Bytes() {
            NgramYearEntry entry = new NgramYearEntry(2004, 122446, 7330);
            OptimizedNgramYearEntry optimizedEntry = new OptimizedNgramYearEntry(entry);
            Assert.That(optimizedEntry.BytesCount, Is.EqualTo(6));
            Assert.That(optimizedEntry.Bytes.Length, Is.EqualTo(6));
            Assert.That(optimizedEntry.Bytes[0], Is.EqualTo(0x22));
            Assert.That(optimizedEntry.Bytes[1], Is.EqualTo(0xe5));
            Assert.That(optimizedEntry.Bytes[2], Is.EqualTo(0x00));
            Assert.That(optimizedEntry.Bytes[3], Is.EqualTo(0x4e));
            Assert.That(optimizedEntry.Bytes[4], Is.EqualTo(0xde));
            Assert.That(optimizedEntry.Bytes[5], Is.EqualTo(0x01));
        }
        [Test]
        public void CompressionDecompressionTest6Bytes() {
            NgramYearEntry entry = new NgramYearEntry(2004, 122446, 7330);
            OptimizedNgramYearEntry optimizedEntry = new OptimizedNgramYearEntry(entry);
            optimizedEntry = new OptimizedNgramYearEntry(optimizedEntry.Bytes, 0);
            Assert.That(optimizedEntry.Entry.Year, Is.EqualTo(-1));
            Assert.That(optimizedEntry.Entry.OccurencesCount, Is.EqualTo(entry.OccurencesCount));
            Assert.That(optimizedEntry.Entry.DistinctBooksCount, Is.EqualTo(entry.DistinctBooksCount));
        }
        [Test]
        public void CompressionTestEntry() {
            NgramDataEntry entry = new NgramDataEntry("abc");
            entry.Add(2004, 122446, 7330);
            entry.Add(2008, 6, 3);
            OptimizedNgramDataEntry optimizedEntry = new OptimizedNgramDataEntry(entry);
            Assert.That(optimizedEntry.BytesCount, Is.EqualTo(2 + 1 + 6));
            Assert.That(optimizedEntry.Bytes[0], Is.EqualTo(0xf8));
            Assert.That(optimizedEntry.Bytes[1], Is.EqualTo(0x11));
            Assert.That(optimizedEntry.Bytes[2], Is.EqualTo(0x22));
            Assert.That(optimizedEntry.Bytes[3], Is.EqualTo(0xe5));
            Assert.That(optimizedEntry.Bytes[4], Is.EqualTo(0x00));
            Assert.That(optimizedEntry.Bytes[5], Is.EqualTo(0x4e));
            Assert.That(optimizedEntry.Bytes[6], Is.EqualTo(0xde));
            Assert.That(optimizedEntry.Bytes[7], Is.EqualTo(0x01));
            Assert.That(optimizedEntry.Bytes[8], Is.EqualTo(0xb6));
        }
        [Test]
        public void CompressionDecompressionTestEntry() {
            NgramDataEntry entry = new NgramDataEntry("abc");
            entry.Add(2004, 122446, 7330);
            entry.Add(2008, 6, 3);
            OptimizedNgramDataEntry optimizedEntry = new OptimizedNgramDataEntry(entry);
            using(MemoryStream stream = new MemoryStream()) {
                optimizedEntry.WriteTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
                optimizedEntry = new OptimizedNgramDataEntry("abc", stream, (int)stream.Length);
            }
            Assert.That(optimizedEntry.Entry.Ngram, Is.EqualTo("abc"));
            var yearEntries = optimizedEntry.Entry.YearEntries.ToArray();
            Assert.That(yearEntries.Length, Is.EqualTo(2));
            Assert.That(yearEntries[0].Year, Is.EqualTo(2004));
            Assert.That(yearEntries[0].DistinctBooksCount, Is.EqualTo(7330));
            Assert.That(yearEntries[0].OccurencesCount, Is.EqualTo(122446));
            Assert.That(yearEntries[1].Year, Is.EqualTo(2008));
            Assert.That(yearEntries[1].DistinctBooksCount, Is.EqualTo(3));
            Assert.That(yearEntries[1].OccurencesCount, Is.EqualTo(6));
        }
    }
}
