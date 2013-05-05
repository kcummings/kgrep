
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using kgrep;

namespace Tests {

    [TestFixture]
    public class ReplacementFileTests {

        [Test]
        public void WhenOneArgument_ExpectOneReplacement() {
            ReplacementFile rf = new ReplacementFile("a~bc");
            List<Replacement> reps = rf.ReplacementList;
            Assert.AreEqual((new Regex("a".Trim(), RegexOptions.Compiled)).ToString(), reps[0].frompattern.ToString());
            Assert.AreEqual( "bc", reps[0].topattern);
        }

        [Test]
        public void WhenTwoArguments_ExpectTwoReplacements() {
            ReplacementFile rf = new ReplacementFile("a~bc; g~jk");
            List<Replacement> reps = rf.ReplacementList;
            Assert.AreEqual(2,reps.Count);
            Assert.AreEqual("bc",reps[0].topattern);
            Assert.AreEqual((new Regex("g".Trim(), RegexOptions.Compiled)).ToString(), reps[1].frompattern.ToString());
            Assert.AreEqual("jk",  reps[1].topattern);
        }

        [Test]
        public void WhenThreeArguments_ExpectThreeReplacements() {
            ReplacementFile rf = new ReplacementFile("a~bc;   hello~world  ;  third ~ fourth ");
            List<Replacement> reps = rf.ReplacementList;
            Assert.AreEqual(3, reps.Count);
            Assert.AreEqual((new Regex("third".Trim(), RegexOptions.Compiled)).ToString(), reps[2].frompattern.ToString());
            Assert.AreEqual( "fourth", reps[2].topattern);
        }

        [Test]
        public void WhenEmbeddedDelim_ExpectChangedDelim() {
            ReplacementFile rf = new ReplacementFile("delim=,; a,bc;   hello,world  ;  third , fourth ");
            List<Replacement> reps = rf.ReplacementList;
            Assert.AreEqual((new Regex("third".Trim(), RegexOptions.Compiled)).ToString(), reps[2].frompattern.ToString());
            Assert.AreEqual("fourth", reps[2].topattern);
        }

        [Test]
        public void WhenEmbeddedDelimAndScopeFirst_ExpectFirstOnlyReplaces() {
            ReplacementFile rf = new ReplacementFile("scope=first;delim=,; a,b; b,c");
            List<Replacement> reps = rf.ReplacementList;
            Assert.IsTrue(reps.Count == 2);
            Assert.AreEqual((new Regex("a".Trim(), RegexOptions.Compiled)).ToString(), reps[0].frompattern.ToString());
            Assert.AreEqual("b", reps[0].topattern);
        }

        [Test]
        public void WhenEmbeddedDelimAndScopeAll_ExpectAllReplaces() {
            ReplacementFile rf = new ReplacementFile("scope=all;delim=,; a,b; b,c");
            List<Replacement> reps = rf.ReplacementList;
            Assert.IsTrue(reps.Count == 2);
            Assert.AreEqual((new Regex("b".Trim(), RegexOptions.Compiled)).ToString(), reps[1].frompattern.ToString());
            Assert.AreEqual( "c", reps[1].topattern);
        }

        [Test]
        public void WhenEmbeddedComment_ExpectCommentIgnored() {
            ReplacementFile rf = new ReplacementFile("#comment; a~bc");
            List<Replacement> reps = rf.ReplacementList;
            Assert.AreEqual((new Regex("a".Trim(), RegexOptions.Compiled)).ToString(), reps[0].frompattern.ToString());
            Assert.AreEqual( "bc", reps[0].topattern);
        }

        [Test]
        public void WhenEmbeddedComment_ExpectNoChange() {
            ReplacementFile rf = new ReplacementFile("comment=:; :ignored;");
            List<Replacement> reps = rf.ReplacementList;

            ReplacerEngine engine = new ReplacerEngine();
            string result = engine.ApplyReplacementsAll("a b ca", reps);
            Assert.AreEqual("a b ca", result);
            Assert.IsTrue(reps.Count == 0);
        }

        [Test]
        public void WhenNoTopattern_ExpectFrompatternRemoved() {
            ReplacementFile rf = new ReplacementFile("a~");
            List<Replacement> reps = rf.ReplacementList;

            ReplacerEngine engine = new ReplacerEngine();
            string result = engine.ApplyReplacementsAll("a b ca", reps);
            Assert.AreEqual(" b c", result);
        }

        [Test]
        public void WhenEnclosedQuotes_ExpectTrailingSpacesRetained() {
            ReplacementFile rf = new ReplacementFile(" \"a \" ~ b ");
            List<Replacement> reps = rf.ReplacementList;

            ReplacerEngine engine = new ReplacerEngine();
            string result = engine.ApplyReplacementsAll("a b ca", reps);
            Assert.AreEqual("bb ca", result);
        }

        [TestCase("d a b ca", @" \sa ~ b ", "db b ca")]  // single leading
        [TestCase(" ab  c", @" \s\sc~", " ab")]          // two leading spaces and remove it
        [TestCase("abc d", @" bc\s~bc", "abcd")]         // single trailing
        [TestCase("abc  d", @" bc\s\s~bc", "abcd")]      // two trailng spaces and remove it
        public void WhenRegexSpaceInFrompattern_ExpectSpaces(string input, string repstring, string expect) {
            ReplacementFile rf = new ReplacementFile(repstring);
            List<Replacement> reps = rf.ReplacementList;

            ReplacerEngine engine = new ReplacerEngine();
            string result = engine.ApplyReplacementsAll(input, reps);
            Assert.AreEqual(expect, result);
        }

        [TestCase("[.a~c~b")] // invalid anchor
        [TestCase("a[.~b")]   // invalid from pattern
        [TestCase("a~b[.")]   // invalid to pattern
        [ExpectedException(typeof(System.Exception))]
        public void WhenInvalidRegexPattern_ExpectException(string pattern) {
            ReplacementFile rf = new ReplacementFile(pattern);
            List<Replacement> reps = rf.ReplacementList;
        }

    }
}
