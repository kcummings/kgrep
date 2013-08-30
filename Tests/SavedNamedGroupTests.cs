using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using kgrep;

namespace Tests {

    [TestFixture]
    public class SavedNameGroupTests {

        [Test]
        public void WhenNoNamedGroupPresentInFromPart_ExpectCountZeroResults() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile("my (bye)~hi(?<title>[a-z]+) ");
            Assert.AreEqual(0, replacementCommands.ReplacementList[0].NamedGroupCount);
        }

        [Test]
        public void WhenOneNamedGroupPresent_ExpectCountOneResults() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() {sw = new WriteToString()};
            ParseReplacementFile replacementCommands = new ParseReplacementFile("my (?<title>[a-z]+) bye~hi");
            Assert.AreEqual(1, replacementCommands.ReplacementList[0].NamedGroupCount);
        }

        [Test]
        public void WhenTwoNamedGroupPresent_ExpectCountTwoResults() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile("my (?<title>[a-z]+) (?<title>[a-z]+)bye~hi");
            Assert.AreEqual(2, replacementCommands.ReplacementList[0].NamedGroupCount);
        }

        [Test]
        public void WhenNamedGroupPresentInMultipleLines_ExpectResults() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile("a~b; my (?<title>[a-z]+) (?<title>[a-z]+)bye~hi; hi~bye");
            Assert.AreEqual(0, replacementCommands.ReplacementList[0].NamedGroupCount);
            Assert.AreEqual(2, replacementCommands.ReplacementList[1].NamedGroupCount);
            Assert.AreEqual(0, replacementCommands.ReplacementList[2].NamedGroupCount);
        }

        [Test]
        [ExpectedException(typeof(System.Exception))]
        public void WhenInvalidNamedGroupPresent_ExpectException() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile("my (?<title) bye~hi");
            Assert.AreEqual(0, replacementCommands.ReplacementList[0].NamedGroupCount);
        }

        [Test]
        public void WhenOnePlaceholderGroupPresent_ExpectCountOneResults() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile("my (?<title>[a-z]+) (?<title>[a-z]+)bye~${title}hi");
            Assert.AreEqual(1, replacementCommands.ReplacementList[0].NamedGroupPlaceholderCount);
        }

        [Test]
        public void WhenTwoPlaceholderGroupPresent_ExpectCountTwoResults() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile("my (?<title>[a-z]+) (?<title>[a-z]+)bye~${title}hi${title}");
            Assert.AreEqual(2, replacementCommands.ReplacementList[0].NamedGroupPlaceholderCount);
        }

        [Test]
        public void WhenNoPlaceholderGroupPresent_ExpectZeroResults() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile("my bye~${titlehi");
            Assert.AreEqual(0, replacementCommands.ReplacementList[0].NamedGroupPlaceholderCount);
        }

        [Test]
        public void WhenTwoNamesGiven_ExpectTwo() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=all; ^(?<name>[a-z]+).*?(?<second>[0-9]+)~b");
            string newline = engine.ApplyReplacements(replacementCommands, new List<string> { "ab c 45" });
            var results = engine.NamedGroupValues["name"];
            Assert.AreEqual("ab", results);
            results = engine.NamedGroupValues["second"];
            Assert.AreEqual("45",results);
        }

        [Test]
        public void WhenNameRepeats_ExpectLastValue() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=all; ^(?<name>[a-z]+).*?(?<name>[0-9]+)~b");
            string newline = engine.ApplyReplacements(replacementCommands, new List<string> { "ab c 45" });
            var results = engine.NamedGroupValues["name"];
            Assert.AreEqual("45", results);
        }

        [Test]
        public void WhenFirstNameRepeats_ExpectLastValue() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=first; ^(?<name>[a-z]+).*?(?<name>[0-9]+)~b");
            string newline = engine.ApplyReplacements(replacementCommands, new List<string> { "ab c 45; ab 98" });
            var results = engine.NamedGroupValues["name"];
            Assert.AreEqual("45", results);
        }

        [Test]
        public void WhenPlaceholderPresent_ExpectReplacedValue() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=all; ^(?<name>[a-z]+)~c${name}");
            string results = engine.ApplyReplacements(replacementCommands, new List<string> { "a" });
            Assert.AreEqual("ca\n", results);
        }

        [Test]
        public void WhenRepeatPlaceholderPresent_ExpectReplacedValue() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=all; ^(?<name>[a-z]+)~c${name}${name}");
            string results = engine.ApplyReplacements(replacementCommands, new List<string> { "a" });
            Assert.AreEqual("caa\n", results);
        }

        [Test]
        public void WhenPlaceholderSpansLines_ExpectReplacedValue() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=all; ^(?<name>[a-z]+)~blue ${name};blue ~${name}");
            string results = engine.ApplyReplacements(replacementCommands, new List<string> { "word" });
            Assert.AreEqual("word word\n", results);
        }

        [Test]
        public void WhenTwoPlaceholderSpansLines_ExpectReplacedValue() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=all; ^(?<name>[a-z]+)~blue ${name};(?<digit>[0-9]+) ~${name}; blue~ ${name}${digit}");
            string results = engine.ApplyReplacements(replacementCommands, new List<string> {  "ab 89 cd" });
            Assert.AreEqual("ab89 ab ab cd\n", results);
        }

        [Test]
        [Ignore]
        public void WhenTwoPlaceholderSpansLinesWithMultipleReferences_ExpectReplacedValue() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            ParseReplacementFile replacementCommands = new ParseReplacementFile(@"scope=all; ^(?<tag>\<chap=([a-z]+?)\>); end~${tag}");
            string results = engine.ApplyReplacements(replacementCommands, new List<string> { "<chap=a> <last=z> end" });
            Assert.AreEqual("ab89 ab ab cd\n", results);
        }

        [Test]
        public void WhenPlaceholderInCollection_ExpectReplacedValue() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles() { sw = new WriteToString() };
            Dictionary<string, string> groupValues = new Dictionary<string, string>(){ {"a", "hi" },{"b","bye"} };
            string results = engine.ReplaceRegexPlaceholdersIfPresent("${a}", groupValues);
            Assert.AreEqual("hi", results);
            results = engine.ReplaceRegexPlaceholdersIfPresent("${b}", groupValues);
            Assert.AreEqual("bye", results);
        }
    }
}

