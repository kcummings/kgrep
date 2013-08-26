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
    }
}

