using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using kgrep;
using NUnit.Framework;

namespace kgrep.Tests {
    [TestFixture]
    public class ReplacementTests {
        [Test]
        public void TestEmptyReplacementSet() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            Assert.AreEqual( "abc",engine.ApplyReplacements("abc", reps));
        }

        [Test]
        public void TestSimpleReplacementToRemoveToken() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            Replacement rep = new Replacement(false, "", "token", "");
            reps.Add(rep);
            Assert.AreEqual("abc", engine.ApplyReplacements("abctoken", reps));
        }

        [Test]
        public void TestSimpleReplacementNoMatch() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            Replacement rep = new Replacement(false, "", "k", "def");
            reps.Add(rep);
            Assert.AreNotEqual("", engine.ApplyReplacements("abc", reps));
        }

        [Test]
        public void TestSimpleReplacement() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            Replacement rep = new Replacement(false, "", "abc", "def");
            reps.Add(rep);
            Assert.AreEqual("def",engine.ApplyReplacements("abc", reps));
        }

        [Test]
        public void TestEndPointReplacements() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            Replacement rep = new Replacement(false, "", "ab", "de");
            reps.Add(rep);
            Assert.AreEqual("dele dee lincode",engine.ApplyReplacements("able abe lincoab", reps));
        }

        [Test]
        public void TestFirstReplacmentWithMultipleLines() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement(false, "", "from", "to"));
            reps.Add(new Replacement(false, "", "to", "this"));
            Assert.AreEqual("to me to you", engine.ApplyReplacements("from me to you", reps));
        }

        [Test]
        public void TestAllReplacmentWithMultipleLines() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement(true, "", "from", "to"));
            reps.Add(new Replacement(true, "", "to", "this"));
            Assert.AreEqual("this me this you", engine.ApplyReplacements("from me to you", reps));
        }

        [Test]
        public void TestFirstReplacmentWithMultipleLinesWithGroups() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement(false, "", "f(..)m", "$1"));
            reps.Add(new Replacement(false, "", "to", "this"));
            Assert.AreEqual("ro me to you", engine.ApplyReplacements("from me to you", reps));
        }

        [Test]
        public void TestFirstReplacmentWithMultipleLinesSwappingGroups() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement(true, "", @"(\w+)\s(\w+)", "$2 $1"));
            reps.Add(new Replacement(true, "", "to", "this"));
            Assert.AreEqual("me from you this", engine.ApplyReplacements("from me to you", reps));
        }

        [Test]
        public void TestAllReplacmentWithMultipleLinesSwappingGroups() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement(false, "", @"(\w+)\s(\w+)", "$2 $1"));
            reps.Add(new Replacement(false, "", "to", "this"));
            Assert.AreEqual("me from you to", engine.ApplyReplacements("from me to you", reps));
        }

        [Test]
        public void TestWithAnchor() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement(false, "today", "you", "to"));
            Assert.AreEqual("from me to to today", engine.ApplyReplacements("from me to you today", reps));
        }

        [Test]
        public void TestWithAnchorMismatch() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement(false, "Joe", "you", "to"));
            Assert.AreEqual("from me to you", engine.ApplyReplacements("from me to you", reps));
        }

        [Test]
        public void TestReplacmentWithFirstAndAll() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            reps.Add(new Replacement(false, "", "test", "hello"));
            reps.Add(new Replacement(false, "", "he", "this"));
            reps.Add(new Replacement(true, "", "by", "bye"));
            reps.Add(new Replacement(true, "", "done", "go"));
            Assert.AreEqual("hello he bye the go", engine.ApplyReplacements("test he by the done", reps));
        }
    }
}
