using System.Collections.Generic;
using kgrep;
using NUnit.Framework;

namespace Tests {
    [TestFixture]
    public class ReplacerEngineTests {

        [Test]
        public void WhenEmptyReplacement_ExpectNoChange() {
            ReplaceTokensFirstOccurrence engine = new ReplaceTokensFirstOccurrence();
            List<Command> reps = new List<Command>();
            Assert.AreEqual("abc", engine.ApplyCommandsFirstMatch("abc", reps));
        }

        [Test]
        public void WhenEmptyTopatternMatches_ExpectTokenRemoved() {
            ReplaceTokensFirstOccurrence engine = new ReplaceTokensFirstOccurrence();
            List<Command> reps = new List<Command>();
            reps.Add(new Command("token", ""));
            Assert.AreEqual("abc", engine.ApplyCommandsFirstMatch("abctoken", reps));
        }

        [Test]
        public void WhenNoMatchFound_FirstOnly_ExpectNoChange() {
            ReplaceTokensFirstOccurrence engine = new ReplaceTokensFirstOccurrence();
            List<Command> reps = new List<Command>();
            reps.Add(new Command("k", "def"));
            Assert.AreNotEqual("", engine.ApplyCommandsFirstMatch("abc", reps));
        }

        [TestCase("def", "abc", "def", "abc")]
        [TestCase("hll bye", "e(..)o-(...)", "$1 $2", "hello-bye")]
        [TestCase("attr=hi","<tag attr='(.+?)'>", "attr=$1","<tag attr='hi'>")]
        public void WhenSimpleRegex_ExpectSimpleResults(string expected, string frompattern, string topattern, string input) {
            ReplaceTokensAllOccurrences engine = new ReplaceTokensAllOccurrences();
            List<Command> reps = new List<Command>();
            reps.Add(new Command(frompattern, topattern));
            Assert.AreEqual(expected, engine.ApplyCommandsAllMatches(input, reps));
        }

        [Test]
        public void WhenMatchesEndPoint_FirstOnly_ExpectChange() {
            ReplaceTokensFirstOccurrence engine = new ReplaceTokensFirstOccurrence();
            List<Command> reps = new List<Command>();
            reps.Add(new Command("ab", "de"));
            Assert.AreEqual("dele dee lincode",engine.ApplyCommandsFirstMatch("able abe lincoab", reps));
        }

        [Test]
        public void WhenMatchesOnMultipleLines_FirstOnly_ExpectFirstReplaceOnly() {
            ReplaceTokensFirstOccurrence engine = new ReplaceTokensFirstOccurrence();
            List<Command> reps = new List<Command>();
            reps.Add(new Command("from", "to"));
            reps.Add(new Command("to", "this"));
            Assert.AreEqual("to me to you", engine.ApplyCommandsFirstMatch("from me to you", reps));
        }

        [Test]
        public void WhenMatchsOnMultipleLines_All_ExpectCummulativeChange() {
            ReplaceTokensAllOccurrences engine = new ReplaceTokensAllOccurrences();
            List<Command> reps = new List<Command>();
            reps.Add(new Command("from", "to"));
            reps.Add(new Command("to", "this"));
            Assert.AreEqual("this me this you", engine.ApplyCommandsAllMatches("from me to you", reps));
        }

        [Test]
        public void WhenMatchesOnMultipleLinesWithGroups_FirstOnly_ExpectFirstReplaceOnly() {
            ReplaceTokensFirstOccurrence engine = new ReplaceTokensFirstOccurrence();
            List<Command> reps = new List<Command>();
            reps.Add(new Command("f(..)m", "$1"));
            reps.Add(new Command("to", "this"));
            Assert.AreEqual("ro me to you", engine.ApplyCommandsFirstMatch("from me to you", reps));
        }

        [Test]
        public void WhenReplacmentWithMultipleLinesSwappingGroups_FirstOnly_ExpectChange() {
            ReplaceTokensFirstOccurrence engine = new ReplaceTokensFirstOccurrence();
            List<Command> reps = new List<Command>();
            reps.Add(new Command(@"(\w+)\s(\w+)", "$2 $1"));
            reps.Add(new Command("to", "this"));
            Assert.AreEqual("me from you to", engine.ApplyCommandsFirstMatch("from me to you", reps));
        }

        [Test]
        public void WhenReplacmentWithMultipleLinesSwappingGroups_All_ExpectChange() {
            ReplaceTokensAllOccurrences engine = new ReplaceTokensAllOccurrences();
            List<Command> reps = new List<Command>();
            reps.Add(new Command(@"(\w+)\s(\w+)", "$2 $1"));
            reps.Add(new Command("to", "this"));
            Assert.AreEqual("me from you this", engine.ApplyCommandsAllMatches("from me to you", reps));
        }

        [Test]
        public void WhenAnchorMatchesButPatternDoesNot_All_ExpectNoChange() {
            ReplaceTokensAllOccurrences engine = new ReplaceTokensAllOccurrences();
            List<Command> reps = new List<Command>();
            reps.Add(new Command("today", "Joe", "to"));
            Assert.AreEqual("you and you today", engine.ApplyCommandsAllMatches("you and you today", reps));
        }

        [Test]
        public void WhenAnchorAndPatternMatches_All_ExpectChange() {
            ReplaceTokensAllOccurrences engine = new ReplaceTokensAllOccurrences();
            List<Command> reps = new List<Command>();
            reps.Add(new Command("today", "you", "to"));
            Assert.AreEqual("to and to today", engine.ApplyCommandsAllMatches("you and you today", reps));
        }

        [Test]
        public void WhenAnchorAndPatternMatches_FirstOnly_ExpectChange() {
            ReplaceTokensFirstOccurrence engine = new ReplaceTokensFirstOccurrence();
            List<Command> reps = new List<Command>();
            reps.Add(new Command("today","you", "to"));
            Assert.AreEqual("from me to to today", engine.ApplyCommandsFirstMatch("from me to you today", reps));
        }

        [Test]
        public void WhenAnchorMatchesButPatternDoesNot_FirstOnly_ExpectNoChange() {
            ReplaceTokensFirstOccurrence engine = new ReplaceTokensFirstOccurrence();
            List<Command> reps = new List<Command>();
            reps.Add(new Command("to", "Joe", "you"));
            Assert.AreEqual("from me to you", engine.ApplyCommandsFirstMatch("from me to you", reps));
        }

    }
}
