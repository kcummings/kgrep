using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using NLog;

namespace kgrep {
    public class ReplaceTokensInSourceFiles {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public IHandleOutput sw = new WriteStdout();
        public Dictionary<string, string> PickupList = new Dictionary<string, string>();

        public string ApplyCommands(ParseCommandFile rf, List<string> inputFilenames) {
            try {
                string line;
                string alteredLine;

                foreach (string filename in inputFilenames) {
                    logger.Debug("ApplyCommands - Processing input file:{0}", filename);
                    IHandleInput sr = (new ReadFileFactory()).GetSource((filename));
                    while ((line = sr.ReadLine()) != null) {
                        if (rf.ScopeAll)
                            alteredLine = ApplyCommandsAllMatches(line, rf.CommandList);
                        else
                            alteredLine = ApplyCommandsFirstMatch(line, rf.CommandList);
                        if (!String.IsNullOrEmpty(alteredLine)) sw.Write(alteredLine);
                    }
                    sr.Close();
                }
            } catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
            return sw.Close();
        }

        // "scope=First" in effect.
        public string ApplyCommandsFirstMatch(string line, List<Command> commandList) {
            logger.Debug("ApplyCommandsFirstMatch before:{0}", line);
            foreach (Command command in commandList) {
                logger.Trace("   ApplyCommandsFirstMatch - ({0} --> {1})  AnchorString:{2}", command.SubjectString.ToString(), command.ReplacementString, command.AnchorString);
                if (isCandidateForReplacement(line, command)) {
                    if (command.SubjectString.IsMatch(line)) {
                        CollectPickups(line, command);
                        if (command.Style != Command.CommandType.Scan) {
                            if (command.SubjectString.ToString() == "")
                                line = ReplacePickupPlaceholders(command.ReplacementString, PickupList); // ~${name}    force print of placeholders
                            else {
                                line = command.SubjectString.Replace(line, command.ReplacementString);
                                line = ReplacePickupPlaceholders(line, PickupList);
                            }
                            break;
                        }
                    }
                }
            }
            logger.Debug("ApplyCommandsFirstMatch  after:{0}", line);
            return line;
        }

        // "scope=All" in effect.
        public string ApplyCommandsAllMatches(string line, List<Command> commandList) {
            logger.Debug("ApplyCommandsAllMatches before:{0}", line);
            foreach (Command command in commandList) {
                logger.Trace("   ApplyCommandsAllMatches - applying '{0}' --> '{1}'  AnchorString:'{2}'", command.SubjectString.ToString(), command.ReplacementString, command.AnchorString);
                logger.Trace("   ApplyCommandsAllMatches - line before:'{0}'", line);
                if (isCandidateForReplacement(line, command)) {
                    CollectPickups(line, command);
                    if (command.Style != Command.CommandType.Scan) { 
                        if (command.SubjectString.ToString() == "")
                            line = ReplacePickupPlaceholders(command.ReplacementString, PickupList); // ~${name}    force print of placeholders
                        else {
                            line = command.SubjectString.Replace(line, command.ReplacementString);
                            line = ReplacePickupPlaceholders(line, PickupList);
                        }
                    }
                }
                logger.Trace("   ApplyCommandsAllMatches - line  after:'{0}'",line);
            }
            logger.Debug("ApplyCommandsAllMatches  after:'{0}'", line);
            return line;
        }

        private void CollectPickups(string line, Command command) {
            if (command.PickupCount > 0) {
                GroupCollection groups = command.SubjectString.Match(line).Groups;

                foreach (string groupName in command.SubjectString.GetGroupNames()) {
                    if (PickupList.ContainsKey(groupName))
                        PickupList[groupName] = groups[groupName].Value;
                    else
                        PickupList.Add(groupName, groups[groupName].Value);
                }
            }
        }


        private string ReplacePickupPlaceholders(string line, Dictionary<string, string> pickupList) {
            Regex re = new Regex(@"\$\{(.+?)\}",RegexOptions.Compiled);
            MatchCollection mc = re.Matches(line);
            string PickupValue;

            foreach (Match m in mc) {
                PickupValue = m.Groups[1].Value;
                if (pickupList.ContainsKey(PickupValue))
                    line = line.Replace("${" + PickupValue + "}", pickupList[PickupValue]);
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
