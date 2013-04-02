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
            Assert.AreEqual((new Regex("a".Trim(), RegexOptions.Compiled)).ToString(), reps[0].frompattern.ToString());
            Assert.AreEqual( "bc", reps[0].topattern);
        }

        [Test]
        public void TestTwoArguments() {
            ReplacementFile rf = new ReplacementFile("a~bc; g~jk");
            List<Replacement> reps = rf.GetReplacements();
            Assert.AreEqual("bc",reps[0].topattern);
            Assert.AreEqual((new Regex("g".Trim(), RegexOptions.Compiled)).ToString(), reps[1].frompattern.ToString());
            Assert.AreEqual("jk",  reps[1].topattern);
        }

        [Test]
        public void TestThreeArgument() {
            ReplacementFile rf = new ReplacementFile("a~bc;   hello~world  ;  third ~ fourth ");
            List<Replacement> reps = rf.GetReplacements();
            Assert.AreEqual((new Regex("third".Trim(), RegexOptions.Compiled)).ToString(), reps[2].frompattern.ToString());
            Assert.AreEqual( "fourth", reps[2].topattern);
        }

        [Test]
        public void TestEmbeddedDelim() {
            ReplacementFile rf = new ReplacementFile("delim=,; a,bc;   hello,world  ;  third , fourth ");
            List<Replacement> reps = rf.GetReplacements();
            Assert.AreEqual((new Regex("third".Trim(), RegexOptions.Compiled)).ToString(), reps[2].frompattern.ToString());
            Assert.AreEqual("fourth", reps[2].topattern);
        }

        [Test]
        public void TestEmbeddedDelimAndScopeFirst() {
            ReplacementFile rf = new ReplacementFile("scope=first;delim=,; a,b; b,c");
            List<Replacement> reps = rf.GetReplacements();
            Assert.IsTrue(reps.Count == 2);
            Assert.AreEqual((new Regex("a".Trim(), RegexOptions.Compiled)).ToString(), reps[0].frompattern.ToString());
            Assert.AreEqual("b", reps[0].topattern);
        }

        [Test]
        public void TestEmbeddedDelimAndScopeAll() {
            ReplacementFile rf = new ReplacementFile("scope=all;delim=,; a,b; b,c");
            List<Replacement> reps = rf.GetReplacements();
            Assert.IsTrue(reps.Count == 2);
            Assert.AreEqual((new Regex("b".Trim(), RegexOptions.Compiled)).ToString(), reps[1].frompattern.ToString());
            Assert.AreEqual( "c", reps[1].topattern);
        }

        [Test]
        public void TestEmbeddedComment() {
            ReplacementFile rf = new ReplacementFile("#comment; a~bc");
            List<Replacement> reps = rf.GetReplacements();
            Assert.AreEqual((new Regex("a".Trim(), RegexOptions.Compiled)).ToString(), reps[0].frompattern.ToString());
            Assert.AreEqual( "bc", reps[0].topattern);
        }

        [Test]
        public void TestEmbeddedControlsNoAction() {
            ReplacementFile rf = new ReplacementFile("comment=:; :ignored;");
            List<Replacement> reps = rf.GetReplacements();

            KgrepEngine engine = new KgrepEngine();
            string result = engine.ApplyReplacementsAll("a b ca", reps);
            Assert.AreEqual("a b ca", result);
            Assert.IsTrue(reps.Count == 0);
        }

        [Test]
        public void TestRemoveOneArgument() {
            ReplacementFile rf = new ReplacementFile("# remove a from arg; a~");
            List<Replacement> reps = rf.GetReplacements();

            KgrepEngine engine = new KgrepEngine();
            string result = engine.ApplyReplacementsAll("a b ca", reps);
            Assert.AreEqual(" b c", result);
        }

        [Test]
        public void TestTrailingSpace() {
            ReplacementFile rf = new ReplacementFile(@" a\s ~ b ");
            List<Replacement> reps = rf.GetReplacements();

            KgrepEngine engine = new KgrepEngine();
            string result = engine.ApplyReplacementsAll("a b ca", reps);
            Assert.AreEqual("bb ca", result);
        }

        [Test]
        public void TestTrailingSpaces() {
            ReplacementFile rf = new ReplacementFile(@" a\s\s ~ b ");
            List<Replacement> reps = rf.GetReplacements();

            KgrepEngine engine = new KgrepEngine();
            string result = engine.ApplyReplacementsAll("a   b ca", reps);
            Assert.AreEqual("b b ca", result);
        }

        [Test]
        public void TestLeadingSpaceWillFail() {
            ReplacementFile rf = new ReplacementFile(@" \sa ~ b ");
            List<Replacement> reps = rf.GetReplacements();

            KgrepEngine engine = new KgrepEngine();
            string result = engine.ApplyReplacementsAll(" da b ca", reps);
            Assert.AreEqual(" da b ca", result);
        }

        [Test]
        public void TestLeadingSpace() {
            ReplacementFile rf = new ReplacementFile(@" \sa ~ b ");
            List<Replacement> reps = rf.GetReplacements();

            KgrepEngine engine = new KgrepEngine();
            string result = engine.ApplyReplacementsAll(" a b ca", reps);
            Assert.AreEqual("b b ca", result);
        }

        [Test]
        public void TestLeadingSpaces() {
            ReplacementFile rf = new ReplacementFile(@" \s\sa  ~ b ");
            List<Replacement> reps = rf.GetReplacements();

            KgrepEngine engine = new KgrepEngine();
            string result = engine.ApplyReplacementsAll("  a  b ca", reps);
            Assert.AreEqual("b  b ca", result);
        }

        [Test]
        public void TestTrailingSpaceAfterField() {
            ReplacementFile rf = new ReplacementFile(@" \sa  ~ b\s ");
            List<Replacement> reps = rf.GetReplacements();

            KgrepEngine engine = new KgrepEngine();
            string result = engine.ApplyReplacementsAll("3 ab ca", reps);
            Assert.AreEqual("3b b ca", result);
        }

        [Test]
        public void TestTrailingSpacesAfterField() {
            ReplacementFile rf = new ReplacementFile(@" a  ~ b\s\s ");
            List<Replacement> reps = rf.GetReplacements();

            KgrepEngine engine = new KgrepEngine();
            string result = engine.ApplyReplacementsAll("3 ab ca", reps);
            Assert.AreEqual("3 b  b cb  ", result);
        }

        [Test]
        public void TestLeadingSpacesAfterField() {
            ReplacementFile rf = new ReplacementFile(@" a  ~ \sb ");
            List<Replacement> reps = rf.GetReplacements();

            KgrepEngine engine = new KgrepEngine();
            string result = engine.ApplyReplacementsAll("3ab ca", reps);
            Assert.AreEqual("3 bb c b", result);
        }

        [Test]
        public void TestLeadingAndTrailingSpaces() {
            ReplacementFile rf = new ReplacementFile(@" \s\sa\s  ~ b ");
            List<Replacement> reps = rf.GetReplacements();

            KgrepEngine engine = new KgrepEngine();
            string result = engine.ApplyReplacementsAll("  a b ca", reps);
            Assert.AreEqual("bb ca", result);
        }

        [Test]
        [ExpectedException(typeof(System.Exception))]
        public void TestInvalidRegexFromPattern() {
            ReplacementFile rf = new ReplacementFile("a[.~b");
            List<Replacement> reps = rf.GetReplacements();
        }

        [Test]
        [ExpectedException(typeof(System.Exception))]
        public void TestInvalidRegexToPattern() {
            ReplacementFile rf = new ReplacementFile("a~b[.");
            List<Replacement> reps = rf.GetReplacements();
        }

        [Test]
        [ExpectedException(typeof(System.Exception))]
        public void TestInvalidRegexWithAnchor() {
            ReplacementFile rf = new ReplacementFile("[.a~c~b");
            List<Replacement> reps = rf.GetReplacements();
        }
    }
}
