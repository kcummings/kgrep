using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NLog;

namespace kgrep
{
    public class ParseCommandFile
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public List<Command> CommandList = new List<Command>();
        private bool _scopeAll = true;
        public bool ScopeAll { get { return _scopeAll; } }
        private String _comment = "#";
        private String _delim = "~";
        private IHandleInput sr;
        public string ScannerFS = "\n";
        // TODO: Let UseAsScanner be private or remove
        public bool UseAsScanner = false;
        public enum RunMode {
            Scanner,
            ReplaceFirst,
            ReplaceAll
        }

        public RunMode RunAs;

        public ParseCommandFile(String filename) {
            if (filename == null) return;
            logger.Debug("Start reading commandfile:{0}",filename);
            sr = (new ReadFileFactory()).GetSource(filename);

            DateTime startParse = DateTime.Now;
            CommandList = GetReplacementList();
            TimeSpan ts = DateTime.Now - startParse;
            logger.Info("Parsed {0} commands in command file. [{1:d} ms]", CommandList.Count, ts.Milliseconds);
        }

        public List<Command> GetReplacementList() {
            List<Command> commandList = new List<Command>();
            // TODO: Replace RunMode enum with a class so can write: if (runningas.Scanner)
            // TODO: Use Strategy pattern to only call regex.Replace when regex present, otherwise call String.Replace.
            RunAs = RunMode.ReplaceAll;
            UseAsScanner = true;
            String line;
            while ((line = sr.ReadLine()) != null) {
                logger.Trace("   command source line:{0}",line);
                line = line.Trim();

                // Remove comment lines.
                if (line == String.Empty || line.StartsWith(_comment)) {    
                    continue;
                }

                // Remove any trailing comments.
                int i = line.IndexOf(_comment);
                if (i >= 0)
                    line = line.Remove(i);
              //  if (Regex.Match(line, "[ ~]+").Success) continue;

                if (line.ToLower().StartsWith("comment="))
                    _comment = GetOption(line, "comment");
                else if (line.ToLower().StartsWith("delim="))
                    _delim = GetOption(line, "delim");
                else if (line.ToLower().StartsWith("scope=first"))  // Once true, it's true for the remaining commands.
                    RunAs = RunMode.ReplaceFirst;
                else if (line.ToLower().StartsWith("scope=all"))
                    RunAs = RunMode.ReplaceAll;
                else if (line.ToLower().StartsWith("scannerfs="))
                    ScannerFS = GetOption(line, "FS");
                else {
                    // TODO: Add tests for mixed settings, e.g. "scope=first;a;scope=all;a~b
                    Command command = null;
                    String[] parts = line.Split(_delim.ToCharArray(), 4);
                    if (parts.Length == 1) {
                        command = new Command(parts[0]) { ScannerFS = ScannerFS };
                    }
                    if (parts.Length == 2) {
                        command = new Command(parts[0], parts[1]);
                        UseAsScanner = false;
                    }
                    if (parts.Length == 3) {
                        command = new Command(parts[0], parts[1], parts[2]);
                        UseAsScanner = false;
                    }
                    if (IsValidCommand(command))
                        commandList.Add(command);
                }
            }
            sr.Close();
            if (UseAsScanner) RunAs = RunMode.Scanner;
            return commandList;
        }

        private bool IsValidCommand(Command command) {
            if (command == null) return false;

            if (string.IsNullOrEmpty(command.SubjectString.ToString())
                && string.IsNullOrEmpty(command.ReplacementString))
                return false;
            return true;
        }

        // TODO: Let GetOption accept blanks around =
        // Get the provided value for the given Control Option.
        // allow optional enclosing in quotes
        private string GetOption(string line, string type) {
            Match m = Regex.Match(line, type+"=\"(.+)\"", RegexOptions.IgnoreCase);  
            if (!m.Success)
                m = Regex.Match(line, type+"=(.+)", RegexOptions.IgnoreCase);
            return m.Groups[1].Value;
        }
    }
}
