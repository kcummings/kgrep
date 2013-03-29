using System;
using NUnit.Framework;
using kgrep;

namespace Tests {

    // To successfully run these tests, the files "file1.txt" and "file2.txt" must
    // exist in the src/bin/Debug folder. They are part of the repository.

    [TestFixture]
    public class ParseCommandLineTests {

        [Test]
        // kgrep frompattern frompattern2
        public void TestScanAndReplacement() {
            string[] args = new String[] { "a~c", "abc" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            Assert.IsNull(cmd.SearchPattern);
            Assert.AreEqual("a~c", cmd.ReplacementFileName);
            Assert.AreEqual(0, cmd.InputSourceNames.Count);
        }

        [Test]
        // kgrep frompattern frompattern2
        public void TestTwoScans() {
            string[] args = new String[] { "a~c", "a~bc" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            Assert.IsNull(cmd.SearchPattern);
            Assert.AreEqual("a~c", cmd.ReplacementFileName);
            Assert.AreEqual(0, cmd.InputSourceNames.Count);
        }

        [Test]
        // kgrep frompattern filename1
        public void TestSimpleScanSingleFile() {
            string[] args = new String[] { "abc", "file1.txt" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            Assert.AreEqual("abc", cmd.SearchPattern);
            Assert.IsNull(cmd.ReplacementFileName);
            Assert.AreEqual(1, cmd.InputSourceNames.Count);
            Assert.AreEqual("file1.txt", cmd.InputSourceNames[0]);
        }

        [Test]
        // kgrep frompattern filename1 filename2
        public void TestSimpleScanTwoFiles() {
            string[] args = new String[] { "abc", "file1.txt", "file2.txt" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            Assert.AreEqual("abc", cmd.SearchPattern);
            Assert.IsNull(cmd.ReplacementFileName);
            Assert.AreEqual(2, cmd.InputSourceNames.Count);
            Assert.AreEqual("file1.txt", cmd.InputSourceNames[0]);
            Assert.AreEqual("file2.txt", cmd.InputSourceNames[1]);
        }

        [Test]
        // kgrep frompattern filename1 filename2
        public void TestSimpleReplacementTwoFiles() {
            string[] args = new String[] { "a~c; b~d", "file1.txt", "file2.txt" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            Assert.AreEqual("a~c; b~d", cmd.ReplacementFileName);
            Assert.IsNull(cmd.SearchPattern);
            Assert.AreEqual(2, cmd.InputSourceNames.Count);
            Assert.AreEqual("file1.txt", cmd.InputSourceNames[0]);
            Assert.AreEqual("file2.txt", cmd.InputSourceNames[1]);
        }
        [Test]
        // cat filename|kgrep replacementFilename
        public void TestReplacementFileScan() {
            string[] args = new String[] { "hi~bye" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            Assert.IsNull(cmd.SearchPattern);
            Assert.AreEqual("hi~bye", cmd.ReplacementFileName);
            Assert.AreEqual(1, cmd.InputSourceNames.Count);
            Assert.AreEqual(cmd.STDIN, cmd.InputSourceNames[0]);
        }
    }
}
