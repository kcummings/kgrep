using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using kgrep;
using NUnit.Framework;

namespace kgrep.Tests {
    [TestFixture]
    public class EngineTests {
        [Test]
        public void TestEmptyReplacementSet() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            Assert.AreEqual(engine.ApplyReplacements("abc", reps), "abc");
        }

        [Test]
        public void TestSimpleReplacement() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            Replacement rep = new Replacement(false, "", "abc", "def");
            reps.Add(rep);
            Assert.AreEqual(engine.ApplyReplacements("abc", reps), "def");
        }

        [Test]
        public void TestMultipleOccurances() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            Replacement rep = new Replacement(false, "", "ab", "de");
            reps.Add(rep);
            Assert.AreEqual(engine.ApplyReplacements("able abe lincoln", reps), "dele dee lincoln");
        }

        [Test]
        public void TestEndPointReplacements() {
            KgrepEngine engine = new KgrepEngine();
            List<Replacement> reps = new List<Replacement>();
            Replacement rep = new Replacement(false, "", "ab", "de");
            reps.Add(rep);
            Assert.AreEqual(engine.ApplyReplacements("able abe lincoab", reps), "dele dee lincode");
        }
    }
}
