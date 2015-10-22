using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace kgrep {

    public class ReplaceTokens : IFileAction {
        public IHandleOutput sw = new WriteStdout();
        private readonly Pickup _pickup;

        public ReplaceTokens() {
            _pickup = new Pickup();
        }

        public virtual string ApplyCommandsToInputFileList(ParseCommandFile rf, List<string> inputFilenames) {
            try {
                foreach (string filename in inputFilenames) {
                    ApplyCommandsToFile(rf, filename);
                }
            } catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
            return sw.Close();
        }

        private void ApplyCommandsToFile(ParseCommandFile commandFile, string filename) {
            IHandleInput sr = (new ReadFileFactory()).GetSource((filename));
            string line;
            while ((line = sr.ReadLine()) != null) {
                string alteredLine = ApplyCommandsToLine(line, commandFile.CommandList);
                if (!String.IsNullOrEmpty(alteredLine)) sw.Write(alteredLine);
            }
            sr.Close();
        }

        public string ApplyCommandsToLine(string argline, List<Command> commandList) {
            string line = argline;
            foreach (Command command in commandList) {
                if ( ! isCandidateForReplacement(line, command)) break;
                line = ApplyCommandsAllMatches(line, command);
            }
            return line;
        }

        public string ApplyCommandsAllMatches(string line, Command command) {
            if (command.IsPickup)
                _pickup.CollectAllPickupsInLine(line, command);
            else
                line = ApplySingleCommand(line, command);
            return line;
        }

        private string ApplySingleCommand(string line, Command command) {
            _pickup.CollectAllPickupsInLine(line, command);
            line = ReplaceIt(command.SubjectRegex, line, _pickup.ReplacePickupsWithStoredValue(command.ReplacementString));
            return line;
        }

        private string ReplaceIt(Regex re, string source, string target) {
            int count = re.Matches(source).Count;
            if (count>0) {
                return re.Replace(source, target);
            }
            return source;
        }

        private bool isCandidateForReplacement(string line, Command command) {
            if (!command.IsAnchored)   
                return true;

            if (Regex.IsMatch(line, command.AnchorString))
                return true;
            return false;
        }
    }
}
