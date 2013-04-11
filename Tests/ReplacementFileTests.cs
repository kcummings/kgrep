using System.Collections.Generic;
using kgrep;
using NUnit.Framework;

namespace Tests {
    [TestFixture]
    public class ReplacementTests {

        [Test]
        public void TestRegexReplacement() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("e(..)o-(...)", "$1 $2"));
            Assert.AreEqual("hll bye", engine.ApplyReplacementsAll("hello-bye", reps));
        }

        [Test]
        public void TestRegexExtractReplacement() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("<tag attr='(.+?)'>", "attr=$1"));
            Assert.AreEqual("attr=hi", engine.ApplyReplacementsAll("<tag attr='hi'>", reps));
        }

        [Test]
        public void TestEmptyReplacementSet() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            Assert.AreEqual( "abc",engine.ApplyReplacementsFirst("abc", reps));
        }

        [Test]
        public void TestSimpleReplacementToRemoveToken() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("token", ""));
            Assert.AreEqual("abc", engine.ApplyReplacementsFirst("abctoken", reps));
        }

        [Test]
        public void TestSimpleReplacementNoMatch() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("k", "def"));
            Assert.AreNotEqual("", engine.ApplyReplacementsFirst("abc", reps));
        }

        [Test]
        public void TestSimpleReplacement() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("abc", "def"));
            Assert.AreEqual("def",engine.ApplyReplacementsFirst("abc", reps));
        }

        [Test]
        public void TestEndPointReplacements() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("ab", "de"));
            Assert.AreEqual("dele dee lincode",engine.ApplyReplacementsFirst("able abe lincoab", reps));
        }

        [Test]
        public void TestFirstReplacmentWithMultipleLines() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("from", "to"));
            reps.Add(new Replacement("to", "this"));
            Assert.AreEqual("to me to you", engine.ApplyReplacementsFirst("from me to you", reps));
        }

        [Test]
        public void TestAllReplacmentWithMultipleLines() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("from", "to"));
            reps.Add(new Replacement("to", "this"));
            Assert.AreEqual("this me this you", engine.ApplyReplacementsAll("from me to you", reps));
        }

        [Test]
        public void TestFirstReplacmentWithMultipleLinesWithGroups() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("f(..)m", "$1"));
            reps.Add(new Replacement("to", "this"));
            Assert.AreEqual("ro me to you", engine.ApplyReplacementsFirst("from me to you", reps));
        }

        [Test]
        public void TestFirstReplacmentWithMultipleLinesSwappingGroups() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement(@"(\w+)\s(\w+)", "$2 $1"));
            reps.Add(new Replacement("to", "this"));
            Assert.AreEqual("me from you to", engine.ApplyReplacementsFirst("from me to you", reps));
        }

        [Test]
        public void TestAllReplacmentWithMultipleLinesSwappingGroups() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement(@"(\w+)\s(\w+)", "$2 $1"));
            reps.Add(new Replacement("to", "this"));
            Assert.AreEqual("me from you this", engine.ApplyReplacementsAll("from me to you", reps));
        }

        [Test]
        public void TestWithAnchor() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("today","you", "to"));
            Assert.AreEqual("from me to to today", engine.ApplyReplacementsFirst("from me to you today", reps));
        }

        [Test]
        public void TestSuccessWithAnchorAsRegex() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("t...y", "you", "to"));
            Assert.AreEqual("from me to to today", engine.ApplyReplacementsFirst("from me to you today", reps));
        }

        [Test]
        public void TestFailureWithAnchorAsRegex() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("to", "ty", "you"));
            Assert.AreEqual("from me to you today", engine.ApplyReplacementsFirst("from me to you today", reps));
        }
 
        [Test]
        public void TestWithAnchorMismatch() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement("to", "Joe", "you"));
            Assert.AreEqual("from me to you", engine.ApplyReplacementsFirst("from me to you", reps));
        }

    }
}
