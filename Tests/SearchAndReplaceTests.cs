using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using kgrep;

namespace Tests {

    // A higher level test
    [TestFixture]
    public class SearchAndReplaceAcceptenceTests {

        [Test]
        public void WhenReplacementWithOneLine_ExpectChanges() {
            ReplaceAllMatches engine = new ReplaceAllMatches() {sw = new WriteToString()};
            ParseCommandFile commands = new ParseCommandFile("a~b");
            string newline = engine.ApplyCommands(commands, new List<string> {"abc"});
            Assert.AreEqual("bbc\n", newline);
        }

        [Test]
        public void WhenReplacementWithTwoInputLines_ExpectChanges() {
            ReplaceAllMatches engine = new ReplaceAllMatches() {sw = new WriteToString()};
            ParseCommandFile commands = new ParseCommandFile("a~b");
            string newline = engine.ApplyCommands(commands, new List<string> {"abc", "daf"});
            Assert.AreEqual("bbc\ndbf\n", newline);
        }

        [Test]
        public void WhenFullCycleReplaceNoArguments_ExpectNothing() {
            IUtilities util = Substitute.For<IUtilities>();
            util.ExpandFileNameWildCards("abc").Returns(new List<string> { "abc" });

            string[] args = new String[0];
            ParseCommandLine cmd = new ParseCommandLine(){utilities = util};
            cmd.Init(args);
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(cmd.ReplacementFileName);
            string results = engine.ApplyCommands(commands, cmd.InputSourceList);
            Assert.AreEqual("", results);
        }

        [Test]
        public void WhenScopeAllGiven_ExpectAllReplaced() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("scope=all; a~b; b~c");
            string newline = engine.ApplyCommands(commands, new List<string> { "a b c", "a b c", "earth" });
            Assert.AreEqual("c c c\nc c c\necrth\n", newline);
        }

        [Test]
        public void WhenFullCycleReplace_ExpectChanges() {
            IUtilities util = Substitute.For<IUtilities>();
            util.ExpandFileNameWildCards("abc").Returns(new List<string> { "abc" });

            string[] args = new String[] { "a~c", "abc" };
            ParseCommandLine cmd = new ParseCommandLine() {utilities = util};
            cmd.Init(args);
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(cmd.ReplacementFileName);
            string results = engine.ApplyCommands(commands, cmd.InputSourceList);
            Assert.AreEqual("cbc\n", results);
        }

        [Test]
        public void WhenScopeFirstGiven_ExpectFirstReplace() {
            ReplaceFirstMatch engine = new ReplaceFirstMatch() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("scope=first; a~b; b~c");
            string newline = engine.ApplyCommands(commands, new List<string> { "a b c", "a b c", "earth" });
            Assert.AreEqual("b b c\nb b c\nebrth\n", newline);
        }

        [Test]
        public void WhenReplacementGiven_ExpectRemoveSpaces() {
            ReplaceFirstMatch engine = new ReplaceFirstMatch() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"\s~");
            string newline = engine.ApplyCommands(commands, new List<string> { "a b c", "the   dog  ran. " });
            Assert.AreEqual("abc\nthedogran.\n", newline);
        }

        [Test]
        public void WhenReplacementGiven_ExpectExpandEveryThirdLetter() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"([a-z]{3})~$1-");
            string newline = engine.ApplyCommands(commands, new List<string> { "kgrep works today by" });
            Assert.AreEqual("kgr-ep wor-ks tod-ay by\n", newline);
        }

        [Test]
        public void WhenReplacementGiven_ExpectEveryOtherLetterSwapped() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"([a-z])([a-z])~$2$1");
            string newline = engine.ApplyCommands(commands, new List<string> { "kgrep works today by" });
            Assert.AreEqual("gkerp owkrs otady yb\n", newline);
        }

        [Test]
        public void WhenDelimIsChanged_ExpectChangeWithNewDelim() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("delim=,; hi,bye; here,there");
            string newline = engine.ApplyCommands(commands, new List<string> { "hi world", "go home today", "here is it" });
            Assert.AreEqual("bye world\ngo home today\nthere is it\n", newline);
        }

        [Test]
        public void WhenDelimChangesTwice_ExpectTwoChanges() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("delim=,; hi,bye; delim=-; here-there");
            string newline = engine.ApplyCommands(commands, new List<string> { "hi world", "go home today", "here is it" });
            Assert.AreEqual("bye world\ngo home today\nthere is it\n", newline);
        }

        // AnchorString tests
        [Test]
        public void WhenAnchorMatchesFirstLineOnly_ExpectChanges() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my~hi~bye; all~gone");
            string newline = engine.ApplyCommands(commands,
                                        new List<string> { "Now is my hi world", "go hi today" });
            Assert.AreEqual("Now is my bye world\ngo hi today\n", newline);
        }

        [Test]
        public void WhenAnchorNotMatched_ExpectNoChange() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my~hi~bye; all~gone");
            string newline = engine.ApplyCommands(commands,
                                        new List<string> { "Now is your hi world", "go hi today" });
            Assert.AreEqual("Now is your hi world\ngo hi today\n", newline);
        }

        [Test]
        public void WhenReplacementHasAnchorThatMatchesManySameLine_ExpectMultipleChangesLine() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my~hi~bye; all~gone");
            string newline = engine.ApplyCommands(commands,
                                        new List<string> { "This is my hi world", "go hi today" });
            Assert.AreEqual("Tbyes is my bye world\ngo hi today\n", newline);
        }

        [Test]
        public void WhenReplacementHasAnchorThatMatchesOnTwoLines_ExpectChanges() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my~hi~bye; all~gone");
            string newline = engine.ApplyCommands(commands,
                                        new List<string> { "my is my hi world", "go hi mytoday" });
            Assert.AreEqual("my is my bye world\ngo bye mytoday\n", newline);
        }

        [Test]
        public void WhenReplacementHasAnchorThatMatches_ExpectChange() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("^Th~hi~bye; all~gone");
            string newline = engine.ApplyCommands(commands,
                                        new List<string> { "mTh my hi world", "Thgo hi mytoday" });
            Assert.AreEqual("mTh my hi world\nThgo bye mytoday\n", newline);
        }

        [Test]
        public void WhenEmptyReplacement_ExpectNoChange() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("");
            string newline = engine.ApplyCommands(commands,
                                        new List<string> { "Hello World", "See you later." });
            Assert.AreEqual("Hello World\nSee you later.\n", newline);
        }

        [Test]
        public void WhenNoReplacementGiven_ExpectNoChange() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("scope=first");
            string newline = engine.ApplyCommands(commands,
                                        new List<string> { "Hello World", "See you later." });
            Assert.AreEqual("Hello World\nSee you later.\n", newline);
        }
    }
}
