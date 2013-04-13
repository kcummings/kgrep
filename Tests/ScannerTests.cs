using kgrep;
using NUnit.Framework;

namespace Tests {
 
        [TestFixture]
        public class ScannerTests {
            [Test]
            public void TestScannerForSingleToken() {
                ScannerEngine engine = new ScannerEngine();
                Assert.AreEqual("b\n", engine.ScanForTokens("abc", "a(b)c"));
            }

            [Test]
            public void TestScannerForMultipleGroupsOnOneLine() {
                ScannerEngine engine = new ScannerEngine();
                Assert.AreEqual("ell\no\n", engine.ScanForTokens("hello world", "h(...)(.)"));
            }

            [Test]
            public void TestScannerForNoMatch() {
                ScannerEngine engine = new ScannerEngine();
                Assert.AreEqual("", engine.ScanForTokens("abc", "def"));
            }

            [Test]
            public void TestScannerForSingleGroup() {
                ScannerEngine engine = new ScannerEngine();
                Assert.AreEqual("bc\n", engine.ScanForTokens("abc", "a(bc)"));
            }

            [Test]
            public void TestScannerForTwoGroups() {
                ScannerEngine engine = new ScannerEngine();
                Assert.AreEqual("bc\n89\n", engine.ScanForTokens("abc 89", "(..) ([0-9]+)"));
            }

            [Test]
            public void TestScannerForMatchValue() {
                ScannerEngine engine = new ScannerEngine();
                Assert.AreEqual("b\n", engine.ScanForTokens("abc", "b"));
            }

            [Test]
            public void TestScannerForSingleLetters() {
                ScannerEngine engine = new ScannerEngine();
                Assert.AreEqual("a\nb\nc\n \nd\n", engine.ScanForTokens("abc d", "."));
            }

            [Test]
            public void TestScannerForSingleDigits() {
                ScannerEngine engine = new ScannerEngine();
                Assert.AreEqual("8\n5\n1\n2\n3\n", engine.ScanForTokens("85 dollars 123 lost", "[0-9]"));
            }

            [Test]
            public void TestScannerForNumbers() {
                ScannerEngine engine = new ScannerEngine();
                Assert.AreEqual("85\n123\n", engine.ScanForTokens("85 dollars 123 lost", "[0-9]+"));
            }
        }
    }
