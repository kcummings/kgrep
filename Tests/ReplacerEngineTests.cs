using System.Collections.Generic;
using kgrep;
using NUnit.Framework;

namespace Tests {
    [TestFixture]
    public class ReplacerEngineTests {

        [Test]
        public void WhenEmptyReplacement_ExpectNoChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            Command command = new Command();
            Assert.AreEqual("abc", engine.ApplyCommandsFirstMatch("abc", command));
        }

        [Test]
        public void WhenEmptyTopatternMatches_ExpectTokenRemoved() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            Command command =  new Command("token", "");
            Assert.AreEqual("abc", engine.ApplyCommandsFirstMatch("abctoken", command));
        }

        [Test]
        public void WhenNoMatchFound_FirstOnly_ExpectNoChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            Command command = new Command("k", "def");
            Assert.AreNotEqual("", engine.ApplyCommandsFirstMatch("abc", command));
        }

        [TestCase("def", "abc", "def", "abc")]
        [TestCase("hll bye", "e(..)o-(...)", "$1 $2", "hello-bye")]
        [TestCase("attr=hi", "<tag attr='(.+?)'>", "attr=$1", "<tag attr='hi'>")]
        public void WhenSimpleRegex_ExpectSimpleResults(string expected, string frompattern, string topattern, string input) {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            Command command = new Command(frompattern, topattern);
            Assert.AreEqual(expected, engine.ApplyCommandsAllMatches(input, command));
        }

        [Test]
        public void WhenMatchesEndPoint_FirstOnly_ExpectChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            Command command = new Command("ab", "de");
            Assert.AreEqual("dele dee lincode", engine.ApplyCommandsFirstMatch("able abe lincoab", command));
        }

        [Test]
        public void WhenMatchsOnMultipleLines_All_ExpectCummulativeChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            List<Command> reps = new List<Command>();
            reps.Add(new Command("from", "to"));
            reps.Add(new Command("to", "this"));
            Assert.AreEqual("this me this you", engine.ApplyCommandsToLine("from me to you", reps));
        }

        [Test]
        public void WhenMatchesOnMultipleLinesWithGroups_ExpectChainedReplaces() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            List<Command> reps = new List<Command>();
            reps.Add(new Command("f(..)m", "$1"));
            reps.Add(new Command("to", "this"));
            Assert.AreEqual("ro me this you", engine.ApplyCommandsToLine("from me to you", reps));
        }

        [Test]
        public void WhenReplacmentSwapsGroups_FirstOnly_ExpectChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            List<Command> reps = new List<Command>();
            Command command = new Command(@"(\w+)\s(\w+)", "$2 $1");
            Assert.AreEqual("me from you to", engine.ApplyCommandsFirstMatch("from me to you", command));
        }

        [Test]
        public void WhenReplacmentSwapsGroups_All_ExpectChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            List<Command> reps = new List<Command>();
            Command command = new Command(@"(\w+)\s(\w+)", "$2 $1");
            Assert.AreEqual("me from you to", engine.ApplyCommandsAllMatches("from me to you", command));
        }

        [Test]
        public void WhenAnchorMatchesButPatternDoesNot_All_ExpectNoChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            Command command = new Command("today", "Joe", "to");
            Assert.AreEqual("you and you today", engine.ApplyCommandsAllMatches("you and you today", command));
        }

        [Test]
        public void WhenAnchorAndPatternMatches_All_ExpectChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            Command command = new Command("today", "you", "to");
            Assert.AreEqual("to and to today", engine.ApplyCommandsAllMatches("you and you today", command));
        }

        [Test]
        public void WhenAnchorAndPatternMatches_FirstOnly_ExpectChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            Command command = new Command("today", "you", "to");
            Assert.AreEqual("from me to to today", engine.ApplyCommandsFirstMatch("from me to you today", command));
        }

        [Test]
        public void WhenAnchorMatchesButPatternDoesNot_FirstOnly_ExpectNoChange() {
            ReplaceTokens engine = new ReplaceTokens() { sw = new WriteToString() };
            Command command = new Command("to", "Joe", "you");
            Assert.AreEqual("from me to you", engine.ApplyCommandsFirstMatch("from me to you", command));
        }

    }
}
