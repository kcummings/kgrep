using System;
using System.Collections.Generic;
using NUnit.Framework;
using kgrep;

namespace Tests {

    // A higher level test
    [TestFixture]
    public class SearchAndReplaceAcceptenceTests {

        [Test]
        public void WhenReplacementWithOneLine_ExpectChanges() {
            ReplacerEngine engine = new ReplacerEngine() {sw = new WriteToString()};
            string newline = engine.ApplyReplacements("a~b", new List<string> {"abc"});
            Assert.AreEqual("bbc\n", newline);
        }

        [Test]
        public void WhenReplacementWithTwoInputLines_ExpectChanges() {
            ReplacerEngine engine = new ReplacerEngine() {sw = new WriteToString()};
            string newline = engine.ApplyReplacements("a~b", new List<string> {"abc", "daf"});
            Assert.AreEqual("bbc\ndbf\n", newline);
        }

        [Test]
        public void WhenFullCycleReplaceNoArguments_ExpectNothing() {
            string[] args = new String[0]; 
            ParseCommandLine cmd = new ParseCommandLine(args);
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string results = engine.ApplyReplacements(cmd.ReplacementFileName, cmd.InputSourceNames);
            Assert.AreEqual("", results);
        }

        [Test]
        public void WhenFullCycleReplace_ExpectChanges() {
            string[] args = new String[] { "a~c", "abc" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string results = engine.ApplyReplacements(cmd.ReplacementFileName, cmd.InputSourceNames);
            Assert.AreEqual("cbc\n", results);
        }

        [Test]
        public void WhenFullCycleWithScannerFS_ExpectDelimitedResults() {
            string[] args = new String[] { "ScannerFS=,; a;b", "a b ca" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string results = engine.ApplyReplacements(cmd.ReplacementFileName, cmd.InputSourceNames);
            Assert.AreEqual("", results);
        }

        [Test]
        public void WhenFullCycleScan_ExpectChanges() {
            string[] args = new String[] { "a", "abca" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string results = engine.ApplyReplacements(cmd.ReplacementFileName, cmd.InputSourceNames);
            Assert.AreEqual("a\na\n", results);
        }


        [Test]
        public void WhenScopeAllGiven_ExpectAllReplaced() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("scope=all; a~b; b~c", new List<string> { "a b c", "a b c", "earth" });
            Assert.AreEqual("c c c\nc c c\necrth\n", newline);
        }

        [Test]
        public void WhenScopeFirstGiven_ExpectFirstReplace() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("scope=first; a~b; b~c", new List<string> { "a b c", "a b c", "earth" });
            Assert.AreEqual("b b c\nb b c\nebrth\n", newline);
        }

        [Test]
        public void WhenReplacementGiven_ExpectRemoveSpaces() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements(@"\s~", new List<string> { "a b c", "the   dog  ran. " });
            Assert.AreEqual("abc\nthedogran.\n", newline);
        }

        [Test]
        public void WhenReplacementGiven_ExpectExpandEveryThirdLetter() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements(@"([a-z]{3})~$1-", new List<string> { "kgrep works today by"});
            Assert.AreEqual("kgr-ep wor-ks tod-ay by\n", newline);
        }

        [Test]
        public void WhenReplacementGiven_ExpectEveryOtherLetterSwapped() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements(@"([a-z])([a-z])~$2$1", new List<string> { "kgrep works today by" });
            Assert.AreEqual("gkerp owkrs otady yb\n", newline);
        } 

        [Test]
        public void WhenDelimIsChanged_ExpectChangeWithNewDelim() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("delim=,; hi,bye; here,there", new List<string> { "hi world", "go home today", "here is it" });
            Assert.AreEqual("bye world\ngo home today\nthere is it\n", newline);
        }

        [Test]
        public void WhenDelimChangesTwice_ExpectTwoChanges() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("delim=,; hi,bye; delim=-; here-there", new List<string> { "hi world", "go home today", "here is it" });
            Assert.AreEqual("bye world\ngo home today\nthere is it\n", newline);
        }

        // Anchor tests
        [Test]
        public void WhenAnchorMatchesFirstLineOnly_ExpectChanges() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("my~hi~bye; all~gone", 
                                        new List<string> { "Now is my hi world", "go hi today" });
            Assert.AreEqual("Now is my bye world\ngo hi today\n", newline);
        }

        [Test]
        public void WhenAnchorNotMatched_ExpectNoChange() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("my~hi~bye; all~gone",
                                        new List<string> { "Now is your hi world", "go hi today" });
            Assert.AreEqual("Now is your hi world\ngo hi today\n", newline);
        }

        [Test]
        public void WhenReplacementHasAnchorThatMatchesManySameLine_ExpectMultipleChangesLine() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("my~hi~bye; all~gone",
                                        new List<string> { "This is my hi world", "go hi today" });
            Assert.AreEqual("Tbyes is my bye world\ngo hi today\n", newline);
        }

        [Test]
        public void WhenReplacementHasAnchorThatMatchesOnTwoLines_ExpectChanges() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("my~hi~bye; all~gone",
                                        new List<string> { "my is my hi world", "go hi mytoday" });
            Assert.AreEqual("my is my bye world\ngo bye mytoday\n", newline);
        }

        [Test]
        public void WhenReplacementHasAnchorThatMatches_ExpectChange() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("^Th~hi~bye; all~gone",
                                        new List<string> { "mTh my hi world", "Thgo hi mytoday" });
            Assert.AreEqual("mTh my hi world\nThgo bye mytoday\n", newline);
        }

        [Test]
        public void WhenEmptyReplacement_ExpectNoChange() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("",
                                        new List<string> { "Hello World", "See you later." });
            Assert.AreEqual("Hello World\nSee you later.\n", newline);
        }

        [Test]
        public void WhenNoReplacementGiven_ExpectNoChange() {
            ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
            string newline = engine.ApplyReplacements("scope=first",
                                        new List<string> { "Hello World", "See you later." });
            Assert.AreEqual("Hello World\nSee you later.\n", newline);
        }
    }
}
