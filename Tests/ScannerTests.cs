using System.Collections.Generic;
using kgrep;
using NUnit.Framework;

namespace Tests {
 
        [TestFixture]
        public class ScannerTests {
            [Test]
            public void TestScannerForEndingToken() {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string results = engine.ApplyReplacements("bye bye (.*?)$", new List<string> { "today bye bye birdie" });
                Assert.AreEqual("birdie\n", results);
            }

            [Test]
            public void TestScannerNew() {
                ReplacerEngine engine = new ReplacerEngine() {sw = new WriteToString()};
                string newline = engine.ApplyReplacements("bye", new List<string> {"today bye bye birdie"});
                Assert.AreEqual("bye\nbye\n", newline);
            }

            [Test]
            public void TestScannerForMultipleGroupsOnOneLine() {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string results = engine.ApplyReplacements("h(...)(.)", new List<string> { "hello world" });
                Assert.AreEqual("ell\no\n", results);
            }

            [Test]
            public void TestScannerForNoMatch() {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string results = engine.ApplyReplacements("abc", new List<string> { "def" });
                Assert.AreEqual("", results);
            }

            [Test]
            public void TestScannerForSingleGroup() {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string results = engine.ApplyReplacements("a(bc)", new List<string> { "abc" });
                Assert.AreEqual("bc\n", results);
            }

            [Test]
            public void TestScannerForTwoGroups() {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string results = engine.ApplyReplacements("(..) ([0-9]+)", new List<string> { "abc 89" });
                Assert.AreEqual("bc\n89\n", results);
            }

            [Test]
            public void TestScannerForMatchValue() {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string results = engine.ApplyReplacements("b", new List<string> { "abc" });
                Assert.AreEqual("b\n", results);
            }

            [Test]
            public void TestScannerForSingleLetters() {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string results = engine.ApplyReplacements(".", new List<string> { "abc d" });
                Assert.AreEqual("a\nb\nc\n \nd\n", results);
            }

            [Test]
            public void TestScannerForSingleDigits() {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string results = engine.ApplyReplacements("[0-9]", new List<string> { "85 dollars 123 lost" });
                Assert.AreEqual("8\n5\n1\n2\n3\n", results);
            }

            [Test]
            public void TestScannerForNumbers() {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string results = engine.ApplyReplacements("[0-9]+", new List<string> { "85 dollars 123 lost" });
                Assert.AreEqual("85\n123\n", results);
            }

            [Test]
            public void TestScanWithCommaSeparatedFSinQuotes() {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string result = engine.ApplyReplacements("ScannerFS=\", \"; a", new List<string> { "a b ca" });
                Assert.AreEqual("a, a\n", result);
            }

            [Test]
            public void TestScanWithCommaSeparatedFSwithoutQuotes() {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string result = engine.ApplyReplacements("ScannerFS=,; a", new List<string> { "a b ca" });
                Assert.AreEqual("a,a\n", result);
            }
        }
    }
