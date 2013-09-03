﻿using System;
using NUnit.Framework;
using kgrep;

namespace Tests {

    [TestFixture]
    public class ParseCommandLineTests {

        [TestCase("a~c", "abc", "a~c")]
        [TestCase("a~c", "a~bc", "a~c")]   
        public void WhenThreeArguments_ExpectReplacementFileAndSourceFiles(string token, string repfile, string expected) {
            string[] args = new String[] { token, repfile };
            ParseCommandLine cmd = new ParseCommandLine(args);
            Assert.AreEqual(expected, cmd.ReplacementFileName);
            Assert.AreEqual(1, cmd.InputSourceNames.Count);
        }

        [Test]
        // kgrep FromPattern filename1
        public void WhenTwoArguments_ExpectReplacementFileAndSourceFile() {
            string[] args = new String[] { "abc", "file1.txt" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            Assert.AreEqual(1, cmd.InputSourceNames.Count);
            Assert.AreEqual("file1.txt", cmd.InputSourceNames[0]);
        }

        [TestCase("abc","file1.txt","file2.txt", "file1.txt", "file2.txt")]
        [TestCase("a~c; b~d", "file1.txt", "file2.txt", "file1.txt","file2.txt")]
        [TestCase("#comment; a~c; b~d", "file1.txt", "file2.txt", "file1.txt", "file2.txt")]
        // kgrep FromPattern filename1 filename2
        public void WhenManyArguments_ExpectReplacementFileAndManySourceFiles(string token, string file1, string file2, string expected1, string expected2) {
            string[] args = new String[] { token, file1, file2 };
            ParseCommandLine cmd = new ParseCommandLine(args);
            Assert.AreEqual(2, cmd.InputSourceNames.Count);
            Assert.AreEqual(expected1, cmd.InputSourceNames[0]);
            Assert.AreEqual(expected2, cmd.InputSourceNames[1]);
        }

        [Test]
        // cat filename|kgrep replacementFilename
        public void WhenOnlyReplacementFileArgument_ExpectStdinAsInputSource() {
            string[] args = new String[] { "hi~bye" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            Assert.AreEqual("hi~bye", cmd.ReplacementFileName);
            Assert.AreEqual(1, cmd.InputSourceNames.Count);
            Assert.AreEqual(cmd.STDIN, cmd.InputSourceNames[0]);
        }

        [Test]
        public void WhenNoArguments_ExpectNoArguments() {
            string[] args = new String[] { "" };
            ParseCommandLine cmd = new ParseCommandLine(args);
            Assert.AreEqual("", cmd.ReplacementFileName);
            Assert.AreEqual(1, cmd.InputSourceNames.Count);  // empty string
            Assert.AreEqual(cmd.STDIN, cmd.InputSourceNames[0]);
        }
    }
}
