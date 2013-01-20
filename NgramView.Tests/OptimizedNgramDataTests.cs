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
        public void CompressionTest2Bytes() {
            NgramYearEntry entry = new NgramYearEntry(2008, 6, 3);
            OptimizedNgramYearEntry optimizedEntry = new OptimizedNgramYearEntry(entry);
            Assert.That(optimizedEntry.BytesCount, Is.EqualTo(1));
            Assert.That(optimizedEntry.Bytes.Length, Is.EqualTo(1));
            Assert.That(optimizedEntry.Bytes[0], Is.EqualTo(0xb6));
        }
        [Test]
        public void CompressionDecompressionTest2Bytes() {
            NgramYearEntry entry = new NgramYearEntry(2008, 6, 3);
            OptimizedNgramYearEntry optimizedEntry = new OptimizedNgramYearEntry(entry);
            using(MemoryStream stream = new MemoryStream()) {
                optimizedEntry.WriteTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
                optimizedEntry = new OptimizedNgramYearEntry(stream);
            }
            Assert.That(optimizedEntry.Entry.Year, Is.EqualTo(-1));
            Assert.That(optimizedEntry.Entry.OccurencesCount, Is.EqualTo(entry.OccurencesCount));
            Assert.That(optimizedEntry.Entry.DistinctBooksCount, Is.EqualTo(entry.DistinctBooksCount));
        }
        [Test]
        public void CompressionTest7Bytes() {
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
        public void CompressionDecompressionTest7Bytes() {
            NgramYearEntry entry = new NgramYearEntry(2004, 122446, 7330);
            OptimizedNgramYearEntry optimizedEntry = new OptimizedNgramYearEntry(entry);
            using(MemoryStream stream = new MemoryStream()) {
                optimizedEntry.WriteTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
                optimizedEntry = new OptimizedNgramYearEntry(stream);
            }
            Assert.That(optimizedEntry.Entry.Year, Is.EqualTo(-1));
            Assert.That(optimizedEntry.Entry.OccurencesCount, Is.EqualTo(entry.OccurencesCount));
            Assert.That(optimizedEntry.Entry.DistinctBooksCount, Is.EqualTo(entry.DistinctBooksCount));
        }
    }
}
