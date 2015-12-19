using System;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace kgrep
{
    public class Command {

        public String AnchorString = "";
        public String ReplacementString = "";
        public Regex SubjectRegex;
        public String SubjectString ="";
        public bool IsAnchored {get { return !String.IsNullOrEmpty(AnchorString); }}
        public bool IsPickup {get { return _parts.Length == 1; }}
        public bool IsPickupOnly = false;
        public bool IsNormal {get { return !IsAnchored && !IsPickup;}}
        public bool IsCaptureInSubjectString = false;
        public bool IsPickupInReplacementString = false;   // pickup syntax: ${name}
        public bool IsUsingTargetTemplate = false;     // subject -> target rather than subject ~ target
        private static Regex allParensPattern = new Regex(@"(\(\?<.+?>.+?\)|\(.*?\))",RegexOptions.Compiled);
        private static Regex nonCapturingPattern = new Regex(@"\(\?(?:[^<=]|<=|<!).*?\)", RegexOptions.Compiled);
        public static Regex PickupPattern = new Regex(@"\$\{(.+?)\}", RegexOptions.Compiled);
        private Pickup _pickup = new Pickup();
        private String[] _parts;
        public string OFS;  // Output Field Seperator use by scanner only, like AWK's

        public enum CommandType {
            isReplace,
            isTemplate,
            isAnchoredReplace,
            isAnchoredTemplate,
            isPickupOnly,
            isFindAndPrint
        }

        public CommandType CommandIs = CommandType.isReplace;

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
        //          "/AnchorString/ SubjectRegex -> ReplacementString
        //     Only SubjectRegex is required.
        //     Parse line and init parameters.
        public void ParseLine(string rawcommand, string delim) {
            try {
                
                AnchorString = Regex.Match(rawcommand, @"^\s*/(.*?)/").Groups[1].Value;
                AnchorString = RemoveEnclosingQuotesIfPresent(AnchorString);
                if (!String.IsNullOrEmpty(AnchorString)) {
                    rawcommand = Regex.Replace(rawcommand, @"^\s*/(.*?)/", "");
                }

                _parts = rawcommand.Split(new string[] {"->"}, StringSplitOptions.None);  // use "template" as target. Not a simple replace
                if (_parts.Length == 2) 
                    IsUsingTargetTemplate = true;
                else // "->" not found
                     _parts = rawcommand.Split(delim.ToCharArray(), 4);
                SubjectString = RemoveEnclosingQuotesIfPresent(_parts[0].Trim());

                // if command is "/abc/" pattern without a subject or replace, treat it as a pickup and don't print it.
                if (!String.IsNullOrEmpty(AnchorString) && String.IsNullOrEmpty(SubjectString)) {
                    IsPickupOnly = true;
                    SubjectString = AnchorString;
                    AnchorString = "";
                }

                CommandIs = GetCommandType();

                // Since subject is requires but "->abc" is a valid command, add a substitue subject for template without a subject.
                if (CommandIs == CommandType.isTemplate && String.IsNullOrEmpty(SubjectString))
                    SubjectString = ".";               
                
                SubjectString = _pickup.ReplaceShorthandPatternWithFormalRegex(SubjectString);
                SubjectRegex = new Regex(SubjectString, RegexOptions.Compiled);
                if (_parts.Length == 2)
                    ReplacementString = RemoveEnclosingQuotesIfPresent(_parts[1].Trim());
                SetType();



            } catch (Exception e) {
                Console.WriteLine("Regex error Command, from '{0}'  to '{1}'  AnchorString '{2}'", SubjectString,
                                  ReplacementString, AnchorString);
                throw new Exception(e.Message);
            }
            return;
        }

        private CommandType GetCommandType() {
            bool isAnchored = !String.IsNullOrEmpty(AnchorString);
            bool isNormal = _parts.Length == 2;

            if (IsPickupOnly)
                return CommandType.isPickupOnly;
            if (isAnchored && IsUsingTargetTemplate)
                return CommandType.isAnchoredTemplate;
            if (IsUsingTargetTemplate)
                return CommandType.isTemplate;  
            if (isAnchored && isNormal)
                return CommandType.isAnchoredReplace;
            if (isNormal)
                return CommandType.isReplace;
            return CommandType.isFindAndPrint;
        }

        private void SetType() {
            IsCaptureInSubjectString = allParensPattern.Match(SubjectString).Success;
            IsPickupInReplacementString = PickupPattern.Match(ReplacementString).Success;
            return;
        }

        private string RemoveEnclosingQuotesIfPresent(string pattern) {
            string pat = pattern.Trim();
            if (pattern.StartsWith("\"") && pattern.EndsWith("\"")) {
                string patternWithoutQuotes = pat.Substring(1, pat.Length - 2);
                return patternWithoutQuotes;
            }
            return pattern; // return the original string untouched
        }

        public bool IsValid() {
            if (String.IsNullOrEmpty(SubjectString)) {
                return false;
            }
            if (_parts.Length > 2)
                return false;
            return true;
        }
    }
}
