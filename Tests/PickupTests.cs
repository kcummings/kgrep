using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using kgrep;

namespace Tests {

    [TestFixture]
    public class PickupTests {

        [Test]
        public void WhenNoCapturePresentInSubjectString_ExpectCountZeroResults() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my bye~hi(?<title>[a-z]+) ");
            Assert.AreEqual(0, commands.CommandList[0].CountOfCapturesInSubjectString);
        }

        [Test]
        public void WhenOneCapturePresent_ExpectCountOneResults() {
            ReplaceAllMatches engine = new ReplaceAllMatches() {sw = new WriteToString()};
            ParseCommandFile commands = new ParseCommandFile("my (?<title>[a-z]+) bye~hi");
            Assert.AreEqual(1, commands.CommandList[0].CountOfCapturesInSubjectString);
        }

        [Test]
        // If the same name is given to more than one capture in the SubjectString, the pickup's value will be the first match.
        // e.g. given source line "ab cd ed" and SubjectString "(?<letter>[a-z]+)", ${letter} will be "ab", not "ed". 
        public void WhenTwoCapturesPresent_ExpectCountTwoResults() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my (?<title>[a-z]+) (?<title>[a-z]+)bye~hi");
            Assert.AreEqual(2, commands.CommandList[0].CountOfCapturesInSubjectString);
        }

        [Test]
        [ExpectedException(typeof(System.Exception))]
        public void WhenInvalidCapturePresent_ExpectException() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my (?<title) bye~hi");
            Assert.AreEqual(0, commands.CommandList[0].CountOfCapturesInSubjectString);
        }

        [Test]
        public void WhenReplacementPickupPresent_ExpectResults() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my bye~hi ${title}");
            Assert.AreEqual(1, commands.CommandList[0].CountOfPickupsInReplacementString);
        }

        [Test]
        public void WhenReplacementAndSubjectPickupPresent_ExpectResults() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("${1}my bye~hi ${title}");
            Assert.AreEqual(1, commands.CommandList[0].CountOfPickupsInReplacementString);
            Assert.AreEqual(1, commands.CommandList[0].CountOfPickupsInSubjectString);
        }

        [Test]
        public void WhenInvalidReplacementPickupPresent_ExpectResults() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my bye~hi {title}$");
            Assert.AreEqual(0, commands.CommandList[0].CountOfPickupsInReplacementString);
            Assert.AreEqual(0, commands.CommandList[0].CountOfPickupsInSubjectString);
        }

        [Test]
        public void WhenOneCapturePresent_ExpectResults() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my (?<title>[a-z]+) (?<title>[a-z]+)bye~${title}hi");
            Assert.AreEqual(1, commands.CommandList[0].CountOfPickupsInReplacementString);
        }

        [Test]
        public void WhenNoPickupPresent_ExpectZeroResults() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("my bye~${titlehi");
            Assert.AreEqual(0, commands.CommandList[0].CountOfPickupsInReplacementString);
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
        // TODO: Possibly need to adopt ${one} syntax for unnamed matches rather thean ${1} to avoid collison with regex unnamed capture syntax
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
        public void WhenOverlappingPickups_ExpectOverlappedResults() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands =
                new ParseCommandFile(@"scope=all; ^(?<firstname>[a-z]+)\.; \.(?<lastname>[a-z]+)@; @(?<company>[a-z.]+); ~${company}@${lastname}.${firstname}");
            string results = engine.ApplyCommands(commands, new List<string> { "john.doe@linux.org" });
            Assert.AreEqual("linux.org@doe.john\n", results);
        }

        [Test]
        public void WhenOverlappingPickupsFirst_ExpectOverlappedResults() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands =
                new ParseCommandFile(@"scope=first; ^(?<firstname>[a-z]+)\.; \.(?<lastname>[a-z]+)@; @(?<company>[a-z.]+); ~${company}@${lastname}.${firstname}");
            string results = engine.ApplyCommands(commands, new List<string> { "john.doe@linux.org" });
            Assert.AreEqual("linux.org@doe.john\n", results);
        }

        [Test]
        public void WhenTwoPickupSpansLinesFirst_ExpectFirstReplacedValue() {
            ReplaceFirstMatch engine = new ReplaceFirstMatch() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"scope=first; ^(?<name>[a-z]+)~blue ${name};(?<digit>[0-9]+) ~${name}; blue~ ${name}${digit}");
            string results = engine.ApplyCommands(commands, new List<string> { "ab 89 cd", "ab 89 cd" });
            Assert.AreEqual("blue ab 89 cd\nblue ab 89 cd\n", results);
        }

        [Test]
        public void WhenMatchedNamedAndUnnamedCaptures_ExpectReplacedValue() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile(@"scope=all; (?<name>[a-z]+).*([0-9])$~blue;~${1}--${name}");
            string results = engine.ApplyCommands(commands, new List<string> { "ab 12" });
            Assert.AreEqual("2--ab\n", results);
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
            Assert.AreEqual(Command.CommandType.Print, commands.CommandList[2].Style);
            Assert.AreEqual(Command.CommandType.Anchored, commands.CommandList[3].Style);
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
        [TestCase("~a  ")]
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

        [Test]
        public void WhenPickupPresentInMultipleLines_ExpectResults() {
            ReplaceAllMatches engine = new ReplaceAllMatches() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("(a)~b; my (?<title>[a-z]+) (?<title>[a-z]+)bye~hi; hi~bye");
            Assert.AreEqual(1, commands.CommandList[0].CountOfCapturesInSubjectString);
            Assert.AreEqual(2, commands.CommandList[1].CountOfCapturesInSubjectString);
            Assert.AreEqual(0, commands.CommandList[2].CountOfCapturesInSubjectString);
        }
    }
}

