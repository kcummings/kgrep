
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using kgrep;

namespace Tests {

    [TestFixture]
    public class ReplacementFileTests {

        [Test]
        public void WhenOneArgument_ExpectOneReplacement() {
            ParseReplacementFile rf = new ParseReplacementFile("a~bc");
            List<Replacement> reps = rf.ReplacementList;
            Assert.AreEqual((new Regex("a".Trim(), RegexOptions.Compiled)).ToString(), reps[0].frompattern.ToString());
            Assert.AreEqual( "bc", reps[0].topattern);
        }

        [Test]
        public void WhenTwoArguments_ExpectTwoReplacements() {
            ParseReplacementFile rf = new ParseReplacementFile("a~bc; g~jk");
            List<Replacement> reps = rf.ReplacementList;
            Assert.AreEqual(2,reps.Count);
            Assert.AreEqual("bc",reps[0].topattern);
            Assert.AreEqual((new Regex("g".Trim(), RegexOptions.Compiled)).ToString(), reps[1].frompattern.ToString());
            Assert.AreEqual("jk",  reps[1].topattern);
        }

        [Test]
        public void WhenThreeArguments_ExpectThreeReplacements() {
            ParseReplacementFile rf = new ParseReplacementFile("a~bc;   hello~world  ;  third ~ fourth ");
            List<Replacement> reps = rf.ReplacementList;
            Assert.AreEqual(3, reps.Count);
            Assert.AreEqual((new Regex("third".Trim(), RegexOptions.Compiled)).ToString(), reps[2].frompattern.ToString());
            Assert.AreEqual( "fourth", reps[2].topattern);
        }

        [Test]
        public void WhenEmbeddedDelim_ExpectChangedDelim() {
            ParseReplacementFile rf = new ParseReplacementFile("delim=,; a,bc;   hello,world  ;  third , fourth ");
            List<Replacement> reps = rf.ReplacementList;
            Assert.AreEqual((new Regex("third".Trim(), RegexOptions.Compiled)).ToString(), reps[2].frompattern.ToString());
            Assert.AreEqual("fourth", reps[2].topattern);
        }

        [Test]
        public void WhenEmbeddedDelimAndScopeFirst_ExpectFirstOnlyReplaces() {
            ParseReplacementFile rf = new ParseReplacementFile("scope=first;delim=,; a,b; b,c");
            List<Replacement> reps = rf.ReplacementList;
            Assert.IsTrue(reps.Count == 2);
            Assert.AreEqual((new Regex("a".Trim(), RegexOptions.Compiled)).ToString(), reps[0].frompattern.ToString());
            Assert.AreEqual("b", reps[0].topattern);
        }

        [Test]
        public void WhenEmbeddedDelimAndScopeAll_ExpectAllReplaces() {
            ParseReplacementFile rf = new ParseReplacementFile("scope=all;delim=,; a,b; b,c");
            List<Replacement> reps = rf.ReplacementList;
            Assert.IsTrue(reps.Count == 2);
            Assert.AreEqual((new Regex("b".Trim(), RegexOptions.Compiled)).ToString(), reps[1].frompattern.ToString());
            Assert.AreEqual( "c", reps[1].topattern);
        }

        [Test]
        public void WhenEmbeddedComment_ExpectCommentIgnored() {
            ParseReplacementFile rf = new ParseReplacementFile("#comment; a~bc");
            List<Replacement> reps = rf.ReplacementList;
            Assert.AreEqual((new Regex("a".Trim(), RegexOptions.Compiled)).ToString(), reps[0].frompattern.ToString());
            Assert.AreEqual( "bc", reps[0].topattern);
        }

        [Test]
        public void WhenEmbeddedComment_ExpectNoChange() {
            ParseReplacementFile rf = new ParseReplacementFile("comment=:; :ignored;");
            List<Replacement> reps = rf.ReplacementList;

            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            string result = engine.ApplyReplacementsAll("a b ca", reps);
            Assert.AreEqual("a b ca", result);
            Assert.IsTrue(reps.Count == 0);
        }

        [Test]
        public void WhenNoTopattern_ExpectFrompatternRemoved() {
            ParseReplacementFile rf = new ParseReplacementFile("a~");
            List<Replacement> reps = rf.ReplacementList;

            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            string result = engine.ApplyReplacementsAll("a b ca", reps);
            Assert.AreEqual(" b c", result);
        }

        [Test]
        public void WhenEnclosedQuotes_ExpectTrailingSpacesRetained() {
            ParseReplacementFile rf = new ParseReplacementFile(" \"a \" ~ b ");
            List<Replacement> reps = rf.ReplacementList;

            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            string result = engine.ApplyReplacementsAll("a b ca", reps);
            Assert.AreEqual("bb ca", result);
        }

        [TestCase("d a b ca", @" \sa ~ b ", "db b ca")]  // single leading
        [TestCase(" ab  c", @" \s\sc~", " ab")]          // two leading spaces and remove it
        [TestCase("abc d", @" bc\s~bc", "abcd")]         // single trailing
        [TestCase("abc  d", @" bc\s\s~bc", "abcd")]      // two trailng spaces and remove it
        public void WhenRegexSpaceInFrompattern_ExpectSpaces(string input, string repstring, string expect) {
            ParseReplacementFile rf = new ParseReplacementFile(repstring);
            List<Replacement> reps = rf.ReplacementList;

            ReplaceTokensInSourceFiles engine = new ReplaceTokensInSourceFiles();
            string result = engine.ApplyReplacementsAll(input, reps);
            Assert.AreEqual(expect, result);
        }

        [TestCase("[.a~c~b")] // invalid anchor
        [TestCase("a[.~b")]   // invalid from pattern
        [TestCase("a~b[.")]   // invalid to pattern
        [ExpectedException(typeof(System.Exception))]
        public void WhenInvalidRegexPattern_ExpectException(string pattern) {
            ParseReplacementFile rf = new ParseReplacementFile(pattern);
            List<Replacement> reps = rf.ReplacementList;
        }

    }
}
