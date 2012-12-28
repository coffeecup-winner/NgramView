using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NgramView.Data;
using NgramView.Providers.Google.Offline;
using System.IO;

namespace NgramView.Tests {
    [TestFixture]
    public class OptimizedNgramDataTests {
        [Test]
        public void CompressionTest7Bytes() {
            NgramYearEntry entry = new NgramYearEntry(2004, 122446, 7330);
            OptimizedNgramDataEntry optimizedEntry = new OptimizedNgramDataEntry(entry);
            Assert.That(optimizedEntry.BytesCount, Is.EqualTo(7));
            Assert.That(optimizedEntry.Bytes.Length, Is.EqualTo(7));
            Assert.That(optimizedEntry.Bytes[0], Is.EqualTo(0x62));
            Assert.That(optimizedEntry.Bytes[1], Is.EqualTo(0x72));
            Assert.That(optimizedEntry.Bytes[2], Is.EqualTo(0x00));
            Assert.That(optimizedEntry.Bytes[3], Is.EqualTo(0xf8));
            Assert.That(optimizedEntry.Bytes[4], Is.EqualTo(0xce));
            Assert.That(optimizedEntry.Bytes[5], Is.EqualTo(0xbc));
            Assert.That(optimizedEntry.Bytes[6], Is.EqualTo(0x03));
        }
        [Test]
        public void CompressionDecompressionTest7Bytes() {
            NgramYearEntry entry = new NgramYearEntry(2004, 122446, 7330);
            OptimizedNgramDataEntry optimizedEntry = new OptimizedNgramDataEntry(entry);
            using(MemoryStream stream = new MemoryStream()) {
                optimizedEntry.WriteTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
                optimizedEntry = new OptimizedNgramDataEntry(stream);
            }
            Assert.That(optimizedEntry.Entry.Year, Is.EqualTo(entry.Year));
            Assert.That(optimizedEntry.Entry.OccurencesCount, Is.EqualTo(entry.OccurencesCount));
            Assert.That(optimizedEntry.Entry.DistinctBooksCount, Is.EqualTo(entry.DistinctBooksCount));
        }
    }
}
