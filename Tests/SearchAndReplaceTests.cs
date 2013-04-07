using System.Collections.Generic;
using NUnit.Framework;
using kgrep;

namespace Tests {

    [TestFixture]
    public class SearchAndReplaceTests {

        [Test]
        public void TestSimpleOneLineReplace() {
            KgrepEngine engine = new KgrepEngine() {sw = new WriteToString()};
            string newline = engine.SearchAndReplaceTokens("a~b", new List<string> {"abc"});
            Assert.AreEqual("bbc\n", newline);
        }

        [Test]
        public void TestSimpleTwoLineReplace() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            string newline = engine.SearchAndReplaceTokens("a~b", new List<string> { "abc", "daf" });
            Assert.AreEqual("bbc\ndbf\n", newline);
        }

        [Test]
        public void TestScopeAllReplace() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            string newline = engine.SearchAndReplaceTokens("scope=all; a~b; b~c", new List<string> { "a b c", "a b c", "earth" });
            Assert.AreEqual("c c c\nc c c\necrth\n", newline);
        }

        [Test]
        public void TestScopeFirstReplace() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            string newline = engine.SearchAndReplaceTokens("scope=first; a~b; b~c", new List<string> { "a b c", "a b c", "earth" });
            Assert.AreEqual("b b c\nb b c\nebrth\n", newline);
        }

        [Test]
        public void TestRemoveSpaces() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            string newline = engine.SearchAndReplaceTokens(@"\s~", new List<string> { "a b c", "the   dog  ran. " });
            Assert.AreEqual("abc\nthedogran.\n", newline);
        }

        [Test]
        public void TestExpandEveryThirdLetter() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            string newline = engine.SearchAndReplaceTokens(@"([a-z]{3})~$1-", new List<string> { "kgrep works today by"});
            Assert.AreEqual("kgr-ep wor-ks tod-ay by\n", newline);
        }

        [Test]
        public void TestSwapEveryOtherLetter() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            string newline = engine.SearchAndReplaceTokens(@"([a-z])([a-z])~$2$1", new List<string> { "kgrep works today by" });
            Assert.AreEqual("gkerp owkrs otady yb\n", newline);
        } 

        [Test]
        public void TestChangeDelimReplace() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            string newline = engine.SearchAndReplaceTokens("delim=,; hi,bye; here,there", new List<string> { "hi world", "go home today", "here is it" });
            Assert.AreEqual("bye world\ngo home today\nthere is it\n", newline);
        }

        [Test]
        public void TestChangeDelimTwiceReplace() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            string newline = engine.SearchAndReplaceTokens("delim=,; hi,bye; delim=-; here-there", new List<string> { "hi world", "go home today", "here is it" });
            Assert.AreEqual("bye world\ngo home today\nthere is it\n", newline);
        }
    }
}
