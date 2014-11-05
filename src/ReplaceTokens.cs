using System.Collections.Generic;
using System.Text.RegularExpressions;
using NLog;

namespace kgrep {

    public class ReplaceTokens : IFileAction {
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        public IHandleOutput sw = new WriteStdout();
        protected int _countOfMatchesInFile = 0;
        protected int _lineNumber = 0;
        private Command _command;
        private readonly Pickup _pickup;

        public ReplaceTokens() {
            _pickup = new Pickup();
        }

        public virtual string ApplyCommands(ParseCommandFile rf, List<string> inputFilenames) {
            return "";
        }


        protected string ApplySingleCommand(string line, Command command) {
            _command = command;
            _pickup.CollectAllPickupsInLine(line, command);
            line = ReplaceIt(command.SubjectString, line, _pickup.ReplacePickupsWithStoredValue(command.ReplacementString));
            return line;
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
