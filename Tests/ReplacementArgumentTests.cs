using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using kgrep;

namespace Tests {

    [TestFixture]
    public class ReplacementArgumentTests {

        [Test]
        public void TestSimpleArgument() {
            ReplacementFile rf = new ReplacementFile("a~bc");
            List<Replacement> reps = rf.GetReplacements();
            Assert.AreEqual(reps[0].frompattern.ToString(), (new Regex("a".Trim(), RegexOptions.Compiled)).ToString());
            Assert.AreEqual(reps[0].topattern, "bc");
        }

        [Test]
        public void TestTwoArguments() {
            ReplacementFile rf = new ReplacementFile("a~bc; g~jk");
            List<Replacement> reps = rf.GetReplacements();
            Assert.AreEqual(reps[0].topattern, "bc");
            Assert.AreEqual(reps[1].frompattern.ToString(), (new Regex("g".Trim(), RegexOptions.Compiled)).ToString());
            Assert.AreEqual(reps[1].topattern, "jk");
        }

        [Test]
        public void TestThreeArgument() {
            ReplacementFile rf = new ReplacementFile("a~bc;   hello~world  ;  third ~ fourth ");
            List<Replacement> reps = rf.GetReplacements();
            Assert.AreEqual(reps[2].frompattern.ToString(), (new Regex("third".Trim(), RegexOptions.Compiled)).ToString());
            Assert.AreEqual(reps[2].topattern, "fourth");
        }

        [Test]
        public void TestEmbeddedDelim() {
            ReplacementFile rf = new ReplacementFile("delim=,; a,bc;   hello,world  ;  third , fourth ");
            List<Replacement> reps = rf.GetReplacements();
            Assert.AreEqual(reps[2].frompattern.ToString(), (new Regex("third".Trim(), RegexOptions.Compiled)).ToString());
            Assert.AreEqual(reps[2].topattern, "fourth");
        }

        [Test]
        public void TestEmbeddedDelimAndScopeFirst() {
            ReplacementFile rf = new ReplacementFile("scope=first;delim=,; a,b; b,c");
            List<Replacement> reps = rf.GetReplacements();
            Assert.IsTrue(reps.Count == 2);
            Assert.AreEqual(reps[0].frompattern.ToString(), (new Regex("a".Trim(), RegexOptions.Compiled)).ToString());
            Assert.AreEqual(reps[0].topattern, "b");
        }

        [Test]
        public void TestEmbeddedDelimAndScopeAll() {
            ReplacementFile rf = new ReplacementFile("scope=all;delim=,; a,b; b,c");
            List<Replacement> reps = rf.GetReplacements();
            Assert.IsTrue(reps.Count == 2);
            Assert.AreEqual(reps[1].frompattern.ToString(), (new Regex("b".Trim(), RegexOptions.Compiled)).ToString());
            Assert.AreEqual(reps[1].topattern, "c");
        }

        [Test]
        public void TestEmbeddedComment() {
            ReplacementFile rf = new ReplacementFile("#comment; a~bc");
            List<Replacement> reps = rf.GetReplacements();
            Assert.AreEqual(reps[0].frompattern.ToString(), (new Regex("a".Trim(), RegexOptions.Compiled)).ToString());
            Assert.AreEqual(reps[0].topattern, "bc");
        }

        [Test]
        public void TestRemoveOneArgument() {
            ReplacementFile rf = new ReplacementFile("# remove a from arg; a~");
            List<Replacement> reps = rf.GetReplacements();

            KgrepEngine engine = new KgrepEngine();
            string result = engine.ApplyReplacementsAll("a b ca", reps);
            Assert.AreEqual(result, " b c");
        }

        [Test]
        [ExpectedException(typeof(System.Exception))]
        public void TestInvalidRegex() {
            ReplacementFile rf = new ReplacementFile("a[.~b");
            List<Replacement> reps = rf.GetReplacements();
        }

        [Test]
        [ExpectedException(typeof(System.Exception))]
        public void TestInvalidRegex() {
            ReplacementFile rf = new ReplacementFile("a~b[.");
            List<Replacement> reps = rf.GetReplacements();
        }
    }
}
