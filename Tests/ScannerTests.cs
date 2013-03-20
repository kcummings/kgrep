using kgrep;
using NUnit.Framework;

namespace Tests {
 
        [TestFixture]
        public class ScannerTests {
            [Test]
            public void TestScannerForSingleToken() {
                KgrepEngine engine = new KgrepEngine();
                Assert.AreEqual("b", engine.ScanForTokens("abc", "a(b)c", ""));
            }

            [Test]
            public void TestScannerForMultipleGroupsOnOneLine() {
                KgrepEngine engine = new KgrepEngine();
                Assert.AreEqual("ell|o|", engine.ScanForTokens("hello world", "h(...)(.)", "|"));
            }

            [Test]
            public void TestScannerForNoMatch() {
                KgrepEngine engine = new KgrepEngine();
                Assert.AreEqual("", engine.ScanForTokens("abc", "def", ""));
            }

            [Test]
            public void TestScannerForSingleTokenWithDelim() {
                KgrepEngine engine = new KgrepEngine();
                Assert.AreEqual("bc\n", engine.ScanForTokens("abc", "a(bc)", "\n"));
            }

            [Test]
            public void TestScannerForMatchValue() {
                KgrepEngine engine = new KgrepEngine();
                Assert.AreEqual("b", engine.ScanForTokens("abc", "b", ""));
            }

            //[Test]
            //public void TestScannerWithInvalidToken() {
            //    KgrepEngine engine = new KgrepEngine();
            //    Assert.AreEqual("bc",engine.ScanForTokens("abc", "a(bc", ""));
            //}
        }
    }
