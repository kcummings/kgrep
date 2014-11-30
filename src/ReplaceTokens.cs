﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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

        public virtual string ApplyCommandsToInputFiles(ParseCommandFile rf, List<string> inputFilenames) {
            try {
                string line;
                string alteredLine;
                Stopwatch timer;

                foreach (string filename in inputFilenames) {
                    timer = Stopwatch.StartNew();
                    logger.Debug("Replace Matches - Processing input file:{0}", filename);
                    IHandleInput sr = (new ReadFileFactory()).GetSource((filename));
                    _lineNumber = 0;
                    _countOfMatchesInFile = 0;
                    while ((line = sr.ReadLine()) != null) {
                        _lineNumber++;
                        // TODO: Print Info after every 5000 lines, i.e. I'm still processing
                        alteredLine = ApplyCommandsToLine(line, rf.CommandList);
                        if (!String.IsNullOrEmpty(alteredLine)) sw.Write(alteredLine);
                    }
                    sr.Close();
                    timer.Stop();
                    logger.Info("File {0} found {1} matches on {2} input lines [{3:d} ms]"
                                , filename, _countOfMatchesInFile, _lineNumber, timer.ElapsedMilliseconds);
                }
            } catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
            return sw.Close();
        }

        public string ApplyCommandsToLine(string argline, List<Command> commandList) {
            string line = argline;
            foreach (Command command in commandList) {
                if (command.IsReplaceFirstMatchCommand) {
                   line = (ApplyCommandsFirstMatch(line, command));
                   if (command.SubjectString.IsMatch(argline)) break;
                }
                else 
                   line = ApplyCommandsAllMatches(line, command);
            }
            return line;
        }

        public string ApplyCommandsAllMatches(string line, Command command) {
            if (isCandidateForReplacement(line, command)) {
                if (command.Style == Command.CommandType.Pickup)
                    _pickup.CollectAllPickupsInLine(line, command);
                else
                    line = ApplySingleCommand(line, command);
            }
            return line;
        }

        public string ApplyCommandsFirstMatch(string line, Command command) {
            if (isCandidateForReplacement(line, command)) {
                if (command.SubjectString.IsMatch(line)) {
                    if (command.Style == Command.CommandType.Normal || command.Style == Command.CommandType.Anchored) {
                        _pickup.CollectAllPickupsInLine(line, command);
                        line = command.SubjectString.Replace(line, _pickup.ReplacePickupsWithStoredValue(command.ReplacementString)); //,1);
                    }
                    _pickup.CollectAllPickupsInLine(line, command);
                }
            }
            return line;
        }

        private string ApplySingleCommand(string line, Command command) {
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

        private bool isCandidateForReplacement(string line, Command command) {
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
