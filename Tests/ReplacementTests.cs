using System.Collections.Generic;
using kgrep;
using NUnit.Framework;

namespace Tests {

    [TestFixture]
    public class ReplacementTests {

        [Test]
        public void WhenEnclosedQuotesInTopattern_ExpectQuotesRemoved() {
            Command command = new Command("this", "\" with blanks \"");
            Assert.AreEqual(" with blanks ", command.ReplacementString);
        }

        [Test]
        public void WhenEnclosedQuotesInFrompattern_ExpectQuotesRemoved() {
            Command command = new Command("\" from  \"", "to");
            Assert.AreEqual(" from  ", command.SubjectString.ToString());
        }

        [Test]
        public void WhenEnclosedQuotesInAnchor_ExpectQuotesRemoved() {
            Command command = new Command("\" AnchorString  \"", " from  ", "to");
            Assert.AreEqual(" AnchorString  ", command.AnchorString);
        }

        [Test]
        public void WhenEnclosedQuotesInTopatternWithExcessSpacesFollowing_ExpectQuotesRemoved() {
            Command command = new Command("this", "\" with blanks \"  ");
            Assert.AreEqual(" with blanks ", command.ReplacementString);
        }

        [Test]
        public void WhenEnclosedQuotesInTopatternWithExcessSpacesPreceeding_ExpectQuotesRemoved() {
            Command command = new Command("this", "   \" with blanks \"");
            Assert.AreEqual(" with blanks ", command.ReplacementString);
        }

    }

}
