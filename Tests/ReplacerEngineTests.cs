using System.Collections.Generic;
using kgrep;
using NUnit.Framework;

namespace Tests {
    [TestFixture]
    public class ReplacerEngineTests {

        [Test]
        public void WhenEmptyReplacement_ExpectNoChange() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            List<Replacement> reps = new List<Replacement>();
            Assert.AreEqual("abc", engine.ApplyReplacementsFirst("abc", reps));
        }

        [Test]
        public void WhenEmptyTopatternMatches_ExpectTokenRemoved() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("token", ""));
            Assert.AreEqual("abc", engine.ApplyReplacementsFirst("abctoken", reps));
        }

        [Test]
        public void WhenNoMatchFound_FirstOnly_ExpectNoChange() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("k", "def"));
            Assert.AreNotEqual("", engine.ApplyReplacementsFirst("abc", reps));
        }

        [TestCase("def", "abc", "def", "abc")]
        [TestCase("hll bye", "e(..)o-(...)", "$1 $2", "hello-bye")]
        [TestCase("attr=hi","<tag attr='(.+?)'>", "attr=$1","<tag attr='hi'>")]
        public void WhenSimpleRegex_ExpectSimpleResults(string expected, string frompattern, string topattern, string input) {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement(frompattern, topattern));
            Assert.AreEqual(expected, engine.ApplyReplacementsAll(input, reps));
        }

        [Test]
        public void WhenMatchesEndPoint_FirstOnly_ExpectChange() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("ab", "de"));
            Assert.AreEqual("dele dee lincode",engine.ApplyReplacementsFirst("able abe lincoab", reps));
        }

        [Test]
        public void WhenMatchesOnMultipleLines_FirstOnly_ExpectFirstReplaceOnly() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("from", "to"));
            reps.Add(new Replacement("to", "this"));
            Assert.AreEqual("to me to you", engine.ApplyReplacementsFirst("from me to you", reps));
        }

        [Test]
        public void WhenMatchsOnMultipleLines_All_ExpectCummulativeChange() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("from", "to"));
            reps.Add(new Replacement("to", "this"));
            Assert.AreEqual("this me this you", engine.ApplyReplacementsAll("from me to you", reps));
        }

        [Test]
        public void WhenMatchesOnMultipleLinesWithGroups_FirstOnly_ExpectFirstReplaceOnly() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("f(..)m", "$1"));
            reps.Add(new Replacement("to", "this"));
            Assert.AreEqual("ro me to you", engine.ApplyReplacementsFirst("from me to you", reps));
        }

        [Test]
        public void WhenReplacmentWithMultipleLinesSwappingGroups_FirstOnly_ExpectChange() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement(@"(\w+)\s(\w+)", "$2 $1"));
            reps.Add(new Replacement("to", "this"));
            Assert.AreEqual("me from you to", engine.ApplyReplacementsFirst("from me to you", reps));
        }

        [Test]
        public void WhenReplacmentWithMultipleLinesSwappingGroups_All_ExpectChange() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement(@"(\w+)\s(\w+)", "$2 $1"));
            reps.Add(new Replacement("to", "this"));
            Assert.AreEqual("me from you this", engine.ApplyReplacementsAll("from me to you", reps));
        }

        [Test]
        public void WhenAnchorMatchesButPatternDoesNot_All_ExpectNoChange() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("today", "Joe", "to"));
            Assert.AreEqual("you and you today", engine.ApplyReplacementsAll("you and you today", reps));
        }

        [Test]
        public void WhenAnchorAndPatternMatches_All_ExpectChange() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("today", "you", "to"));
            Assert.AreEqual("to and to today", engine.ApplyReplacementsAll("you and you today", reps));
        }

        [Test]
        public void WhenAnchorAndPatternMatches_FirstOnly_ExpectChange() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("today","you", "to"));
            Assert.AreEqual("from me to to today", engine.ApplyReplacementsFirst("from me to you today", reps));
        }

        [Test]
        public void WhenAnchorMatchesButPatternDoesNot_FirstOnly_ExpectNoChange() {
            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("to", "Joe", "you"));
            Assert.AreEqual("from me to you", engine.ApplyReplacementsFirst("from me to you", reps));
        }

    }
}
