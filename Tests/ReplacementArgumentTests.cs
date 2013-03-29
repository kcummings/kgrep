using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using kgrep;

namespace Tests {

    [TestFixture]
    public class ReplacementArgumentTests {

        [Test]
        public void TestSimpleArgument() {
            ReplacementFile rf = new ReplacementFile("a~bc");
            List<Replacement> reps = rf.GetReplacements();
            Assert.AreEqual(reps[0].frompattern.ToString(), (new Regex("a".Trim(), RegexOptions.Compiled)).ToString());
            Assert.AreEqual(reps[0].topattern, "bc");
        }

        [Test]
        public void TestTwoArguments() {
            ReplacementFile rf = new ReplacementFile("a~bc; g~jk");
            List<Replacement> reps = rf.GetReplacements();
            Assert.AreEqual(reps[0].topattern, "bc");
            Assert.AreEqual(reps[1].frompattern.ToString(), (new Regex("g".Trim(), RegexOptions.Compiled)).ToString());
            Assert.AreEqual(reps[1].topattern, "jk");
        }

        [Test]
        public void TestThreeArgument() {
            ReplacementFile rf = new ReplacementFile("a~bc;   hello~world  ;  third ~ fourth ");
            List<Replacement> reps = rf.GetReplacements();
            Assert.AreEqual(reps[2].frompattern.ToString(), (new Regex("third".Trim(), RegexOptions.Compiled)).ToString());
            Assert.AreEqual(reps[2].topattern, "fourth");
        }


        [Test]
        public void TestCommentArgument() {
            ReplacementFile rf = new ReplacementFile("#comment; a~bc");
            List<Replacement> reps = rf.GetReplacements();
            Assert.AreEqual(reps[0].frompattern.ToString(), (new Regex("a".Trim(), RegexOptions.Compiled)).ToString());
            Assert.AreEqual(reps[0].topattern, "bc");
        }

        [Test]
        public void TestRemoveOneArgument() {
            ReplacementFile rf = new ReplacementFile("# remove a from arg; a~");
            List<Replacement> reps = rf.GetReplacements();

            KgrepEngine engine = new KgrepEngine();
            string result = engine.ApplyReplacementsAll("a b ca", reps);
            Assert.AreEqual(result, " b c");
        }
    }
}
