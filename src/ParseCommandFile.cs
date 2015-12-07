using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace kgrep
{
    public class ParseCommandFile
    {
        public List<Command> CommandList = new List<Command>();
        private String _comment = "#";
        private String _delim = "~";
        private IHandleInput sr;
        public string OFS = "";    // Output Field Seperator, like AWK's
        public bool ReplaceOnEntireLine = true;
        public int MaxReplacements = 9999;
    
        // Kgrep is only in one state or mode.
        // The mode is determined by the types and sequence of commands. 
        public enum RunningAs {
            Scanner,
            ReplaceAll
        }

        public RunningAs kgrepMode;

        public ParseCommandFile(String filename) {
            if (filename == null) return;
            sr = (new ReadFileFactory()).GetSource(filename);

            CommandList = GetReplacementList();
        }

        public List<Command> GetReplacementList() {
            // TODO: Use Strategy pattern to only call regex.Replace when regex present, otherwise call String.Replace.
            kgrepMode = RunningAs.ReplaceAll;
            String line;
            while ((line = sr.ReadLine()) != null) {
                line = line.Trim();

                // Remove comment lines.
                if (line == String.Empty || line.StartsWith(_comment)) {    
                    continue;
                }

                // Remove any trailing comments.
                int i = line.IndexOf(_comment);
                if (i >= 0)
                    line = line.Remove(i);

                if (line.ToLower().StartsWith("comment="))
                    _comment = GetOption(line, "comment");
                else if (line.ToLower().StartsWith("delim="))
                    _delim = GetOption(line, "delim");
                else if (line.ToLower().StartsWith("ofs=")) {
                    OFS = GetOption(line, "OFS");
                    OFS = OFS.Replace("\\n", "\n"); // interpret \n on command line as newline
                    OFS = OFS.Replace("\\t", "\t"); // interpret \t on command line as tab
                } else {
                    Command command = new Command(line, _delim);
                    if (command.IsValid()) {
                        CommandList.Add(command);
                    }
                }
            }
            sr.Close();
            if (IsScanner()) kgrepMode = RunningAs.Scanner;
            return CommandList;
        }

        // TODO: Let GetOption accept blanks around =
        // Get the provided value for the given Control Option.
        // allow optional enclosing in quotes
        private string GetOption(string line, string type) {
            Match m = Regex.Match(line, type+"=\"(.*)\"", RegexOptions.IgnoreCase);  
            if (!m.Success)
               m = Regex.Match(line, type+"='(.*)'", RegexOptions.IgnoreCase);  
            if (!m.Success)
                m = Regex.Match(line, type+"=(.*)", RegexOptions.IgnoreCase);
            return m.Groups[1].Value;
        }

        public bool IsScanner() {
            foreach (Command cmd in CommandList)
                if (cmd.IsNormal)
                    return false;
            return true;
        }
    }
}
