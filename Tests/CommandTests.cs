using System.Collections.Generic;
using kgrep;
using NUnit.Framework;

namespace Tests {

    [TestFixture]
    public class CommandTests {

        [Test]
        public void WhenUsingTemplateTarget_ExpectSplitOnTemplate() {
            Command command = new Command("/ hello/ a->b");
            Assert.AreEqual("a", command.SubjectString);
        }

        [Test]
        public void WhenUsingTildaInSubjectAndTemplateTarget_ExpectSplitOnTilda() {
            Command command = new Command("/ hello/ a~ c->b");
            Assert.AreEqual("a", command.SubjectString);
        }

        [Test]
        public void WhenUsingTildaInTargetAndTemplateTarget_ExpectSplitOnTemplate() {
            Command command = new Command("/ hello/ (..) c-> b~dog");
            Assert.AreEqual("b~dog", command.ReplacementString);
        }

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
            Assert.AreEqual("a", command.SubjectString);
        }

        [Test]
        public void WhenOnlySubjectInPattern() {
            Command command = new Command(" a");
            Assert.AreEqual("a", command.SubjectString);
        }

        [Test]
        public void WhenSubjectInPatternWIthSpaces() {
            Command command = new Command(" a ~b");
            Assert.AreEqual("a", command.SubjectString);
        }

        [Test]
        public void WhenEnclosedQuotesInTopattern_ExpectQuotesRemoved() {
            Command command = new Command("this ~ \" with blanks \"");
            Assert.AreEqual(" with blanks ", command.ReplacementString);
        }

        [Test]
        public void WhenEnclosedQuotesInFrompattern_ExpectQuotesRemoved() {
            Command command = new Command("\" from  \" ~ to");
            Assert.AreEqual(" from  ", command.SubjectRegex.ToString());
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

        [Test]
        public void WhenOFSused_LetItBecomeDefaultUntilChanged() {
            PrintTokensInSourceFiles engine = new PrintTokensInSourceFiles() { sw = new WriteToString() };
            ParseCommandFile commands = new ParseCommandFile("(.);OFS=' ';(.);(.); OFS='+';(.)");
            string results = engine.ApplyCommandsToInputFileList(commands, new List<string> { "de" });
            Assert.AreEqual("d\ne\nd e\nd e\nd+e\n", results);
        }

        [TestCase ("/abc/ from  ~ to", Command.CommandType.isAnchoredReplace,"abc","from","~","to")]
        [TestCase("from  ~ to", Command.CommandType.isReplace,"","from","~","to")]
        [TestCase("/abc/ from ~", Command.CommandType.isAnchoredReplace,"abc","from","~","")]
        [TestCase("from  ~", Command.CommandType.isReplace,"","from","~","")]
        [TestCase("/abc/ from  -> to", Command.CommandType.isAnchoredTemplate,"abc","from","->","to")]
        [TestCase("/abc/-> to", Command.CommandType.isAnchoredTemplate,"abc",".","->","to")]
        [TestCase("from  -> to", Command.CommandType.isTemplate,"","from","->","to")]
        [TestCase("/abc/ from  ->", Command.CommandType.isNotSupported,"abc","from","->","")]
        [TestCase("from  ->", Command.CommandType.isNotSupported,"","from","->","")]
        [TestCase("->to", Command.CommandType.isTemplate,"",".","->","to")]
        [TestCase("/abc/ ", Command.CommandType.isPickupOnly,"","abc","","")]
        [TestCase("from to", Command.CommandType.isScan,"","from to","","")]
        [TestCase("->", Command.CommandType.isNotSupported, "", "", "->", "")]
        [TestCase("s->", Command.CommandType.isNotSupported, "", "s", "->", "")]
        [TestCase("/a/~t", Command.CommandType.isNotSupported, "a", "", "~", "t")]
        [TestCase("/a/~", Command.CommandType.isNotSupported, "a", "", "", "")]

        public void TestCommandTypes(string line, Command.CommandType type, string anchor, string subject, string delim, string target) {
            Command command = new Command(line);
            Assert.AreEqual(type, command.CommandIs);
            Assert.AreEqual(anchor, command.AnchorString);
            Assert.AreEqual(subject,command.SubjectString);
        }
    }

}
