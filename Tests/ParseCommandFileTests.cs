
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using kgrep;

namespace Tests {

    [TestFixture]
    public class ParseCommandFileTests {

        [Test]
        public void WhenOneArgument_ExpectOneReplacement() {
            ParseCommandFile rf = new ParseCommandFile("a~bc");
            List<Command> reps = rf.CommandList;
            Assert.AreEqual((new Regex("a".Trim(), RegexOptions.Compiled)).ToString(), reps[0].SubjectRegex.ToString());
            Assert.AreEqual("bc", reps[0].ReplacementString);
        }

        [Test]
        public void WhenTwoArguments_ExpectTwoReplacements() {
            ParseCommandFile rf = new ParseCommandFile("a~bc; g~jk");
            List<Command> reps = rf.CommandList;
            Assert.AreEqual(2, reps.Count);
            Assert.AreEqual("bc", reps[0].ReplacementString);
            Assert.AreEqual((new Regex("g".Trim(), RegexOptions.Compiled)).ToString(), reps[1].SubjectRegex.ToString());
            Assert.AreEqual("jk", reps[1].ReplacementString);
        }

        [Test]
        public void WhenThreeArguments_ExpectThreeReplacements() {
            ParseCommandFile rf = new ParseCommandFile("a~bc;   hello~world  ;  third ~ fourth ");
            List<Command> reps = rf.CommandList;
            Assert.AreEqual(3, reps.Count);
            Assert.AreEqual((new Regex("third".Trim(), RegexOptions.Compiled)).ToString(), reps[2].SubjectRegex.ToString());
            Assert.AreEqual("fourth", reps[2].ReplacementString);
        }

        [Test]
        public void WhenEmbeddedDelim_ExpectChangedDelim() {
            ParseCommandFile rf = new ParseCommandFile("delim=,; a,bc;   hello,world  ;  third , fourth ");
            List<Command> reps = rf.CommandList;
            Assert.AreEqual((new Regex("third".Trim(), RegexOptions.Compiled)).ToString(), reps[2].SubjectRegex.ToString());
            Assert.AreEqual("fourth", reps[2].ReplacementString);
        }

        [Test]
        public void WhenEmbeddedDelim_ExpectAllReplaces() {
            ParseCommandFile rf = new ParseCommandFile("delim=,; a,b; b,c");
            List<Command> reps = rf.CommandList;
            Assert.IsTrue(reps.Count == 2);
            Assert.AreEqual((new Regex("b".Trim(), RegexOptions.Compiled)).ToString(), reps[1].SubjectRegex.ToString());
            Assert.AreEqual("c", reps[1].ReplacementString);
        }

        [Test]
        public void WhenEndingScannerToken_ExpectAllReplacesMode() {
            ParseCommandFile rf = new ParseCommandFile("a~b; b");
            List<Command> reps = rf.CommandList;
            Assert.AreEqual(ParseCommandFile.RunningAs.ReplaceAll, rf.kgrepMode);
        }

        [Test]
        public void WhenAllDirectiveGiven_ExpectAllReplaceMode() {
            ParseCommandFile rf = new ParseCommandFile("a~b");
            List<Command> reps = rf.CommandList;
            Assert.AreEqual(ParseCommandFile.RunningAs.ReplaceAll, rf.kgrepMode);
        }

        [Test]
        public void WhenOnlyScansGiven_ExpectScannerToOverrideAllMode() {
            ParseCommandFile rf = new ParseCommandFile("a; [0-9]+");
            List<Command> reps = rf.CommandList;
            Assert.AreEqual(ParseCommandFile.RunningAs.Scanner, rf.kgrepMode);
        }

        [Test]
        public void WhenEndsWithAll_ExpectAllReplaceMode() {
            ParseCommandFile rf = new ParseCommandFile("a; [0-9]+; a~b");
            List<Command> reps = rf.CommandList;
            Assert.AreEqual(ParseCommandFile.RunningAs.ReplaceAll, rf.kgrepMode);
        }

        [Test]
        public void WhenOnlyScanCommands_ExpectScannerMode() {
            ParseCommandFile rf = new ParseCommandFile("a;b;c");
            List<Command> reps = rf.CommandList;
            Assert.AreEqual(ParseCommandFile.RunningAs.Scanner, rf.kgrepMode);
        }
         [Test]
        public void WhenNoCommandsGiven_ExpectScannerMode() {
            ParseCommandFile rf = new ParseCommandFile(";;;");
            List<Command> reps = rf.CommandList;
            Assert.AreEqual(ParseCommandFile.RunningAs.Scanner, rf.kgrepMode);
        }

        [Test]
        public void WhenEmbeddedComment_ExpectCommentIgnored() {
            ParseCommandFile rf = new ParseCommandFile("#comment; a~bc");
            List<Command> reps = rf.CommandList;
            Assert.AreEqual((new Regex("a".Trim(), RegexOptions.Compiled)).ToString(), reps[0].SubjectRegex.ToString());
            Assert.AreEqual("bc", reps[0].ReplacementString);
        }

        [Test]
        public void WhenEmbeddedComment_ExpectNoChange() {
            ParseCommandFile rf = new ParseCommandFile("comment=:; :ignored;");
            List<Command> reps = rf.CommandList;
            Assert.IsTrue(reps.Count == 0);
        }

        [Test]
        public void WhenNoTopattern_ExpectFrompatternRemoved() {
            ParseCommandFile rf = new ParseCommandFile("a~");
            List<Command> reps = rf.CommandList;

            Assert.AreEqual("a", rf.CommandList[0].SubjectRegex.ToString());
            Assert.AreEqual("", rf.CommandList[0].ReplacementString);
        }

        [Test]
        public void WhenEnclosedQuotes_ExpectTrailingSpacesRetained() {
            ParseCommandFile rf = new ParseCommandFile(" \"a \" ~ b ");
            List<Command> reps = rf.CommandList;

            string result = reps[0].SubjectRegex.ToString();
            Assert.AreEqual("a ", result);
        }

        //[TestCase("d a b ca", @" \sa ~ b ", "db b ca")]  // single leading
        //[TestCase(" ab  c", @" \s\sc~", " ab")]          // two leading spaces and remove it
        //[TestCase("abc d", @" bc\s~bc", "abcd")]         // single trailing
        //[TestCase("abc  d", @" bc\s\s~bc", "abcd")]      // two trailng spaces and remove it
        //public void WhenRegexSpaceInFrompattern_ExpectSpaces(string input, string repstring, string expect) {
        //    ParseCommandFile rf = new ParseCommandFile(repstring);
        //    List<Command> reps = rf.CommandList;

        //    string result = reps[0].SubjectRegex.ToString();
        //    Assert.AreEqual(expect, result);
        //}

        [TestCase("[./a/c~b")] // invalid AnchorString
        [TestCase("a[.~b")]   // invalid from pattern
        [ExpectedException(typeof(System.Exception))]
        public void WhenInvalidRegexPattern_ExpectException(string pattern) {
            ParseCommandFile rf = new ParseCommandFile(pattern);
            List<Command> reps = rf.CommandList;
        }

    }
}
