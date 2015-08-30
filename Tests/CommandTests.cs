using System.Collections.Generic;
using kgrep;
using NUnit.Framework;

namespace Tests {

    [TestFixture]
    public class CommandTests {

        [Test]
        public void WhenAnchorInTopattern_ExpectAnchor() {
            Command command = new Command("/ hello/ a~b");
            Assert.AreEqual(" hello", command.AnchorString);
        }

        [Test]
        public void WhenNoAnchor_ExpectNoValue() {
            Command command = new Command("  a~b");
            Assert.AreEqual("", command.AnchorString);
        }

        [Test]
        public void WhenReplacementInTopattern() {
            Command command = new Command("/ hello/ a~b");
            Assert.AreEqual("b", command.ReplacementString);
        }

        [Test]
        public void WhenAnchorAndSubjectInPattern() {
            Command command = new Command("/ hello/ a~b");
            Assert.AreEqual("a", command.test);
        }

        [Test]
        public void WhenOnlySubjectInPattern() {
            Command command = new Command(" a");
            Assert.AreEqual("a", command.test);
        }

        [Test]
        public void WhenSubjectInPatternWIthSpaces() {
            Command command = new Command(" a ~b");
            Assert.AreEqual("a", command.test);
        }

        [Test]
        public void WhenEnclosedQuotesInTopattern_ExpectQuotesRemoved() {
            Command command = new Command("this ~ \" with blanks \"");
            Assert.AreEqual(" with blanks ", command.ReplacementString);
        }

        [Test]
        public void WhenEnclosedQuotesInFrompattern_ExpectQuotesRemoved() {
            Command command = new Command("\" from  \" ~ to");
            Assert.AreEqual(" from  ", command.SubjectString.ToString());
        }

        [Test]
        public void WhenEnclosedQuotesInAnchor_ExpectQuotesRemoved() {
            Command command = new Command("/\" AnchorString  \"/ from  ~ to");
            Assert.AreEqual(" AnchorString  ", command.AnchorString);
        }

        [Test]
        public void WhenEnclosedQuotesInTopatternWithExcessSpacesFollowing_ExpectQuotesRemoved() {
            Command command = new Command("this ~ \" with blanks \"  ");
            Assert.AreEqual(" with blanks ", command.ReplacementString);
        }

        [Test]
        public void WhenEnclosedQuotesInTopatternWithExcessSpacesPreceeding_ExpectQuotesRemoved() {
            Command command = new Command("this  ~ \" with blanks \"");
            Assert.AreEqual(" with blanks ", command.ReplacementString);
        }

    }

}
