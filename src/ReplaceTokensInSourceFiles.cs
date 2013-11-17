using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using NLog;

namespace kgrep {

    // TODO: This class has more than one responsibility. 
    public class ReplaceTokensInSourceFiles {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public IHandleOutput sw = new WriteStdout();
        public Dictionary<string, string> PickupList = new Dictionary<string, string>();
        private int _countOfReplacementsInFile = 0;
        private int _lineNumber = 0;

        public string ApplyCommands(ParseCommandFile rf, List<string> inputFilenames) {
            try {
                string line;
                string alteredLine;

                foreach (string filename in inputFilenames) {
                    if (rf.ScopeAll)
                        logger.Debug("ApplyCommandsAllMatches - Processing input file:{0}", filename);
                    else
                        logger.Debug("ApplyCommandsFirstMatch - Processing input file:{0}", filename);
                    IHandleInput sr = (new ReadFileFactory()).GetSource((filename));
                    _lineNumber = 0;
                    _countOfReplacementsInFile = 0;
                    while ((line = sr.ReadLine()) != null) {
                        _lineNumber++;
                        if (rf.ScopeAll)
                            alteredLine = ApplyCommandsAllMatches(line, rf.CommandList);
                        else
                            alteredLine = ApplyCommandsFirstMatch(line, rf.CommandList);
                        if (!String.IsNullOrEmpty(alteredLine)) sw.Write(alteredLine);
                    }
                    sr.Close();
                    logger.Info("File {0} found {1} matches on {2} input lines", filename, _countOfReplacementsInFile, _lineNumber);
                }
            } catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
           return sw.Close();
        }

        // "scope=First" in effect.
        public string ApplyCommandsFirstMatch(string line, List<Command> commandList) {
            logger.Trace("ApplyCommandsFirstMatch before:{0}", line);
            foreach (Command command in commandList) {
                logger.Trace("   ApplyCommandsFirstMatch - ({0} --> {1})  AnchorString:{2}", command.SubjectString.ToString(), command.ReplacementString, command.AnchorString);
                if (isCandidateForReplacement(line, command)) {
                    if (command.SubjectString.IsMatch(line)) {
                        CollectPickupValues(line, command);
                        if (command.Style != Command.CommandType.Pickup) {
                            line = ApplySingleCommand(line, command);
                            break;
                        }
                    }
                }
            }
            logger.Trace("ApplyCommandsFirstMatch  after:{0}", line);
            return line;
        }

        // "scope=All" in effect.
        public string ApplyCommandsAllMatches(string line, List<Command> commandList) {
            logger.Trace("ApplyCommandsAllMatches before:{0}", line);
            foreach (Command command in commandList) {
                logger.Trace("   ApplyCommandsAllMatches - applying '{0}' --> '{1}'  AnchorString:'{2}'", command.SubjectString.ToString(), command.ReplacementString, command.AnchorString);
                logger.Trace("   ApplyCommandsAllMatches - line before:'{0}'", line);
                if (isCandidateForReplacement(line, command)) {
                    CollectPickupValues(line, command);
                    if (command.Style != Command.CommandType.Pickup) {
                        line = ApplySingleCommand(line, command);
                    }
                }
                logger.Trace("   ApplyCommandsAllMatches - line  after:'{0}'",line);
            }
            logger.Trace("ApplyCommandsAllMatches  after:'{0}'", line);
            return line;
        }

        private string ApplySingleCommand(string line, Command command) {
            if (command.Style == Command.CommandType.Print)
                line = ReplacePickupsWithStoredValue(command.ReplacementString);   
            else {
                if (command.CountOfPickupsInSubjectString > 0) {
                    string subjectStringWithReplacedPickups = ReplacePickupsWithStoredValue(command.SubjectString.ToString());
                    Regex subjectString = new Regex(subjectStringWithReplacedPickups);
                    line = ReplaceIt(subjectString, line, command.ReplacementString);
                }
                else {
                    line = ReplaceIt(command.SubjectString, line, command.ReplacementString);
                }
                line = ReplacePickupsWithStoredValue(line);
            }
            return line;
        }

        private string ReplaceIt(Regex re, string source, string target) {
            int count = re.Matches(source).Count;
            if (count>0) {
                logger.Debug("   At line {0} found {1} occurances of '{2}' in '{3}'", _lineNumber, count, re.ToString(), source);
                _countOfReplacementsInFile += count;
                return re.Replace(source, target);
            }
            return source;
        }

        // Values are in Named and unnamed Captures are only in SubjectString.
        // e.g. named capture syntax: (?<digit>[0-9]+)  yeilds pickup name ${digit} 
        //    unnamed capture syntax: ([0-9]+)    yeilds pickup name ${1}
        public void CollectPickupValues(string line, Command command) {
            if (command.CountOfCapturesInSubjectString > 0) {
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

        private string ReplacePickupsWithStoredValue(string line) {
            Regex re = new Regex(@"\$\{(.+?)\}",RegexOptions.Compiled);
            MatchCollection mc = re.Matches(line);
            string PickupValue;

            foreach (Match m in mc) {
                PickupValue = m.Groups[1].Value;
                if (PickupList.ContainsKey(PickupValue))
                    line = line.Replace("${" + PickupValue + "}", PickupList[PickupValue]);
            }
            return line;
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
