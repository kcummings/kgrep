using System.Collections.Generic;
using kgrep;
using NUnit.Framework;

namespace Tests {

    [TestFixture]
    public class ReplacementTests {

        [Test]
        public void WhenEnclosedQuotesInTopattern_ExpectQuotesRemoved() {
            Replacement rep = new Replacement("this", "\" with blanks \"");
            Assert.AreEqual(" with blanks ", rep.ToPattern);
        }

        [Test]
        public void WhenEnclosedQuotesInFrompattern_ExpectQuotesRemoved() {
            Replacement rep = new Replacement("\" from  \"", "to");
            Assert.AreEqual(" from  ", rep.FromPattern.ToString());
        }

        [Test]
        public void WhenEnclosedQuotesInAnchor_ExpectQuotesRemoved() {
            Replacement rep = new Replacement("\" AnchorPattern  \"", " from  ", "to");
            Assert.AreEqual(" AnchorPattern  ", rep.AnchorPattern);
        }

        [Test]
        public void WhenEnclosedQuotesInTopatternWithExcessSpacesFollowing_ExpectQuotesRemoved() {
            Replacement rep = new Replacement("this", "\" with blanks \"  ");
            Assert.AreEqual(" with blanks ", rep.ToPattern);
        }

        [Test]
        public void WhenEnclosedQuotesInTopatternWithExcessSpacesPreceeding_ExpectQuotesRemoved() {
            Replacement rep = new Replacement("this", "   \" with blanks \"");
            Assert.AreEqual(" with blanks ", rep.ToPattern);
        }

    }

}
