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
                    line = ApplySingleCommand(line, command);
                }
            }
            return line;
        }

        public string ApplySingleCommand(string line, Command command) {
            _pickup.CollectAllPickupsInLine(line, command);
            if (command.CommandIs == Command.CommandType.isAnchoredTemplate || command.CommandIs == Command.CommandType.isTemplate)
                line = ReplaceMatched(command.SubjectRegex, line, _pickup.ReplacePickupsWithStoredValue(command.ReplacementString));            
            else if (command.CommandIs == Command.CommandType.isFindAndPrint) {
                ScanForTokens(line, command.SubjectRegex, command.OFS);
            }
            else 
                line = ReplaceFullLine(command.SubjectRegex, line, _pickup.ReplacePickupsWithStoredValue(command.ReplacementString));
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
            if (command.CommandIs != Command.CommandType.isAnchoredReplace && command.CommandIs != Command.CommandType.isAnchoredReplace)
                return true;
            if (command.CommandIs == Command.CommandType.isPickupOnly)
                return true;

            if (Regex.IsMatch(line, command.AnchorString))
                return true;
            return false;
        }

        public string ScanForTokens(string line, Regex pattern, string FS) {
            List<string> sb = new List<string>();
            Match m = pattern.Match(line);

            // Only return submatches if found, otherwise return any matches.
            while (m.Success) {
                int[] gnums = pattern.GetGroupNumbers();
                if (gnums.Length > 1) {
                    for (int i = 1; i < gnums.Length; i++) {
                        string token = m.Groups[gnums[i]].ToString();
                        sb.Add(token);
                    }
                } else {
                    // Only include the substring that was matched.
                    sb.Add(m.Value);
                }
                m = m.NextMatch();
            }
            return String.Join(FS, sb.ToArray());
        }
    }
}
