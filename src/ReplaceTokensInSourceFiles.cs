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
        public Dictionary<string, string> NamedGroupValues = new Dictionary<string, string>();

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
        public string ApplyReplacementsFirst(string line, List<Replacement> repList) {
            logger.Debug("ApplyReplacementsFirst before:{0}", line);
            foreach (Replacement rep in repList) {
                logger.Trace("   ApplyReplacementsFirst - ({0} --> {1})  anchor:{2}", rep.frompattern.ToString(), rep.topattern, rep.anchor);
                if (isCandidateForReplacement(line, rep)) {
                    if (rep.frompattern.IsMatch(line)) {
                        CollectNamedGroups(line, rep);
                        if (rep.style != Replacement.Style.Scan) {
                            line = rep.frompattern.Replace(line, rep.topattern);
                            line = ReplaceRegexPlaceholdersIfPresent(line, NamedGroupValues);
                            break;
                        }
                    }
                }
            }
            logger.Debug("ApplyReplacementsFirst  after:{0}", line);
            return line;
        }

        // "scope=All" in effect.
        public string ApplyReplacementsAll(string line, List<Replacement> repList) {
            logger.Debug("ApplyReplacementsAll before:{0}", line);
            foreach (Replacement rep in repList) {
                logger.Trace("   ApplyReplacementsAll - applying ({0} --> {1})  anchor:{2}", rep.frompattern.ToString(), rep.topattern, rep.anchor);
                logger.Trace("   ApplyReplacementsAll - line before:{0}", line);
                if (isCandidateForReplacement(line, rep)) {
                    CollectNamedGroups(line, rep);
                    if (rep.style != Replacement.Style.Scan) { 
                        line = rep.frompattern.Replace(line, rep.topattern);
                        line = ReplaceRegexPlaceholdersIfPresent(line, NamedGroupValues);
                    }
                }
                logger.Trace("   ApplyReplacementsAll - line  after:{0}",line);
            }
            logger.Debug("ApplyReplacementsAll  after:{0}", line);
            return line;
        }

        private void CollectNamedGroups(string line, Replacement rep) {
            if (rep.NamedGroupCount > 0) {
                GroupCollection groups = rep.frompattern.Match(line).Groups;

                foreach (string groupName in rep.frompattern.GetGroupNames()) {
                    if (NamedGroupValues.ContainsKey(groupName))
                        NamedGroupValues[groupName] = groups[groupName].Value;
                    else
                        NamedGroupValues.Add(groupName, groups[groupName].Value);
                }
            }
        }


        public string ReplaceRegexPlaceholdersIfPresent(string line, Dictionary<string, string> groupNameValues) {
            Regex re = new Regex(@"\$\{(.+?)\}",RegexOptions.Compiled);
            MatchCollection mc = re.Matches(line);

            foreach (Match m in mc) {
                if (groupNameValues.ContainsKey(m.Groups[1].Value))
                    line = line.Replace("${" + m.Groups[1].Value+"}", groupNameValues[m.Groups[1].Value]);
            }
            return line;
        }

        private bool isCandidateForReplacement(string line, Replacement rep) {
            // Has a matching anchor?
            if (rep.anchor.Length > 0) {
                if (Regex.IsMatch(line, rep.anchor))
                    return true;
                logger.Trace("   is not a replacement candidate");
                return false;
            }
            return true;
        }
    }
}
