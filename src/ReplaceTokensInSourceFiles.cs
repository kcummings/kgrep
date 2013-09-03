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

        public string ApplyReplacements(ParseReplacementFile rf, List<string> inputFilenames) {
            try {
                string line;
                string alteredLine;

                foreach (string filename in inputFilenames) {
                    logger.Debug("ApplyReplacements - Processing input file:{0}", filename);
                    IHandleInput sr = (new ReadFileFactory()).GetSource((filename));
                    while ((line = sr.ReadLine()) != null) {
                        if (rf.ScopeAll)
                            alteredLine = ApplyReplacementsAll(line, rf.ReplacementList);
                        else
                            alteredLine = ApplyReplacementsFirst(line, rf.ReplacementList);
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
        public string ApplyReplacementsFirst(string line, List<Replacement> replacementList) {
            logger.Debug("ApplyReplacementsFirst before:{0}", line);
            foreach (Replacement replacement in replacementList) {
                logger.Trace("   ApplyReplacementsFirst - ({0} --> {1})  AnchorPattern:{2}", replacement.FromPattern.ToString(), replacement.ToPattern, replacement.AnchorPattern);
                if (isCandidateForReplacement(line, replacement)) {
                    if (replacement.FromPattern.IsMatch(line)) {
                        CollectPickups(line, replacement);
                        if (replacement.Style != Replacement.ReplacementType.Scan) {
                            if (replacement.FromPattern.ToString() == "")
                                line = ReplacePickupPlaceholders(replacement.ToPattern, PickupList); // ~${name}    force print of placeholders
                            else {
                                line = replacement.FromPattern.Replace(line, replacement.ToPattern);
                                line = ReplacePickupPlaceholders(line, PickupList);
                            }
                            break;
                        }
                    }
                }
            }
            logger.Debug("ApplyReplacementsFirst  after:{0}", line);
            return line;
        }

        // "scope=All" in effect.
        public string ApplyReplacementsAll(string line, List<Replacement> replacementList) {
            logger.Debug("ApplyReplacementsAll before:{0}", line);
            foreach (Replacement replacement in replacementList) {
                logger.Trace("   ApplyReplacementsAll - applying '{0}' --> '{1}'  AnchorPattern:'{2}'", replacement.FromPattern.ToString(), replacement.ToPattern, replacement.AnchorPattern);
                logger.Trace("   ApplyReplacementsAll - line before:'{0}'", line);
                if (isCandidateForReplacement(line, replacement)) {
                    CollectPickups(line, replacement);
                    if (replacement.Style != Replacement.ReplacementType.Scan) { 
                        if (replacement.FromPattern.ToString() == "")
                            line = ReplacePickupPlaceholders(replacement.ToPattern, PickupList); // ~${name}    force print of placeholders
                        else {
                            line = replacement.FromPattern.Replace(line, replacement.ToPattern);
                            line = ReplacePickupPlaceholders(line, PickupList);
                        }
                    }
                }
                logger.Trace("   ApplyReplacementsAll - line  after:'{0}'",line);
            }
            logger.Debug("ApplyReplacementsAll  after:'{0}'", line);
            return line;
        }

        private void CollectPickups(string line, Replacement replacement) {
            if (replacement.PickupCount > 0) {
                GroupCollection groups = replacement.FromPattern.Match(line).Groups;

                foreach (string groupName in replacement.FromPattern.GetGroupNames()) {
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

        private bool isCandidateForReplacement(string line, Replacement replacement) {
            // Has a matching AnchorPattern?
            if (replacement.AnchorPattern.Length > 0) {
                if (Regex.IsMatch(line, replacement.AnchorPattern))
                    return true;
                logger.Trace("   is not a replacement candidate");
                return false;
            }
            return true;
        }
    }
}
