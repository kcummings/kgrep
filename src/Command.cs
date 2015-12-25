using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace kgrep
{
    public class Command {

        public String AnchorString = "";
        public String ReplacementString = "";
        public Regex SubjectRegex;
        public String SubjectString ="";
        private string delimiter;
        public bool IsPickupOnly = false;
        public bool IsCaptureInSubjectString = false;
        public bool IsPickupInReplacementString = false;   // pickup syntax: ${name}
        private static Regex allParensPattern = new Regex(@"(\(\?<.+?>.+?\)|\(.*?\))",RegexOptions.Compiled);
        private static Regex nonCapturingPattern = new Regex(@"\(\?(?:[^<=]|<=|<!).*?\)", RegexOptions.Compiled);
        public static Regex PickupPattern = new Regex(@"\$\{(.+?)\}", RegexOptions.Compiled);
        private Pickup _pickup = new Pickup();
        public string OFS;  // Output Field Seperator use by scanner only, like AWK's

        public enum CommandType {
            isReplace,
            isTemplate,
            isAnchoredReplace,
            isAnchoredTemplate,
            isPickupOnly,
            isAnchoredScan,
            isScan,
            isNotSupported
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
        public void ParseLine(string line, string delim) {
            try {

                string pat = String.Format("(?:/(?<anchor>.*?)/)?(?:(?<subject>.*?)?(?<delim>->|{0})(?<target>.*)|(?<subject>.*))", delim);
                Regex commandTypeRegex = new Regex(pat);
                MatchCollection mc = commandTypeRegex.Matches(line);
                if (mc.Count == 0) {
                    throw new Exception(String.Format("Invalid Command syntax for '{0}'", line));
                }
                Match m = mc[0];
                AnchorString = RemoveEnclosingQuotesIfPresent(m.Groups["anchor"].Value);
                SubjectString = m.Groups["subject"].Value.Trim();
                delimiter = m.Groups["delim"].Value;
                ReplacementString = m.Groups["target"].Value.Trim();
                CommandIs = GetCommandType(AnchorString, SubjectString, delimiter, ReplacementString);

                SubjectString = RemoveEnclosingQuotesIfPresent(SubjectString.Trim());

                // if command is "/abc/" pattern without a subject or replace, treat it as a pickup and don't print it.
                if (CommandIs == CommandType.isPickupOnly) {
                    SubjectString = AnchorString;
                    AnchorString = "";
                }

                // Since subject is requires but "->abc" is a valid command, add a substitue subject for template without a subject.
                if ((CommandIs == CommandType.isTemplate || CommandIs == CommandType.isAnchoredTemplate) && String.IsNullOrEmpty(SubjectString))
                    SubjectString = ".";               
                
                SubjectString = _pickup.ReplaceShorthandPatternWithFormalRegex(SubjectString);
                SubjectRegex = new Regex(SubjectString, RegexOptions.Compiled);
                ReplacementString = RemoveEnclosingQuotesIfPresent(ReplacementString.Trim());
                IsCaptureInSubjectString = allParensPattern.Match(SubjectString).Success;
                IsPickupInReplacementString = PickupPattern.Match(ReplacementString).Success;

            } catch (Exception e) {
                Console.WriteLine("Regex error Command, from '{0}'  to '{1}'  AnchorString '{2}'", SubjectString,
                                  ReplacementString, AnchorString);
                throw new Exception(e.Message);
            }
            return;
        }

        private CommandType GetCommandType(string anchor, string subject, string delim, string target) {
            bool hasAnchor = !String.IsNullOrEmpty(anchor);
            bool hasSubject = !String.IsNullOrEmpty(subject);
            bool hasTarget = !String.IsNullOrEmpty(target);
            bool hasDelimter = !String.IsNullOrEmpty(delim);

            // Determine the type of command.
            // For a Truth Table explaining possibilities, see docs/CommandTypeTruthTable.xlsx
            int state = 0;
            if (hasAnchor) state += 1;
            if (hasSubject) state += 2;
            if (hasDelimter) state += 4;
            if (hasTarget) state += 8;
            
            // Scan only related commands.
            if (String.IsNullOrEmpty(delim)) {
                if (state==3) return CommandType.isAnchoredScan;
                if (state==1) return CommandType.isPickupOnly;
                if (state==2) return CommandType.isScan;
                return CommandType.isNotSupported;
            }

            // Template related search and replace commands.
            if (delim == "->") {
                if (state == 15) return CommandType.isAnchoredTemplate;
                if (state == 13) return CommandType.isAnchoredTemplate;
                if (state==14) return CommandType.isTemplate;
                if (state == 12) return CommandType.isTemplate;
                return CommandType.isNotSupported;
            }

            // Search and repalce commands.
            if (state==15) return CommandType.isAnchoredReplace;
            if (state == 7) return CommandType.isAnchoredReplace;
            if (state == 14) return CommandType.isReplace;
            if (state == 6) return CommandType.isReplace;
            return CommandType.isNotSupported;
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
            return CommandIs != CommandType.isNotSupported;
        }
    }
}
