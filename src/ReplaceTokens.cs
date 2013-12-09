using System.Collections.Generic;
using System.Text.RegularExpressions;
using NLog;

namespace kgrep {

    public class ReplaceTokens : IFileAction {
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        public IHandleOutput sw = new WriteStdout();
        public Dictionary<string, string> PickupList = new Dictionary<string, string>();
        protected int _countOfMatchesInFile = 0;
        protected int _lineNumber = 0;
        private Command _command;

        public virtual string ApplyCommands(ParseCommandFile rf, List<string> inputFilenames) {
            return "";
        }


        protected string ApplySingleCommand(string line, Command command) {
            _command = command;
            if (command.Style == Command.CommandType.Print)
                line = ReplacePickupsWithStoredValue(command.ReplacementString);   
            else {
                CollectPickupValues(line, command);
                line = ReplaceIt(command.SubjectString, line, ReplacePickupsInReplacementString(command.ReplacementString));
            }
            return line;
        }

        private string ReplacePickupsInReplacementString(string repString) {
            if (_command.IsPickupInReplacementString) {
                return ReplacePickupsWithStoredValue(repString);
            }
            return repString;
        }

        private string ReplaceIt(Regex re, string source, string target) {
            int count = re.Matches(source).Count;
            if (count>0) {
                logger.Debug("   At line {0} found {1} occurances of '{2}' in '{3}'", _lineNumber, count, re.ToString(), source);
                _countOfMatchesInFile += count;
                return re.Replace(source, target);
            }
            return source;
        }

        // Values are in Named and unnamed Captures are only in SubjectString.
        // e.g. named capture syntax: (?<digit>[0-9]+)  yeilds pickup name ${digit} 
        //    unnamed capture syntax: ([0-9]+)    yeilds pickup name ${1}
        protected void CollectPickupValues(string line, Command command) {
            if (command.IsCaptureInSubjectString) {
                Match m = command.SubjectString.Match(line);
                if (m.Success) {
                    foreach (int groupNumber in command.SubjectString.GetGroupNumbers()) {
                        string name = command.SubjectString.GroupNameFromNumber(groupNumber);
                        if (PickupList.ContainsKey(name))
                            PickupList[name] = m.Groups[groupNumber].Value;
                        else
                            PickupList.Add(name, m.Groups[groupNumber].Value);
                    }
                }
            }
        }

        protected string ReplacePickupsWithStoredValue(string line) {
            string PickupValue;

            MatchCollection mc = Command.PickupPattern.Matches(line);
            foreach (Match m in mc) {
                PickupValue = m.Groups[1].Value;
                if (PickupList.ContainsKey(PickupValue))
                    line = line.Replace("${" + PickupValue + "}", PickupList[PickupValue]);
            }
            return line;
        }

        protected bool isCandidateForReplacement(string line, Command command) {
            // Has a matching AnchorString?
            if (command.AnchorString.Length > 0) {
                if (Regex.IsMatch(line, command.AnchorString))
                    return true;
                logger.Trace("   is not a Command candidate");
                return false;
            }
            return true;
        }
    }
}
