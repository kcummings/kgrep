using System;
using System.Collections.Generic;
using kgrep;
using NUnit.Framework;

namespace Tests {
 
        [TestFixture]
        public class ScannerTests {


            [Test]
            public void WhenFullCycleWithScannerFS_ExpectDelimitedResults() {
                string[] args = new String[] { "ScannerFS=,; a;b", "a b ca" };
                ParseCommandLine cmd = new ParseCommandLine(args);
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile(cmd.ReplacementFileName);
                string results = engine.ApplyScanner(commands, cmd.InputSourceNames);
                Assert.AreEqual("a,a\nb\n", results);
            }

            [Test]
            public void WhenNoScanTokenGiven_ExpectNoChange() {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile("bye");
                string results = engine.ApplyScanner(commands, new List<string> { "today bye bye birdie" });
                Assert.AreEqual("bye\nbye\n", results);
            }

            [Test]
            public void WhenOnlyScanTokensGiven_ExpectScannerEnabled() {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile("a; b; c");
                Assert.IsTrue(commands.UseAsScanner);
            }

            [Test]
            public void WhenReplacementTokenGiven_ExpectScannerDisabled() {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile("a; b; b~c");
                Assert.IsFalse(commands.UseAsScanner);
            }

            [Test]
            public void WhenNoMatchFound_ExpectNoOutput() {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile("abc");
                string results = engine.ApplyScanner(commands, new List<string> { "def" });
                Assert.AreEqual("", results);
            }

            [ExpectedException(typeof(System.Exception))]
            public void WhenInvalidTokenGiven_ExpectException() {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile("a[bc");
                string results = engine.ApplyScanner(commands, new List<string> { "def" });
                Assert.AreEqual("", results);
            }

            // Args for cases: expected, pattern, input
            [TestCase("bc\n", "a(bc)", "abc")]
            [TestCase("bc\n89\n", "(..) ([0-9]+)", "abc 89")]
            [TestCase("ell\no\n", "h(...)(.)", "hello world")]
            public void WhenGroupsGiven_ExpectSubsetOutput(string expected, string scantoken, string input) {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile(scantoken);
                string results = engine.ApplyScanner(commands, new List<string> { input });
                Assert.AreEqual(expected, results);
            }

            [TestCase("b\n", "b", "abc")]
            [TestCase("bye\nbye\n", "bye", "today bye bye birdie")]
            [TestCase("birdie\n", "bye bye (.*?)$", "today bye bye birdie")]
            public void WhenNonRegexTokenGiven_ExpectSimpleOutput(string expected, string scantoken, string input) {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile(scantoken);
                string results = engine.ApplyScanner(commands, new List<string> { input });
                Assert.AreEqual(expected, results);
            }

            [TestCase("a\nb\nc\n \nd\n", ".", "abc d")]  // chars on seperate lines
            [TestCase("8\n5\n1\n2\n3\n", "[0-9]", "85 dollars 123 lost")]  // digits on seperate lines
            [TestCase("85\n123\n", "[0-9]+", "85 dollars 123 lost")]  // numbers on seperate lines
            public void WhenRegexTokenGiven_ExpectFilteredOutput(string expected, string scantoken, string input) {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile(scantoken);
                string results = engine.ApplyScanner(commands, new List<string> { input });
                Assert.AreEqual(expected, results);
            }

            [TestCase("a b ca", "ScannerFS=\", \"; a", "a, a\n")]   // ", " delimited
            [TestCase("a b ca", "ScannerFS=,; a", "a,a\n")]         // "," delimited
            public void WhenScannerFSUsed_ExpectDelimitedOutput(string input, string pattern, string expected) {
                PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
                ParseCommandFile commands = new ParseCommandFile(pattern);
                string result = engine.ApplyScanner(commands, new List<string> { input });
                Assert.AreEqual(expected, result);
            }

        }
    }
