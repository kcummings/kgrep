using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using kgrep;

namespace Tests {

    // A higher level test
    [TestFixture]
    public class ReplaceTokensTests {


        // To simulate input from 3 files use: new List<string> { "a b c", "a b c", "earth" }
        // To simulate input from a file with 2 commands use: new List<string> { "a~b; 4~5" }
        [Test]
        public void WhenReplacementWithOneLine_ExpectChanges() {
            ReplaceTokens engine = new ReplaceTokens() {sw = new WriteToString()};
            ParseCommandFile commands = new ParseCommandFile("a~b");
            string newline = engine.ApplyCommandsToInputFileList(commands, new List<string> {"abc"});
            Assert.AreEqual("bbc\n", newline);
        }

        [Test]
        public void WhenReplacementWithTwoInputLines_ExpectChanges() {
            ReplaceTokens engine = new ReplaceTokens() {sw = new WriteToString()};
            ParseCommandFile commands = new ParseCommandFile("a~b");
            string newline = engine.ApplyCommandsToInputFileList(commands, new List<string> {"abc", "daf"});
            Assert.AreEqual("bbc\ndbf\n", newline);
        }

        [Test]
        public void WhenFullCycleReplaceNoArguments_ExpectNothing() {
            IUtilities util = Substitute.For<IUtilities>();
            util.ExpandFileNameWildCards("abc").Returns(new List<string> { "abc" });

            string[] args = new String[0];
            ParseCommandLine cmd = new ParseCommandLine(){utilities = util};
            cmd.Init(args);
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(cmd.ReplacementFileName);
            string results = engine.ApplyCommandsToInputFileList(commands, cmd.InputSourceList);
            Assert.AreEqual("", results);
        }

        [Test]
        public void WhenReplacementsGiven_ExpectAllReplaced() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("a~b; b~c");
            string newline = engine.ApplyCommandsToInputFileList(commands, new List<string> { "a b c", "a b c", "earth" });
            Assert.AreEqual("c c c\nc c c\necrth\n", newline);
        }

        [Test]
        public void WhenFullCycleReplace_ExpectChanges() {
            IUtilities util = Substitute.For<IUtilities>();
            util.ExpandFileNameWildCards("abc").Returns(new List<string> { "abc" });

            string[] args = new String[] { "a~c", "abc" };
            ParseCommandLine cmd = new ParseCommandLine() {utilities = util};
            cmd.Init(args);
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(cmd.ReplacementFileName);
            string results = engine.ApplyCommandsToInputFileList(commands, cmd.InputSourceList);
            Assert.AreEqual("cbc\n", results);
        }

        [Test]
        public void WhenReplacementGiven_ExpectRemoveSpaces() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"\s~");
            string newline = engine.ApplyCommandsToInputFileList(commands, new List<string> { "a b c", "the   dog  ran. " });
            Assert.AreEqual("abc\nthedogran.\n", newline);
        }

        [Test]
        public void WhenReplacementGiven_ExpectExpandEveryThirdLetter() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"([a-z]{3})~$1-");
            string newline = engine.ApplyCommandsToInputFileList(commands, new List<string> { "kgrep works today by" });
            Assert.AreEqual("kgr-ep wor-ks tod-ay by\n", newline);
        }

        [Test]
        public void WhenReplacementGiven_ExpectEveryOtherLetterSwapped() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"([a-z])([a-z])~$2$1");
            string newline = engine.ApplyCommandsToInputFileList(commands, new List<string> { "kgrep works today by" });
            Assert.AreEqual("gkerp owkrs otady yb\n", newline);
        }

        [Test]
        public void WhenDelimIsChanged_ExpectChangeWithNewDelim() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("delim=,; hi,bye; here,there");
            string newline = engine.ApplyCommandsToInputFileList(commands, new List<string> { "hi world", "go home today", "here is it" });
            Assert.AreEqual("bye world\ngo home today\nthere is it\n", newline);
        }

        [Test]
        public void WhenDelimChangesTwice_ExpectTwoChanges() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("delim=,; hi,bye; delim=-; here-there");
            string newline = engine.ApplyCommandsToInputFileList(commands, new List<string> { "hi world", "go home today", "here is it" });
            Assert.AreEqual("bye world\ngo home today\nthere is it\n", newline);
        }

        // AnchorString tests
        [Test]
        public void WhenAnchorMatchesFirstLineOnly_ExpectChanges() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("/my/hi~bye; all~gone");
            string newline = engine.ApplyCommandsToInputFileList(commands, new List<string> { "Now is my hi world;go hi today" });
            Assert.AreEqual("Now is my bye world\ngo hi today\n", newline);
        }

        [Test]
        public void WhenAnchorNotMatched_ExpectNoChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my~hi~bye; all~gone");
            string newline = engine.ApplyCommandsToInputFileList(commands,
                                        new List<string> { "Now is your hi world", "go hi today" });
            Assert.AreEqual("Now is your hi world\ngo hi today\n", newline);
        }

        [Test]
        public void WhenReplacementHasAnchorThatMatchesManySameLine_ExpectMultipleChangesLine() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("/my/hi~bye; all~gone");
            string newline = engine.ApplyCommandsToInputFileList(commands,
                                        new List<string> { "This is my hi world", "go hi today" });
            Assert.AreEqual("Tbyes is my bye world\ngo hi today\n", newline);
        }

        [Test]
        public void WhenReplacementHasAnchorThatMatchesOnTwoLines_ExpectChanges() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("/my/hi~bye; all~gone");
            string newline = engine.ApplyCommandsToInputFileList(commands,
                                        new List<string> { "my is my hi world", "go hi mytoday" });
            Assert.AreEqual("my is my bye world\ngo bye mytoday\n", newline);
        }

        [Test]
        public void WhenReplacementHasAnchorThatMatches_ExpectChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("/^Th/hi~bye; all~gone");
            string newline = engine.ApplyCommandsToInputFileList(commands,
                                        new List<string> { "mTh my hi world", "Thgo hi mytoday" });
            Assert.AreEqual("mTh my hi world\nThgo bye mytoday\n", newline);
        }

        [Test]
        public void WhenEmptyReplacement_ExpectNoChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("");
            string newline = engine.ApplyCommandsToInputFileList(commands,
                                        new List<string> { "Hello World", "See you later." });
            Assert.AreEqual("Hello World\nSee you later.\n", newline);
        }

        [TestCase("d a b ca", @" \sa ~ b ", "db b ca\n")]  // single leading
        [TestCase(" ab  c", @" \s\sc~", " ab\n")]          // two leading spaces and remove it
        [TestCase("abc d", @" bc\s~bc", "abcd\n")]         // single trailing
        [TestCase("abc  d", @" bc\s\s~bc", "abcd\n")]      // two trailng spaces and remove it
        public void WhenRegexSpaceInFrompattern_ExpectSpaces(string input, string repstring, string expect) {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(repstring);
            string newline = engine.ApplyCommandsToInputFileList(commands, new List<string> { input });
            Assert.AreEqual(expect, newline);
        }

        [TestCase("def", "abc~def", "abc")]
        [TestCase("hll bye", "e(..)o-(...)~$1 $2", "hello-bye")]
        [TestCase("attr=hi", "<tag attr='(.+?)'>~attr=$1", "<tag attr='hi'>")]
        public void WhenSimpleRegex_ExpectSimpleResults(string expected, string line, string input) {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            Command command = new Command(line);
            Assert.AreEqual(expected, engine.ApplyCommandsAllMatches(input, command));
        }

        [Test]
        public void WhenMatchsOnMultipleLines_All_ExpectCummulativeChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            List<Command> reps = new List<Command>();
            reps.Add(new Command("from~to"));
            reps.Add(new Command("to~this"));
            Assert.AreEqual("this me this you", engine.ApplyCommandsToLine("from me to you", reps));
        }

        [Test]
        public void WhenMatchesOnMultipleLinesWithGroups_ExpectChainedReplaces() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            List<Command> reps = new List<Command>();
            reps.Add(new Command("f(..)m~$1"));
            reps.Add(new Command("to~this"));
            Assert.AreEqual("ro me this you", engine.ApplyCommandsToLine("from me to you", reps));
        }

        [Test]
        public void WhenReplacmentSwapsGroups_All_ExpectChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            List<Command> reps = new List<Command>();
            Command command = new Command(@"(\w+)\s(\w+)~$2 $1");
            Assert.AreEqual("me from you to", engine.ApplyCommandsAllMatches("from me to you", command));
        }

        [Test]
        public void WhenAnchorMatchesButPatternDoesNot_All_ExpectNoChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            Command command = new Command("/today/ Joe~to");
            Assert.AreEqual("you and you today", engine.ApplyCommandsAllMatches("you and you today", command));
        }

        [Test]
        public void WhenAnchorAndPatternMatches_All_ExpectChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            Command command = new Command("/today/ you ~ to");
            Assert.AreEqual("to and to today", engine.ApplyCommandsAllMatches("you and you today", command));
        }

        [Test]
        public void WhenSimpleMatch_OnlyPrintMatched() {
            PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("OFS='';(you) ~ to");
            string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "you and you today" });
            Assert.AreEqual("youyou\n", results);
        }
    }
}
