using System;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using NLog;

namespace kgrep
{
    public class Command {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public String AnchorString = "";
        public String ReplacementString = "";
        public Regex SubjectString = new Regex("");
        public CommandType Style;
        public string ScannerFS = "\n";
        public bool IsCaptureInSubjectString = false;
        public bool IsPickupInReplacementString = false;   // pickup syntax: ${name}
        private static Regex allParensPattern = new Regex(@"(\(\?<.+?>.+?\)|\(.*?\))",RegexOptions.Compiled);
        private static Regex nonCapturingPattern = new Regex(@"\(\?(?:[^<=]|<=|<!).*?\)", RegexOptions.Compiled);
        public static Regex PickupPattern = new Regex(@"\$\{(.+?)\}", RegexOptions.Compiled);
        private ShorthandRegex _shorthandRegex = new ShorthandRegex();

        public enum CommandType {
            Pickup,
            Anchored,
            Normal
        }

        public Command() {
        }

        public Command(string anchorString, string subjectString, string replacementString) {
            _command(anchorString, subjectString, replacementString);
            Style = CommandType.Anchored;
        }

        public Command(string subjectString, string replacementString) {
            _command("", subjectString, replacementString);
            Style = CommandType.Normal;
        }

        public Command(string scanPattern) {
            _command("", scanPattern, "");
            Style = CommandType.Pickup;
        }

        private void _command(string anchorString, string subjectString, string replacementString) {
            try {
                if (String.IsNullOrEmpty(subjectString)) {
                    logger.Debug("Subjectstring cannot be empty - command ignored\nanchor:{0} replacementString:{1}",anchorString,replacementString);
                    return;
                }
                logger.Trace("   _command - AnchorString:'{0}' SubjectString:'{1}' ReplacementString:'{2}'", anchorString, subjectString,
                             replacementString);
                AnchorString = RemoveEnclosingQuotesIfPresent(anchorString.Trim());
                subjectString = RemoveEnclosingQuotesIfPresent(subjectString.Trim());
                subjectString = _shorthandRegex.ReplaceShorthandPatternWithFormalRegex(subjectString);

                IsCaptureInSubjectString = allParensPattern.Match(subjectString).Success;
                IsPickupInReplacementString = PickupPattern.Match(replacementString).Success;
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
    }
}
