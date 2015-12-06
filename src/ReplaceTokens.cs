using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace kgrep {

    public class ReplaceTokens : IFileAction {
        public IHandleOutput sw = new WriteStdout();
        private readonly Pickup _pickup;
        private bool verbose = true;

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
            verbose = commandFile.OutputAllLines;

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
            if (verbose)
                line = ReplaceFullLine(command.SubjectRegex, line, _pickup.ReplacePickupsWithStoredValue(command.ReplacementString));
            else {
                line = ReplaceMatched(command.SubjectRegex, line, _pickup.ReplacePickupsWithStoredValue(command.ReplacementString),command.OFS);
            }
            return line;
        }

        private string ReplaceMatched(Regex re, string source, string target, string FS) {
             List<string> sb = new List<string>();
             Match m = re.Match(source);

            // Only return submatches if found, otherwise return any matches.
            while (m.Success) {
                int[] gnums = re.GetGroupNumbers();
                if (gnums.Length > 1) {  // does source contain any captured text?
                    for (int i = 1; i < gnums.Length; i++) {
                        string capturedText = m.Groups[gnums[i]].Value;
                        string newText = re.Replace(capturedText, target);
                        sb.Add(newText);
                    }
                } else {
                    // Only include the substring that was matched.
                    sb.Add(m.Value);
                }
                m = m.NextMatch();
            }
            return String.Join(FS, sb.ToArray()); 
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
