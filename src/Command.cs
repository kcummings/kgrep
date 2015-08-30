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
        public Regex SubjectRegex;
        public String SubjectString =""; 
        public CommandType Style;
        public string ScannerFS = "\n";
        public bool IsReplaceFirstMatchCommand = false;
        public bool IsCaptureInSubjectString = false;
        public bool IsPickupInReplacementString = false;   // pickup syntax: ${name}
        private static Regex allParensPattern = new Regex(@"(\(\?<.+?>.+?\)|\(.*?\))",RegexOptions.Compiled);
        private static Regex nonCapturingPattern = new Regex(@"\(\?(?:[^<=]|<=|<!).*?\)", RegexOptions.Compiled);
        public static Regex PickupPattern = new Regex(@"\$\{(.+?)\}", RegexOptions.Compiled);
        private Pickup _pickup = new Pickup();
        private String[] parts;

        public enum CommandType {
            Pickup,
            Anchored,
            Normal
        }

        public Command(string line) {
            ParseAndValidateCommand(line, "~");
        }

        public Command(string line, string delim) {
            ParseAndValidateCommand(line, delim);
        }

        void ParseAndValidateCommand(string line, string delim) {
            ParseLine(line, delim);
        }

        //  Sample: "/word/ a~b"
        //          "/AnchorString/ SubjectRegex ~ ReplacementString
        //     Only SubjectRegex is required.
        //     Parse line and init parameters.
        public void ParseLine(string rawcommand, string delim) {
            try {
                AnchorString = Regex.Match(rawcommand, @"^\s*/(.*?)/").Groups[1].Value;
                AnchorString = RemoveEnclosingQuotesIfPresent(AnchorString);
                if (!String.IsNullOrEmpty(AnchorString)) {
                    rawcommand = Regex.Replace(rawcommand, @"^\s*/(.*?)/", "");
                    Style = CommandType.Anchored;
                }

                parts = rawcommand.Split(delim.ToCharArray(), 4);
                SubjectString = RemoveEnclosingQuotesIfPresent(parts[0].Trim());
                SubjectString = _pickup.ReplaceShorthandPatternWithFormalRegex(SubjectString);
                SubjectRegex = new Regex(SubjectString, RegexOptions.Compiled);
                if (parts.Length == 2)
                    ReplacementString = RemoveEnclosingQuotesIfPresent(parts[1].Trim());
                SetType();
                if (parts.Length == 1)
                    Style = CommandType.Pickup;
            } catch (Exception e) {
                Console.WriteLine("Regex error Command, from '{0}'  to '{1}'  AnchorString '{2}'", SubjectString,
                                  ReplacementString, AnchorString);
                throw new Exception(e.Message);
            }

            return;
        }

        private void SetType() {
            if (!String.IsNullOrEmpty(AnchorString))
                Style = CommandType.Anchored;
            else 
                Style = CommandType.Normal;

            IsCaptureInSubjectString = allParensPattern.Match(SubjectString).Success;
            IsPickupInReplacementString = PickupPattern.Match(ReplacementString).Success;
            return;
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

        public bool IsValid() {
            if (String.IsNullOrEmpty(SubjectString)) {
                logger.Debug("Subjectstring cannot be empty - command ignored\nanchor:{0} replacementString:{1}", AnchorString, ReplacementString);
                return false;
            }
            if (parts.Length > 2)
                return false;
            return true;
        }
    }
}
