using System;
using System.Collections.Generic;
using NUnit.Framework;
using kgrep;

namespace Tests {

    [TestFixture]
    public class SearchAndReplaceTests {

        [Test]
        public void TestSimpleOneLineReplace() {
            ReplacerEngine engine = new ReplacerEngine() {sw = new WriteToString()};
            string newline = engine.ApplyReplacements("a~b", new List<string> {"abc"});
            Assert.AreEqual("bbc\n", newline);
        }

        [Test]
        public void TestSimpleTwoLineReplace() {
            ReplacerEngine engine = new ReplacerEngine() {sw = new WriteToString()};
            string newline = engine.ApplyReplacements("a~b", new List<string> {"abc", "daf"});
            Assert.AreEqual("bbc\ndbf\n", newline);
        }

        [Test]
        public void TestFullCycleReplace() {
            string[] args = new String[] { "a~c", "abc" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string results = engine.ApplyReplacements(cmd.ReplacementFileName, cmd.InputSourceNames);
            Assert.AreEqual("cbc\n", results);
        }

        [Test]
        public void TestFullCycleScan() {
            string[] args = new String[] { "a", "abca" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string results = engine.ApplyReplacements(cmd.ReplacementFileName, cmd.InputSourceNames);
            Assert.AreEqual("a\na\n", results);
        }

        [Test]
        public void TestFullCycleScanWithFile() {
            string[] args = new String[] { "hi", "test.lst" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string results = engine.ApplyReplacements(cmd.ReplacementFileName, cmd.InputSourceNames);
            Assert.AreEqual("hi\nhi\nhi\n", results);
        }

        [Test]
        public void TestScopeAllReplace() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("scope=all; a~b; b~c", new List<string> { "a b c", "a b c", "earth" });
            Assert.AreEqual("c c c\nc c c\necrth\n", newline);
        }

        [Test]
        public void TestScopeFirstReplace() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("scope=first; a~b; b~c", new List<string> { "a b c", "a b c", "earth" });
            Assert.AreEqual("b b c\nb b c\nebrth\n", newline);
        }

        [Test]
        public void TestRemoveSpaces() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements(@"\s~", new List<string> { "a b c", "the   dog  ran. " });
            Assert.AreEqual("abc\nthedogran.\n", newline);
        }

        [Test]
        public void TestExpandEveryThirdLetter() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements(@"([a-z]{3})~$1-", new List<string> { "kgrep works today by"});
            Assert.AreEqual("kgr-ep wor-ks tod-ay by\n", newline);
        }

        [Test]
        public void TestSwapEveryOtherLetter() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements(@"([a-z])([a-z])~$2$1", new List<string> { "kgrep works today by" });
            Assert.AreEqual("gkerp owkrs otady yb\n", newline);
        } 

        [Test]
        public void TestChangeDelimReplace() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("delim=,; hi,bye; here,there", new List<string> { "hi world", "go home today", "here is it" });
            Assert.AreEqual("bye world\ngo home today\nthere is it\n", newline);
        }

        [Test]
        public void TestChangeDelimTwiceReplace() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("delim=,; hi,bye; delim=-; here-there", new List<string> { "hi world", "go home today", "here is it" });
            Assert.AreEqual("bye world\ngo home today\nthere is it\n", newline);
        }

        // Anchor tests
        [Test]
        public void TestAnchorMatchingOneLine() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("my~hi~bye; all~gone", 
                                        new List<string> { "Now is my hi world", "go hi today" });
            Assert.AreEqual("Now is my bye world\ngo hi today\n", newline);
        }

        [Test]
        public void TestAnchorMatchingOneLineFailure() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("my~hi~bye; all~gone",
                                        new List<string> { "Now is your hi world", "go hi today" });
            Assert.AreEqual("Now is your hi world\ngo hi today\n", newline);
        }

        [Test]
        public void TestAnchorMatchingMultipleSameLine() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("my~hi~bye; all~gone",
                                        new List<string> { "This is my hi world", "go hi today" });
            Assert.AreEqual("Tbyes is my bye world\ngo hi today\n", newline);
        }

        [Test]
        public void TestAnchorMatchingTwoLines() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("my~hi~bye; all~gone",
                                        new List<string> { "my is my hi world", "go hi mytoday" });
            Assert.AreEqual("my is my bye world\ngo bye mytoday\n", newline);
        }

        [Test]
        public void TestAnchorMatchingStartLine() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("^Th~hi~bye; all~gone",
                                        new List<string> { "mTh my hi world", "Thgo hi mytoday" });
            Assert.AreEqual("mTh my hi world\nThgo bye mytoday\n", newline);
        }

        [Test]
        public void TestEmptyReplacementList() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("",
                                        new List<string> { "mTh my hi world", "Thgo hi mytoday" });
            Assert.AreEqual("mTh my hi world\nThgo hi mytoday\n", newline);
        }

        [Test]
        public void TestEmptyReplacementFirstList() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("scope=first",
                                        new List<string> { "mTh my hi world", "Thgo hi mytoday" });
            Assert.AreEqual("mTh my hi world\nThgo hi mytoday\n", newline);
        }
    }
}
