using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using kgrep;

namespace Tests {

    [TestFixture]
    public class PickupTests {

        [Test]
        public void TestStandardPickup() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            ReplacementFile rf = new ReplacementFile("{{pickup}=[a-z]+; a~b");
            Pickup result = rf.PickupDefinitions["{pickup}"];
            Assert.AreEqual(new Regex("[a-z]+", RegexOptions.Compiled).ToString(), result.PickupPattern.ToString());
        }

        [Test]
        public void TestShortNamePickup() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            ReplacementFile rf = new ReplacementFile("{a}=[a-z]+; a~b");
            Pickup result = rf.PickupDefinitions["{a}"];
            Assert.AreEqual(new Regex("[a-z]+", RegexOptions.Compiled).ToString(), result.PickupPattern.ToString());
        }

        [Test]
        public void TestNoNamePickup() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            ReplacementFile rf = new ReplacementFile("{}=[a-z]+; a~b");  // pickup names must be at least one char
            Assert.IsTrue(rf.PickupDefinitions.Count == 0);
        }

        [Test]
        public void TestInvalidCharacterInNamePickup() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            ReplacementFile rf = new ReplacementFile("{a-b}=[a-z]+; a~b");  // only alphanumeric allowed in name
            Assert.IsTrue(rf.PickupDefinitions.Count == 0);
        }

        [Test]
        public void TestTwoPickups() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            ReplacementFile rf = new ReplacementFile("{pickup}=[a-z]+; a~b; {head} = [0-9]+");
            Pickup result = rf.PickupDefinitions["{pickup}"];
            Assert.AreEqual(new Regex("[a-z]+", RegexOptions.Compiled).ToString(), result.PickupPattern.ToString());
            result = rf.PickupDefinitions["{head}"];
            Assert.AreEqual(new Regex("[0-9]+", RegexOptions.Compiled).ToString(), result.PickupPattern.ToString());
            Assert.IsTrue(rf.PickupDefinitions.Count == 2);
        }

        [Test]
        public void TestInvalidPickup() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            ReplacementFile rf = new ReplacementFile("{pickup=[a-z]+; a~b");  // missing closing "}" 
            Assert.IsTrue(rf.PickupDefinitions.Count == 0);
        }

        [Test]
        public void TestStetchedoutPickup() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            ReplacementFile rf = new ReplacementFile(" {pickup}  =  [a-z]+  ; a~b");
            Pickup result = rf.PickupDefinitions["{pickup}"];
            Assert.AreEqual(new Regex("[a-z]+", RegexOptions.Compiled).ToString(), result.PickupPattern.ToString());
        }

        [Test]
        public void TestComplexPickupPattern() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            ReplacementFile rf = new ReplacementFile(@"{pickup} = hi([a-z]) [0-9]{3} + ; a~b");
            Pickup result = rf.PickupDefinitions["{pickup}"];
            Assert.AreEqual(new Regex(@"hi([a-z]) [0-9]{3} +", RegexOptions.Compiled).ToString(), result.PickupPattern.ToString());
        }

        [Test]
        public void TestManyPickups() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            ReplacementFile rf = new ReplacementFile("{a}=[a-z]+; a~b; {b} = [0-9]+; {c}=.; {def}=..");
            Assert.IsTrue(rf.PickupDefinitions.Count == 4);
            Pickup result = rf.PickupDefinitions["{b}"];
            Assert.AreEqual(new Regex("[0-9]+", RegexOptions.Compiled).ToString(), result.PickupPattern.ToString());
            result = rf.PickupDefinitions["{def}"];
            Assert.AreEqual(new Regex("..", RegexOptions.Compiled).ToString(), result.PickupPattern.ToString());
        }

        [Test]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void TestMissingNameInDictionaryPickup() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            ReplacementFile rf = new ReplacementFile("{a}=[a-z]+; a~b");
            Pickup result = rf.PickupDefinitions["{b}"];
            Assert.AreEqual(new Regex("[a-z]+", RegexOptions.Compiled).ToString(), result.PickupPattern.ToString());
        }


        // Pickup placeholder tests
        [Test]
        public void TestReplacePickup() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            string result = engine.SearchAndReplaceTokens("{pickup}=[0-9]+", new List<string> { " hello 89", "hi {pickup} world" });
            Assert.AreEqual(" hello 89\nhi 89 world\n", result);
        }

        [Test]
        public void TestReportingPickup() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            string result = engine.SearchAndReplaceTokens("{num}=[0-9]+; {word}=[a-zA-Z]+; results~word was {word} ", new List<string> { " hello 89 bye", "results" });
            Assert.AreEqual(" hello 89 bye\nword was hello\n", result);
        }

        [Test]
        public void TestUsePickupTwice() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            string result = engine.SearchAndReplaceTokens("{num}=[0-9]+; {word}=[a-zA-Z]+", new List<string> { " hello 89", "hi {num}-{word}-{num} world" });
            Assert.AreEqual(" hello 89\nhi 89-hello-89 world\n", result);
        }

        [Test]
        public void TestLinuxFoundation() {
            KgrepEngine engine = new KgrepEngine() { sw = new WriteToString() };
            string result = engine.SearchAndReplaceTokens(
                @"Linux~Linus; {w1}=<.*?>; <for>~{w1}; ed~es; \s*$~", 
                    new List<string> { "The Linux Foundation promotes, protects and advances Linux", 
                                       "by providing <unified> resources",
                                       "and services needed <for> open source"});
            Assert.AreEqual("The Linus Foundation promotes, protects and advances Linus\n"
                                +       "by providing <unifies> resources\n"
                                +       "and services neeses <unified> open source\n", result);
        }
    }
}
