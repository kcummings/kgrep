using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using kgrep;

namespace Tests {

    [TestFixture]
    public class PickupTests {

        [TearDown]
        public void ClearPickupList() {
            Pickup pickup = new Pickup();
            pickup.ClearPickupList();
        }


        [TestCase(false, "nothing here")]   // no captures present
        //[TestCase(false, "one (?# comment)")]   // comment
        //[TestCase(false, @"(?<=NUM:)\d+|\w+")]  // if then/else
        //[TestCase(false, @"(?<!this)")]         // lookbehind
        //[TestCase(false, @"(?<=this)")]         // lookahead
        //[TestCase(false, @"(?>.*?)")]           // atomic grouping
        //[TestCase(false, @"(?i:very)")]         // mode modifier
        public void WhenPatternGiven_DoNotFindCaptures(bool expected, string cmd) {
            ReplaceTokens engine = new ReplaceTokens();
            List<Command> reps = new List<Command>();
            reps.Add(new Command(cmd));
            Assert.IsFalse(reps[0].IsCaptureInSubjectString);
        }

        [TestCase(true, "one (?:and|or) (two)")]  // grouping-only parentheses
        [TestCase(true, @"(abc) the art (?<mytag>of) today is (?i:very) (?#new)")]         // mode modifier
        public void WhenPatternGiven_FindCaptures(bool expected, string cmd) {
            ReplaceTokens engine = new ReplaceTokens();
            List<Command> reps = new List<Command>();
            reps.Add(new Command(cmd));
            Assert.IsTrue(reps[0].IsCaptureInSubjectString);
        }

        [Test]
        [ExpectedException(typeof(System.Exception))]
        public void WhenInvalidCapturePresent_ExpectException() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my (?<title) bye~hi");
            Assert.IsFalse(commands.CommandList[0].IsCaptureInSubjectString);
        }

        [Test]
        public void WhenReplacementPickupPresent_ExpectResults() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my bye~hi ${title}");
            Assert.IsTrue(commands.CommandList[0].IsPickupInReplacementString);
        }

        [Test]
        public void WhenInvalidReplacementPickupPresent_ExpectResults() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my bye~hi {title}$");
            Assert.IsFalse(commands.CommandList[0].IsPickupInReplacementString);
        }

        [Test]
        public void WhenTwoCapturesGiven_ExpectTwo() {
            Command command = new Command("^(?<name>[a-z]+).*?(?<second>[0-9]+)");
            Pickup pickup = new Pickup();
            pickup.CollectAllPickupsInLine("ab c 45", command);
            var results = pickup.ReplacePickupsWithStoredValue("${name}");
            Assert.AreEqual("ab", results);
            results = pickup.ReplacePickupsWithStoredValue("${second}");
            Assert.AreEqual("45",results);
        }

        [Test]
        public void WhenSameCaptureNameReused_ExpectLastValue() {
            Command command = new Command(@"^(?<name>[a-z]+).*?(?<name>[0-9]+)");
            Pickup pickup = new Pickup();
            pickup.CollectAllPickupsInLine("ab c 45", command);
            var results = pickup.ReplacePickupsWithStoredValue("${name}");
            Assert.AreEqual("45", results);
        }

        [Test]
        public void WhenPickupPresent_ExpectReplacedValue() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"^(?<name>[a-z]+)~c${name}");
            string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "a" });
            Assert.AreEqual("ca\n", results);
        }

        [Test]
        public void WhenPickupRepeats_ExpectReplacedValues() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"^(?<name>[a-z]+)~c${name}${name}");
            string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "a" });
            Assert.AreEqual("caa\n", results);
        }

        [Test]
        public void WhenPickupSpansLines_ExpectReplacedValue() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"^(?<name>[a-z]+)~blue ${name};blue ~${name}");
            string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "word" });
            Assert.AreEqual("word word\n", results);
        }

        [Test]
        public void WhenPickupAndRegexGiven_ExpectBothReplaced() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"^(\w)(.);..(.)~$1${2}");
            string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "abc" });
            Assert.AreEqual("cb\n", results);
        }

        [Test]
        public void WhenTwoPickupSpansLines_ExpectReplacedValue() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"^(?<name>[a-z]+)~blue ${name};(?<digit>[0-9]+) ~${name}; blue~ ${name}${digit}");
            string results = engine.ApplyCommandsToInputFileList(commands, new List<string> {  "ab 89 cd" });
            Assert.AreEqual("ab89 ab ab cd\n", results);
        }

        [Test]
        public void WhenTwoPickupSpansLinesWithMultipleReferences_ExpectReplacedValue() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"^<chap=(?<tag>[a-z]+?)>~; end~chapter=${tag}");
            string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "<chap=a><last=z> end" });
            Assert.AreEqual("<last=z> chapter=a\n", results);
        }

        [Test]
        public void WhenMatchedNamedAndUnnamedPickups_ExpectReplacedValue() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"(?<letter>[a-z]+).([0-9]);12~${1}${letter}");
            string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "ab 12" });
            Assert.AreEqual("ab 1ab\n", results);
        }

        [Test]
        public void WhenDifferentNamedPickups_ExpectReplacedValue() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"(?<name>[a-z]+) (?<name2>[a-z]+);(?<digit>[0-9]+)~${name}");
            string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "ab cd", "89ab" });
            Assert.AreEqual("ab cd\nabab\n", results);
        }

        [Test]
        public void WhenNamedPickupChanged_ExpectReplacedValue() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"^(?<name>[a-z]+)~blue;(?<digit>[0-9]+)~${name}");
            string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "ab cd", "89ab" });
            Assert.AreEqual("blue cd\nabab\n", results);
        }

        [Test]
        public void WhenVariousTokensGiven_ExpectAppropiateCommandTypes() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"a~b; a; /d/a~b");
            Assert.IsTrue(commands.CommandList[0].IsNormal);
            Assert.IsTrue(commands.CommandList[1].IsPickup);
            Assert.IsTrue(commands.CommandList[2].IsAnchored);
            Assert.AreEqual(ParseCommandFile.RunningAs.ReplaceAll, commands.kgrepMode);
        }

        [Test]
        public void WhenOnlyScanTokensGiven_ExpectScannerMode() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"b; a; c");
            Assert.AreEqual(ParseCommandFile.RunningAs.Scanner, commands.kgrepMode);
        }

        [Test]
        [TestCase("a~b")]
        [TestCase("//a~")]
        [TestCase("a~")]
        public void WhenValidFieldContent_ExpectValidCommand(string line) {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(line);
            Assert.AreEqual(1, commands.CommandList.Count);
        }

        [Test]
        [TestCase("~~~")]
        [TestCase("~~~~")]
        [TestCase("~")]
        [TestCase("/a/ ~")]  // only anchor supplied
        [TestCase(@"a~b~c~d~e")]
        [TestCase("delim=:::::")]
        [TestCase("delim=:::")]
        [TestCase("/a/~#abc")]
        [TestCase("~a  ")]
        public void WhenInvalidFieldContent_ExpectInvalidCommand(string line) {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(line);
            Assert.AreEqual(0, commands.CommandList.Count);
        }

        [Test]
        [ExpectedException(typeof(System.Exception))]
        [TestCase("[~]")]
        [TestCase("(~)")]
        public void WhenInvalidCommandSyntax_ExpectException(string line) {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(line);
        }

        [Test]
        public void WhenGlobPickup_ExpectHeldValue() {
            Command command = new Command(@"a{name}d");
            Pickup pickup = new Pickup();
            pickup.CollectAllPickupsInLine("ab cd",command);
            string results = pickup.ReplacePickupsWithStoredValue("ab${name}");
            Assert.AreEqual("abb c", results);
        }

        [Test]
        public void WhenGlobPickupIsTooShort_ExpectNoValue() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"<{opentag}>;abc~${opentag}");
            string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "<>def", "abc here" });
            Assert.AreEqual("<>def\n${opentag} here\n", results);
        }

        [Test]
        public void WhenGlobPickupsSpanLines_ExpectReplacedValue() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"<{opentag}>;abc~${opentag}");
            string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "<tag>def", "abc here" });
            Assert.AreEqual("<tag>def\ntag here\n", results);
        }

        [Test]
        public void WhenGlobPickupsSpanThreeLines_ExpectReplacedValue() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"<title>{title}</title>; <isbn>{isbn}$; report ~ ${isbn} ${title}");
            string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "<isbn>97809", "<title>Answers</title>", "report" });
            Assert.AreEqual("<isbn>97809\n<title>Answers</title>\n97809 Answers\n", results);
        }

        [Test]
        public void WhenGlobPickupsWithPattern_ExpectReplacedValue() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"<isbn>{isbn=[0-9]+}$; report ~ ${isbn}");
            string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "<isbn>97809", "report" });
            Assert.AreEqual("<isbn>97809\n97809\n", results);
        }

        [Test]
        public void ExpandPickupWithOutAnExplicitPattern() {
            Pickup sh = new Pickup();
            string results = sh.ReplaceShorthandPatternWithFormalRegex("ab{test} d");
            Assert.AreEqual("ab(?<test>.+?) d",results);
        }

        [TestCase("ab{test=[0-9]+} d", "ab(?<test>[0-9]+) d")]
        [TestCase("ab{test=[a-c]+}", "ab(?<test>[a-c]+)")]
        [TestCase("ab{test=[0-9+} d", "ab(?<test>[0-9+) d")]
        [TestCase("ab{test a", "ab{test a")]
        [Test]
        public void ExpandPickupWithAnExplicitPattern(string line, string expectedResults) {
            Pickup sh = new Pickup();
            string results = sh.ReplaceShorthandPatternWithFormalRegex(line);
            Assert.AreEqual(expectedResults, results);
        }
    }
}

