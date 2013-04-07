using kgrep;
using NUnit.Framework;

namespace Tests {
 
        [TestFixture]
        public class ScannerTests {
            [Test]
            public void TestScannerForSingleToken() {
                KgrepEngine engine = new KgrepEngine();
                Assert.AreEqual("b\n", engine.ScanForTokens("abc", "a(b)c"));
            }

            [Test]
            public void TestScannerForMultipleGroupsOnOneLine() {
                KgrepEngine engine = new KgrepEngine();
                Assert.AreEqual("ell\no\n", engine.ScanForTokens("hello world", "h(...)(.)"));
            }

            [Test]
            public void TestScannerForNoMatch() {
                KgrepEngine engine = new KgrepEngine();
                Assert.AreEqual("", engine.ScanForTokens("abc", "def"));
            }

            [Test]
            public void TestScannerForSingleGroup() {
                KgrepEngine engine = new KgrepEngine();
                Assert.AreEqual("bc\n", engine.ScanForTokens("abc", "a(bc)"));
            }

            [Test]
            public void TestScannerForTwoGroups() {
                KgrepEngine engine = new KgrepEngine();
                Assert.AreEqual("bc\n89\n", engine.ScanForTokens("abc 89", "(..) ([0-9]+)"));
            }

            [Test]
            public void TestScannerForMatchValue() {
                KgrepEngine engine = new KgrepEngine();
                Assert.AreEqual("b\n", engine.ScanForTokens("abc", "b"));
            }

            [Test]
            public void TestScannerForSingleLetters() {
                KgrepEngine engine = new KgrepEngine();
                Assert.AreEqual("a\nb\nc\n \nd\n", engine.ScanForTokens("abc d", "."));
            }

            [Test]
            public void TestScannerForSingleDigits() {
                KgrepEngine engine = new KgrepEngine();
                Assert.AreEqual("8\n5\n1\n2\n3\n", engine.ScanForTokens("85 dollars 123 lost", "[0-9]"));
            }

            [Test]
            public void TestScannerForNumbers() {
                KgrepEngine engine = new KgrepEngine();
                Assert.AreEqual("85\n123\n", engine.ScanForTokens("85 dollars 123 lost", "[0-9]+"));
            }
        }
    }
