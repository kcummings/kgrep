using System.Collections.Generic;
using NUnit.Framework;
using kgrep;

namespace Tests {

    [TestFixture]
    class ScanAndPrintTokensTests {

        [Test]
        public void TestSimpleFindToken() {
            ScannerEngine engine = new ScannerEngine() { sw = new WriteToString() };
            string newline = engine.ScanAndPrintTokens("abc", new List<string> { "abc", "daf" });
            Assert.AreEqual("abc\n", newline);
        }

    }
}
