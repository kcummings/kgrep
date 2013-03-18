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
    }
}
