using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using kgrep;

namespace Tests {

    [TestFixture]
    public class PickupTests {

        [Test]
        public void WhenNoPickupPresentInFromPart_ExpectCountZeroResults() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile("my (bye)~hi(?<title>[a-z]+) ");
            Assert.AreEqual(0, replacementCommands.ReplacementList[0].PickupCount);
        }

        [Test]
        public void WhenOnePickupPresent_ExpectCountOneResults() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() {sw = new WriteToString()};
            ParseReplacementFile replacementCommands = new ParseReplacementFile("my (?<title>[a-z]+) bye~hi");
            Assert.AreEqual(1, replacementCommands.ReplacementList[0].PickupCount);
        }

        [Test]
        public void WhenTwoPickupsPresent_ExpectCountTwoResults() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile("my (?<title>[a-z]+) (?<title>[a-z]+)bye~hi");
            Assert.AreEqual(2, replacementCommands.ReplacementList[0].PickupCount);
        }

        [Test]
        public void WhenPickupPresentInMultipleLines_ExpectResults() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile("a~b; my (?<title>[a-z]+) (?<title>[a-z]+)bye~hi; hi~bye");
            Assert.AreEqual(0, replacementCommands.ReplacementList[0].PickupCount);
            Assert.AreEqual(2, replacementCommands.ReplacementList[1].PickupCount);
            Assert.AreEqual(0, replacementCommands.ReplacementList[2].PickupCount);
        }

        [Test]
        [ExpectedException(typeof(System.Exception))]
        public void WhenInvalidPickupPresent_ExpectException() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile("my (?<title) bye~hi");
            Assert.AreEqual(0, replacementCommands.ReplacementList[0].PickupCount);
        }

        [Test]
        public void WhenOnePickupPlaceholderPresent_ExpectCountOneResults() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile("my (?<title>[a-z]+) (?<title>[a-z]+)bye~${title}hi");
            Assert.AreEqual(1, replacementCommands.ReplacementList[0].PickupPlaceholderCount);
        }

        [Test]
        public void WhenTwoPickupPlaceholderPresent_ExpectCountTwoResults() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile("my (?<title>[a-z]+) (?<title>[a-z]+)bye~${title}hi${title}");
            Assert.AreEqual(2, replacementCommands.ReplacementList[0].PickupPlaceholderCount);
        }

        [Test]
        public void WhenNoPickupPlaceholderPresent_ExpectZeroResults() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile("my bye~${titlehi");
            Assert.AreEqual(0, replacementCommands.ReplacementList[0].PickupPlaceholderCount);
        }

        [Test]
        public void WhenTwoPickupsGiven_ExpectTwo() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=all; ^(?<name>[a-z]+).*?(?<second>[0-9]+)~b");
            string newline = engine.ApplyReplacements(replacementCommands, new List<string> { "ab c 45" });
            var results = engine.PickupList["name"];
            Assert.AreEqual("ab", results);
            results = engine.PickupList["second"];
            Assert.AreEqual("45",results);
        }

        [Test]
        public void WhenPickupRepeats_ExpectLastValue() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=all; ^(?<name>[a-z]+).*?(?<name>[0-9]+)~b");
            string newline = engine.ApplyReplacements(replacementCommands, new List<string> { "ab c 45" });
            var results = engine.PickupList["name"];
            Assert.AreEqual("45", results);
        }

        [Test]
        public void WhenFirstPickupRepeats_ExpectLastValue() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=first; ^(?<name>[a-z]+).*?(?<name>[0-9]+)~b");
            string newline = engine.ApplyReplacements(replacementCommands, new List<string> { "ab c 45; ab 98" });
            var results = engine.PickupList["name"];
            Assert.AreEqual("45", results);
        }

        [Test]
        public void WhenPickupPlaceholderPresent_ExpectReplacedValue() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=all; ^(?<name>[a-z]+)~c${name}");
            string results = engine.ApplyReplacements(replacementCommands, new List<string> { "a" });
            Assert.AreEqual("ca\n", results);
        }

        [Test]
        public void WhenPickupPlaceholderRepeats_ExpectReplacedValue() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=all; ^(?<name>[a-z]+)~c${name}${name}");
            string results = engine.ApplyReplacements(replacementCommands, new List<string> { "a" });
            Assert.AreEqual("caa\n", results);
        }

        [Test]
        public void WhenPickupPlaceholderSpansLines_ExpectReplacedValue() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=all; ^(?<name>[a-z]+)~blue ${name};blue ~${name}");
            string results = engine.ApplyReplacements(replacementCommands, new List<string> { "word" });
            Assert.AreEqual("word word\n", results);
        }

        [Test]
        public void WhenTwoPickupPlaceholderSpansLines_ExpectReplacedValue() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=all; ^(?<name>[a-z]+)~blue ${name};(?<digit>[0-9]+) ~${name}; blue~ ${name}${digit}");
            string results = engine.ApplyReplacements(replacementCommands, new List<string> {  "ab 89 cd" });
            Assert.AreEqual("ab89 ab ab cd\n", results);
        }

        [Test]
        public void WhenTwoPickupPlaceholderSpansLinesWithMultipleReferences_ExpectReplacedValue() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=all; ^<chap=(?<tag>[a-z]+?)>~; end~chapter=${tag}");
            string results = engine.ApplyReplacements(replacementCommands, new List<string> { "<chap=a><last=z> end" });
            Assert.AreEqual("<last=z> chapter=a\n", results);
        }

        [Test]
        public void WhenOverlappingPickups_ExpectOverlappedResults() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands =
                new ParseReplacementFile(@"scope=all; ^(?<firstname>[a-z]+)\.; \.(?<lastname>[a-z]+)@; @(?<company>[a-z.]+); ~${company}@${lastname}.${firstname}");
            string results = engine.ApplyReplacements(replacementCommands, new List<string> { "john.doe@linux.org" });
            Assert.AreEqual("linux.org@doe.john\n", results);
        }

        [Test]
        public void WhenOverlappingPickupsFirst_ExpectOverlappedResults() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands =
                new ParseReplacementFile(@"scope=first; ^(?<firstname>[a-z]+)\.; \.(?<lastname>[a-z]+)@; @(?<company>[a-z.]+); ~${company}@${lastname}.${firstname}");
            string results = engine.ApplyReplacements(replacementCommands, new List<string> { "john.doe@linux.org" });
            Assert.AreEqual("linux.org@doe.john\n", results);
        }

        [Test]
        public void WhenTwoPickupPlaceholderSpansLinesFirst_ExpectFirstReplacedValue() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=first; ^(?<name>[a-z]+)~blue ${name};(?<digit>[0-9]+) ~${name}; blue~ ${name}${digit}");
            string results = engine.ApplyReplacements(replacementCommands, new List<string> { "ab 89 cd", "ab 89 cd" });
            Assert.AreEqual("blue ab 89 cd\nblue ab 89 cd\n", results);
        }
    }
}

