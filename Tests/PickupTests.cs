using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using kgrep;

namespace Tests {

    [TestFixture]
    public class PickupTests {

        [TestCase(false, "nothing here")]   // no captures present
        //[TestCase(false, "one (?# comment)")]   // comment
        //[TestCase(false, @"(?<=NUM:)\d+|\w+")]  // if then/else
        //[TestCase(false, @"(?<!this)")]         // lookbehind
        //[TestCase(false, @"(?<=this)")]         // lookahead
        //[TestCase(false, @"(?>.*?)")]           // atomic grouping
        //[TestCase(false, @"(?i:very)")]         // mode modifier
        public void WhenPatternGiven_DoNotFindCaptures(bool expected, string cmd) {
            ReplaceFirstMatch engine = new ReplaceFirstMatch();
            List<Command> reps = new List<Command>();
            reps.Add(new Command(cmd));
            Assert.IsFalse(reps[0].IsCaptureInSubjectString);
        }

        [TestCase(true, "one (?:and|or) (two)")]  // grouping-only parentheses
        [TestCase(true, @"(abc) the art (?<mytag>of) today is (?i:very) (?#new)")]         // mode modifier
        public void WhenPatternGiven_FindCaptures(bool expected, string cmd) {
            ReplaceFirstMatch engine = new ReplaceFirstMatch();
            List<Command> reps = new List<Command>();
            reps.Add(new Command(cmd));
            Assert.IsTrue(reps[0].IsCaptureInSubjectString);
        }

        [Test]
        [ExpectedException(typeof(System.Exception))]
        public void WhenInvalidCapturePresent_ExpectException() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my (?<title) bye~hi");
            Assert.IsFalse(commands.CommandList[0].IsCaptureInSubjectString);
        }

        [Test]
        public void WhenReplacementPickupPresent_ExpectResults() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my bye~hi ${title}");
            Assert.IsTrue(commands.CommandList[0].IsPickupInReplacementString);
        }

        [Test]
        public void WhenInvalidReplacementPickupPresent_ExpectResults() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my bye~hi {title}$");
            Assert.IsFalse(commands.CommandList[0].IsPickupInReplacementString);
        }

        [Test]
        public void WhenTwoCapturesGiven_ExpectTwo() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"scope=all; ^(?<name>[a-z]+).*?(?<second>[0-9]+)~b");
            string newline = engine.ApplyCommands(commands, new List<string> { "ab c 45" });
            var results = engine.PickupList["name"];
            Assert.AreEqual("ab", results);
            results = engine.PickupList["second"];
            Assert.AreEqual("45",results);
        }

        [Test]
        public void WhenSameCaptureNameReused_ExpectLastValue() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"scope=all; ^(?<name>[a-z]+).*?(?<name>[0-9]+)~b${name} done");
            string newline = engine.ApplyCommands(commands, new List<string> { "ab c 45" });
            var results = engine.PickupList["name"];
            Assert.AreEqual("45", results);
            Assert.AreEqual("b45 done\n",newline);
        }

        [Test]
        public void WhenUnnamedCaptureReused_ExpectLastValue() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"scope=all; ^(\w+);(\w+)$; White~${1}");
            string newline = engine.ApplyCommands(commands, new List<string> { "isolation and White box Unit testing" });
            var results = engine.PickupList["1"];
            Assert.AreEqual("testing", results);
            Assert.AreEqual("isolation and testing box Unit testing\n", newline);
        }

        [Test]
        public void WhenFirstCaptureRepeats_ExpectLastValue() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"scope=first; ^(?<name>[a-z]+).*?(?<name>[0-9]+)~b");
            string newline = engine.ApplyCommands(commands, new List<string> { "ab c 45; ab 98" });
            var results = engine.PickupList["name"];
            Assert.AreEqual("45", results);
        }

        [Test]
        public void WhenPickupPresent_ExpectReplacedValue() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"scope=all; ^(?<name>[a-z]+)~c${name}");
            string results = engine.ApplyCommands(commands, new List<string> { "a" });
            Assert.AreEqual("ca\n", results);
        }

        [Test]
        public void WhenPickupRepeats_ExpectReplacedValues() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"scope=all; ^(?<name>[a-z]+)~c${name}${name}");
            string results = engine.ApplyCommands(commands, new List<string> { "a" });
            Assert.AreEqual("caa\n", results);
        }

        [Test]
        public void WhenPickupSpansLines_ExpectReplacedValue() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"scope=all; ^(?<name>[a-z]+)~blue ${name};blue ~${name}");
            string results = engine.ApplyCommands(commands, new List<string> { "word" });
            Assert.AreEqual("word word\n", results);
        }

        [Test]
        public void WhenPickupAndRegexGiven_ExpectBothReplaced() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"scope=all; ^(\w)(.);..(.)~$1${2}");
            string results = engine.ApplyCommands(commands, new List<string> { "abc" });
            Assert.AreEqual("cb\n", results);
            results = engine.PickupList["2"];
            Assert.AreEqual("b", results);
        }

        [Test]
        public void WhenTwoPickupSpansLines_ExpectReplacedValue() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"scope=all; ^(?<name>[a-z]+)~blue ${name};(?<digit>[0-9]+) ~${name}; blue~ ${name}${digit}");
            string results = engine.ApplyCommands(commands, new List<string> {  "ab 89 cd" });
            Assert.AreEqual("ab89 ab ab cd\n", results);
        }

        [Test]
        public void WhenTwoPickupSpansLinesWithMultipleReferences_ExpectReplacedValue() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"scope=all; ^<chap=(?<tag>[a-z]+?)>~; end~chapter=${tag}");
            string results = engine.ApplyCommands(commands, new List<string> { "<chap=a><last=z> end" });
            Assert.AreEqual("<last=z> chapter=a\n", results);
        }

        [Test]
        public void WhenTwoPickupSpansLinesFirst_ExpectFirstReplacedValue() {
            ReplaceFirstMatch engine = new ReplaceFirstMatch() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"scope=first; ^(?<name>[a-z]+)~blue ${name};(?<digit>[0-9]+) ~${name}; blue~ ${name}${digit}");
            string results = engine.ApplyCommands(commands, new List<string> { "ab 89 cd", "ab 89 cd" });
            Assert.AreEqual("blue ab 89 cd\nblue ab 89 cd\n", results);
        }

        [Test]
        public void WhenMatchedNamedAndUnnamedPickups_ExpectReplacedValue() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"scope=all; (?<letter>[a-z]+).([0-9]);12~${1}${letter}");
            string results = engine.ApplyCommands(commands, new List<string> { "ab 12" });
            Assert.AreEqual("ab 1ab\n", results);
        }

        [Test]
        public void WhenDifferentNamedPickups_ExpectReplacedValue() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"scope=all; (?<name>[a-z]+) (?<name2>[a-z]+);(?<digit>[0-9]+)~${name}");
            string results = engine.ApplyCommands(commands, new List<string> { "ab cd", "89ab" });
            Assert.AreEqual("ab cd\nabab\n", results);
        }

        [Test]
        public void WhenNamedPickupChanged_ExpectReplacedValue() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"scope=all; ^(?<name>[a-z]+)~blue;(?<digit>[0-9]+)~${name}");
            string results = engine.ApplyCommands(commands, new List<string> { "ab cd", "89ab" });
            Assert.AreEqual("blue cd\nabab\n", results);
        }

        [Test]
        public void WhenVariousTokensGiven_ExpectAppropiateCommandTypes() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"a~b; a; ~b; d~a~b");
            Assert.AreEqual(Command.CommandType.Normal, commands.CommandList[0].Style);
            Assert.AreEqual(Command.CommandType.Pickup, commands.CommandList[1].Style);
            Assert.AreEqual(Command.CommandType.Anchored, commands.CommandList[2].Style);
            Assert.AreEqual(ParseCommandFile.RunningAs.ReplaceAll, commands.kgrepMode);
        }

        [Test]
        public void WhenOnlyScanTokensGiven_ExpectScannerMode() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"b; a; c");
            Assert.AreEqual(ParseCommandFile.RunningAs.Scanner, commands.kgrepMode);
        }

        [Test]
        [TestCase("a~b")]
        [TestCase("~a~")]
        [TestCase("a~")]
        public void WhenValidFieldContent_ExpectValidCommand(string line) {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(line);
            Assert.AreEqual(1, commands.CommandList.Count);
        }

        [Test]
        [TestCase("~~~")]
        [TestCase("~~~~")]
        [TestCase("~")]
        [TestCase("a~ ~")]  // only anchor supplied
        [TestCase(@"a~b~c~d~e")]
        [TestCase("delim=:::::")]
        [TestCase("delim=:::")]
        [TestCase("a~~#abc")]
        [TestCase("~a  ")]
        public void WhenInvalidFieldContent_ExpectInvalidCommand(string line) {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(line);
            Assert.AreEqual(0, commands.CommandList.Count);
        }

        [Test]
        [ExpectedException(typeof(System.Exception))]
        [TestCase("[~]")]
        [TestCase("(~)")]
        public void WhenInvalidCommandSyntax_ExpectException(string line) {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(line);
        }
    }
}

