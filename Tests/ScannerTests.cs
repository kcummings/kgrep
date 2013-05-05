using System.Collections.Generic;
using kgrep;
using NUnit.Framework;

namespace Tests {
 
        [TestFixture]
        public class ScannerTests {

            [Test]
            public void WhenNoScanTokenGiven_ExpectNoChange() {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string results = engine.ApplyReplacements("", new List<string> { "today bye bye birdie" });
                Assert.AreEqual("today bye bye birdie\n", results);
            }

            [Test]
            public void WhenNoMatchFound_ExpectNoOutput() {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string results = engine.ApplyReplacements("abc", new List<string> { "def" });
                Assert.AreEqual("", results);
            }

            [ExpectedException(typeof(System.Exception))]
            public void WhenInvalidTokenGiven_ExpectException() {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string results = engine.ApplyReplacements("a[bc", new List<string> { "def" });
                Assert.AreEqual("", results);
            }

            // Args for cases: expected, pattern, input
            [TestCase("bc\n", "a(bc)", "abc")]
            [TestCase("bc\n89\n", "(..) ([0-9]+)", "abc 89")]
            [TestCase("ell\no\n", "h(...)(.)", "hello world")]
            public void WhenGroupsGiven_ExpectSubsetOutput(string expected, string scantoken, string input) {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string results = engine.ApplyReplacements(scantoken, new List<string> { input });
                Assert.AreEqual(expected, results);
            }

            [TestCase("b\n", "b", "abc")]
            [TestCase("bye\nbye\n", "bye", "today bye bye birdie")]
            [TestCase("birdie\n", "bye bye (.*?)$", "today bye bye birdie")]
            public void WhenNonRegexTokenGiven_ExpectSimpleOutput(string expected, string scantoken, string input) {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string results = engine.ApplyReplacements(scantoken, new List<string> {input });
                Assert.AreEqual(expected, results);
            }

            [TestCase("a\nb\nc\n \nd\n", ".", "abc d")]  // chars on seperate lines
            [TestCase("8\n5\n1\n2\n3\n", "[0-9]", "85 dollars 123 lost")]  // digits on seperate lines
            [TestCase("85\n123\n", "[0-9]+", "85 dollars 123 lost")]  // numbers on seperate lines
            public void WhenRegexTokenGiven_ExpectFilteredOutput(string expected, string scantoken, string input) {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string results = engine.ApplyReplacements(scantoken, new List<string> { input });
                Assert.AreEqual(expected, results);
            }

            [TestCase("a b ca", "ScannerFS=\", \"; a", "a, a\n")]   // ", " delimited
            [TestCase("a b ca", "ScannerFS=,; a", "a,a\n")]         // "," delimited
            public void WhenScannerFSUsed_ExpectDelimitedOutput(string input, string pattern, string expected) {
                ReplacerEngine engine = new ReplacerEngine() { sw = new WriteToString() };
                string result = engine.ApplyReplacements(pattern, new List<string> { input });
                Assert.AreEqual(expected, result);
            }

        }
    }
