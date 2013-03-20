using System;
using NUnit.Framework;
using kgrep;

namespace Tests {

    // To successfully run these tests, the files "file1.txt" and "file2.txt" must
    // exist in the src/bin/Debug folder. They are part of the repository.

    // kgrep matchpattern topattern filename1 ... filenameN
    // kgrep matchpattern filename1 ... filenameN
    // kgrep -f patternfile filename1 ... filenameN
    // kgrep -f patternfile filename1
    // cat filename|kgrep matchpattern topattern
    // cat filename|kgrep matchpattern
    // cat filename|kgrep -f patternfile
    // cat filename|kgrep matchpattern topattern

    [TestFixture]
    public class ParseCommandLineTests {

        [Test]
        // cat filename|kgrep frompattern topattern
        public void TestSimpleSearchAndReplace() {
            string[] args = new String[] { "abc", "def" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            Assert.AreEqual("abc", cmd.SearchPattern);
            Assert.AreEqual("def", cmd.ReplacementPattern);
            Assert.AreEqual(1, cmd.InputSourceNames.Count);
            Assert.AreEqual(cmd.STDIN, cmd.InputSourceNames[0]);
        }

        [Test]
        // kgrep frompattern filename1
        public void TestSimpleScanSingleFile() {
            string[] args = new String[] { "abc", "file1.txt" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            Assert.AreEqual("abc", cmd.SearchPattern);
            Assert.IsNull(cmd.ReplacementPattern);
            Assert.AreEqual(1, cmd.InputSourceNames.Count);
            Assert.AreEqual("file1.txt", cmd.InputSourceNames[0]);
        }

        [Test]
        // kgrep frompattern filename1 filename2
        public void TestSimpleScanTwoFiles() {
            string[] args = new String[] { "abc", "file1.txt", "file2.txt" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            Assert.AreEqual("abc", cmd.SearchPattern);
            Assert.IsNull(cmd.ReplacementPattern);
            Assert.AreEqual(2, cmd.InputSourceNames.Count);
            Assert.AreEqual("file1.txt", cmd.InputSourceNames[0]);
            Assert.AreEqual("file2.txt", cmd.InputSourceNames[1]);
        }

        [Test]
        // kgrep frompattern topattern filename1 filename2
        public void TestSimpleReplacementsTwoFiles() {
            string[] args = new String[] { "abc", "new", "file1.txt", "file2.txt" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            Assert.AreEqual("abc", cmd.SearchPattern);
            Assert.AreEqual("new",cmd.ReplacementPattern);
            Assert.AreEqual(2, cmd.InputSourceNames.Count);
            Assert.AreEqual("file1.txt", cmd.InputSourceNames[0]);
            Assert.AreEqual("file2.txt", cmd.InputSourceNames[1]);
        }

        [Test]
        // cat filename|kgrep -f replacementFilename
        public void TestReplacementFileScan() {
            string[] args = new String[] { "-f", "replacement.txt" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            Assert.IsNull(cmd.SearchPattern);
            Assert.IsNull(cmd.ReplacementPattern);
            Assert.AreEqual("replacement.txt", cmd.ReplacementFileName);
            Assert.AreEqual(1, cmd.InputSourceNames.Count);
            Assert.AreEqual(cmd.STDIN, cmd.InputSourceNames[0]);
        }
    }
}
