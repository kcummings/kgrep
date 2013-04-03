using System.Collections.Generic;
using NUnit.Framework;
using kgrep;

namespace Tests {

    [TestFixture]
    class ScanAndPrintTokensTests {

        [Test]
        public void TestSimpleFindToken() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            string newline = engine.ScanAndPrintTokens("abc", new List<string> { "abc", "daf" });
            Assert.AreEqual("abc\n", newline);
        }

    }
}
