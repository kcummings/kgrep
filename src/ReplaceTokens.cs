using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace kgrep {

    public class ReplaceTokens : IFileAction {
        public IHandleOutput sw = new WriteStdout();
        private int _countOfMatchesInFile = 0;
        private int _lineNumber = 0;
        private Command _command;
        private readonly Pickup _pickup;

        public ReplaceTokens() {
            _pickup = new Pickup();
        }

        public virtual string ApplyCommandsToInputFileList(ParseCommandFile rf, List<string> inputFilenames) {
            try {
                foreach (string filename in inputFilenames) {
                    ApplyAllCommandsToFile(rf, filename);
                }
            } catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
            return sw.Close();
        }

        private void ApplyAllCommandsToFile(ParseCommandFile commandFile, string filename) {
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
                if (command.IsReplaceFirstMatchCommand) {
                   line = ApplyCommandsFirstMatch(line, command);
                   if (command.SubjectRegex.IsMatch(argline)) break;
                }
                else 
                   line = ApplyCommandsAllMatches(line, command);
            }
            return line;
        }

        public string ApplyCommandsAllMatches(string line, Command command) {
            if (command.Style == Command.CommandType.Pickup)
                _pickup.CollectAllPickupsInLine(line, command);
            else
                line = ApplySingleCommand(line, command);
            return line;
        }

        public string ApplyCommandsFirstMatch(string line, Command command) {
            if ( ! command.SubjectRegex.IsMatch(line))
                return line;

            if (command.Style == Command.CommandType.Normal || command.Style == Command.CommandType.Anchored) {
                line = ApplySingleCommand(line, command);
            }
            _pickup.CollectAllPickupsInLine(line, command);
            return line;
        }

        private string ApplySingleCommand(string line, Command command) {
            _command = command;
            _pickup.CollectAllPickupsInLine(line, command);
            line = ReplaceIt(command.SubjectRegex, line, _pickup.ReplacePickupsWithStoredValue(command.ReplacementString));
            return line;
        }

        private string ReplaceIt(Regex re, string source, string target) {
            int count = re.Matches(source).Count;
            if (count>0) {
                _countOfMatchesInFile += count;
                return re.Replace(source, target);
            }
            return source;
        }

        private bool isCandidateForReplacement(string line, Command command) {
            if (command.AnchorString.Length == 0)   // no anchor present
                return true;

            if (Regex.IsMatch(line, command.AnchorString))
                return true;
            return false;
        }
    }
}
