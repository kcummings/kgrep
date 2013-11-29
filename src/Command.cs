using System;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using NLog;

namespace kgrep
{
    public class Command {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [XmlIgnore] public String AnchorString = "";
        public String ReplacementString = "";
        public Regex SubjectString = new Regex("");
        public CommandType Style;
        public string ScannerFS = "\n";
        public int CountOfCapturesInSubjectString = 0;
        public int CountOfPickupsInReplacementString = 0;   // pickup syntax: ${name}

        public enum CommandType {
            Pickup,
            Anchored,
            Normal,
            Print
        }

        public Command() {
        }

        public Command(string anchorString, string subjectString, string replacementString) {
            _command(anchorString, subjectString, replacementString);
            Style = CommandType.Anchored;
        }

        public Command(string subjectString, string replacementString) {
            _command("", subjectString, replacementString);
            Style = subjectString.Length > 0 ? CommandType.Normal : CommandType.Print;
        }

        public Command(string scanPattern) {
            _command("", scanPattern, "");
            Style = CommandType.Pickup;
        }

        private void _command(string anchorString, string subjectString, string replacementString) {
            try {
                logger.Trace("   _command - AnchorString:'{0}' SubjectString:'{1}' ReplacementString:'{2}'", anchorString, subjectString,
                             replacementString);
                AnchorString = RemoveEnclosingQuotesIfPresent(anchorString.Trim());
                subjectString = RemoveEnclosingQuotesIfPresent(subjectString.Trim());
                CountOfCapturesInSubjectString = CountMatchesInString(@"(\(\?<.+?>.+?\)|\(.*?\))", subjectString);  // count named and unnamed captures
                CountOfPickupsInReplacementString = CountMatchesInString(@"\$\{.+?\}", replacementString);
                SubjectString = new Regex(subjectString, RegexOptions.Compiled);
                ReplacementString = RemoveEnclosingQuotesIfPresent(replacementString.Trim());

                // Just validation here. Will the given pattern throw an exception?
                Regex topat = new Regex(ReplacementString);
                Regex anc = new Regex(AnchorString);
            }
            catch (Exception e) {
                Console.WriteLine("Regex error Command, from '{0}'  to '{1}'  AnchorString '{2}'", subjectString,
                                  replacementString, AnchorString);
                throw new Exception(e.Message);
            }
        }

        private string RemoveEnclosingQuotesIfPresent(string pattern) {
            string pat = pattern.Trim();
            if (pattern.StartsWith("\"") && pattern.EndsWith("\"")) {
                string patternWithoutQuotes = pat.Substring(1, pat.Length - 2);
                logger.Trace("Removed quotes ({0} --> {1})", pattern, patternWithoutQuotes);
                return patternWithoutQuotes;
            }
            return pattern; // return the original string untouched
        }

        private static int CountMatchesInString(string pattern, string line) {
            try {
               Regex regex = new Regex(pattern);
               return regex.Matches(line).Count;
            }
            catch(Exception e)
            {
                logger.Debug(String.Format("CountMatchesInString - Looking for '{0}' in '{1}' \n{2}",pattern,line,e.Message));
                return 0;
            }
        }
    }
}
