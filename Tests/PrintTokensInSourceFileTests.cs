using System;
using System.Collections.Generic;
using kgrep;
using NUnit.Framework;

namespace Tests {
 
        [TestFixture]
        public class ScannerTests {

            [Test]
            public void WhenNoScanTokenGiven_ExpectNoChange() {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile("bye");
                string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "today bye bye birdie" });
                Assert.AreEqual("byebye\n", results);
            }

            [Test]
            public void WhenOnlyScanTokensGiven_ExpectScannerEnabled() {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile("a; b; c");
                Assert.AreEqual(ParseCommandFile.RunningAs.Scanner, commands.kgrepMode);
            }

            [Test]
            public void WhenReplacementTokenGiven_ExpectScannerDisabled() {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile("a; b; b~c");
                Assert.AreEqual(ParseCommandFile.RunningAs.ReplaceAll, commands.kgrepMode);
            }

            [Test]
            public void WhenNoMatchFound_ExpectNoOutput() {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile("abc");
                string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "def" });
                Assert.AreEqual("", results);
            }

            [Test]
            public void WhenAnchorMatchFound_ExpectOutput() {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile("/world/ [0-9]+");
                string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "def", "89 hello world", "hello 09182 worlds" });
                Assert.AreEqual("89\n09182\n", results);
            }

            [Test]
            public void WhenRegexAnchorMatchFound_ExpectOutput() {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile("/([l]{3})/ [0-9]+");
                string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "def", "55 hello ", "helllo 555 worlds"});
                Assert.AreEqual("555\n", results);
            }

            [ExpectedException(typeof(System.Exception))]
            public void WhenInvalidTokenGiven_ExpectException() {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile("a[bc");
                string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "def" });
                Assert.AreEqual("", results);
            }

            // Args for cases: expected, pattern, input
            [TestCase("bc\n", "a(bc)", "abc")]
            [TestCase("bc\n89\n", "OFS='\\n';(..) ([0-9]+)", "abc 89")]
            [TestCase("ell\no\n", @"OFS='\n';h(...)(.)", "hello world")]
            public void WhenGroupsGiven_ExpectSubsetOutput(string expected, string scantoken, string input) {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile(scantoken);
                string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { input });
                Assert.AreEqual(expected, results);
            }

            [TestCase("b\n", "b", "abc")]
            [TestCase("byebye\n", "bye", "today bye bye birdie")]
            [TestCase("birdie\n", "bye bye (.*?)$", "today bye bye birdie")]
            public void WhenNonRegexTokenGiven_ExpectSimpleOutput(string expected, string scantoken, string input) {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile(scantoken);
                string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { input });
                Assert.AreEqual(expected, results);
            }

            [TestCase("a\nb\nc\n \nd\n", @"OFS='\n';.", "abc d")]  // chars on seperate lines
            [TestCase("8\n5\n1\n2\n3\n", @"OFS='\n';[0-9]", "85 dollars 123 lost")]  // digits on seperate lines
            [TestCase("85\n123\n", @"OFS='\n';[0-9]+", "85 dollars 123 lost")]  // numbers on seperate lines
            public void WhenRegexTokenGiven_ExpectFilteredOutput(string expected, string scantoken, string input) {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile(scantoken);
                string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { input });
                Assert.AreEqual(expected, results);
            }

            [TestCase("a b ca", "OFS=\", \"; a", "a, a\n")]   // ", " delimited
            [TestCase("a b ca", "OFS=', '; a", "a, a\n")]   // ", " delimited
            [TestCase("a b ca", "OFS=,; a", "a,a\n")]         // "," delimited
            public void WhenOFSUsed_ExpectDelimitedOutput(string input, string pattern, string expected) {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile(pattern);
                string result = engine.ApplyCommandsToInputFileList(commands, new List<string> { input });
                Assert.AreEqual(expected, result);
            }

        }
    }
