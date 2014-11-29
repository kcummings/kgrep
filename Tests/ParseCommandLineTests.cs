using System;
using System.Collections.Generic;
using NUnit.Framework;
using kgrep;
using NSubstitute;

namespace Tests {

    [TestFixture]
    public class ParseCommandLineTests {

        [TestCase("a~c", "file.txt", "a~c")]
        public void WhenThreeArguments_ExpectReplacementFileAndSourceFiles(string token, string repfile, string expected) {
            IUtilities util = Substitute.For<IUtilities>();
            util.ExpandFileNameWildCards("file.txt").Returns(new List<string> { "file.txt" });

            ParseCommandLine commandLine = new ParseCommandLine() {utilities = util};
            commandLine.Init(new String[] { token, repfile });
            Assert.AreEqual(expected, commandLine.ReplacementFileName);
            Assert.AreEqual(1, commandLine.InputSourceList.Count);
        }

        [Test]
        // kgrep SubjectString filename1
        public void WhenTwoArguments_ExpectReplacementFileAndSourceFile() {
            IUtilities util = Substitute.For<IUtilities>();
            util.ExpandFileNameWildCards("*.txt").Returns(new List<string>{"file1.txt", "file2.txt"});

            ParseCommandLine commandLine = new ParseCommandLine() {utilities = util};
            commandLine.Init(new string[] { "a", "*.txt" });
            Assert.AreEqual(new List<string>{"file1.txt","file2.txt"}, commandLine.InputSourceList);
        }

        [Test]
        // cat filename|kgrep commandFilename
        public void WhenOnlyReplacementFileArgument_ExpectStdinAsInputSource() {
            string[] args = new String[] { "hi~bye" };
            ParseCommandLine cmd = new ParseCommandLine();
            cmd.Init(args);
            Assert.AreEqual("hi~bye", cmd.ReplacementFileName);
            Assert.AreEqual(1, cmd.InputSourceList.Count);
            Assert.AreEqual(cmd.STDIN, cmd.InputSourceList[0]);
        }

        [Test]
        public void WhenNoArguments_ExpectNoArguments() {
            string[] args = new String[] { "" };
            ParseCommandLine cmd = new ParseCommandLine();
            cmd.Init(args);
            Assert.AreEqual("", cmd.ReplacementFileName);
            Assert.AreEqual(1, cmd.InputSourceList.Count);  // empty string
            Assert.AreEqual(cmd.STDIN, cmd.InputSourceList[0]);
        }
    }
}
