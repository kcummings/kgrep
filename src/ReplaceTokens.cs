using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace kgrep {

    public class ReplaceTokens : IFileAction {
        public IHandleOutput sw = new WriteStdout();
        private readonly Pickup _pickup;
        private int _maxReplacements;
        private string OFS;

        public ReplaceTokens() {
            _pickup = new Pickup();
        }

        public virtual string ApplyCommandsToInputFileList(ParseCommandFile rf, List<string> inputFilenames) {
            try {
                OFS = rf.OFS;
                foreach (string filename in inputFilenames) {
                    ApplyCommandsToFile(rf, filename);
                }
            } catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
            return sw.Close();
        }

        public void ApplyCommandsToFile(ParseCommandFile commandFile, string filename) {
            IHandleInput sr = (new ReadFileFactory()).GetSource((filename));
            _maxReplacements = commandFile.MaxReplacements;
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
                if (command.SubjectRegex.IsMatch(line)) {  // only decrement _maxReplacements if matches, don't just count commands read
                    if (_maxReplacements-- <= 0) break;
                    line = ApplyCommandsAllMatches(line, command);
                }
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
            if (command.IsUsingTargetTemplate)
                line = ReplaceMatched(command.SubjectRegex, line, _pickup.ReplacePickupsWithStoredValue(command.ReplacementString));            
            else {
                line = ReplaceFullLine(command.SubjectRegex, line, _pickup.ReplacePickupsWithStoredValue(command.ReplacementString));
            }
            return line;
        }

        private string ReplaceMatched(Regex re, string source, string target) {
            string line = "";

            Match m = re.Match(source);
            if (m.Success) {
                line = re.Replace(m.Value, target);
            }
            return line;
        }

        private string ReplaceFullLine(Regex re, string source, string target) {
            return re.Replace(source, target);
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
