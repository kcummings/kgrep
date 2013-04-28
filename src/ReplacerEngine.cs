using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using NLog;

namespace kgrep {
    public class ReplacerEngine {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public IHandleOutput sw = new WriteStdout();

        public string ApplyReplacements(string replacementFileName, List<string> inputFilenames) {
            try {
                ReplacementFile rf = new ReplacementFile(replacementFileName);
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
                        return rep.frompattern.Replace(line, rep.topattern);
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
                    if (rep.style == Replacement.Style.Scan)
                        line = ScanForTokens(line, rep.frompattern,rep.ScannerFS);
                    else
                        line = rep.frompattern.Replace(line, rep.topattern);
                }
                logger.Trace("   ApplyReplacementsAll - line  after:{0}",line);
            }
            logger.Debug("ApplyReplacementsAll  after:{0}", line);
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

        public string ScanForTokens(string line, Regex pattern, string FS) {
            List<string> sb = new List<string>();
            Match m = pattern.Match(line);
            logger.Debug("   ScanForTokens - scanpatten:{0} FS:{1}", pattern.ToString(), FS);

            // Only return submatches if found, otherwise return any matches.
            while (m.Success) {
                int[] gnums = pattern.GetGroupNumbers();
                if (gnums.Length > 1) {
                    for (int i = 1; i < gnums.Length; i++) {
                        string token = m.Groups[gnums[i]].ToString();
                        logger.Trace("      ScanForTokens - Found grouped token:{0}", token);
                        sb.Add(token);
                    }
                } else {
                    // Only include the substring that was matched.
                    logger.Trace("      ScanForTokens - Found token:{0}",m.Value);
                    sb.Add(m.Value);
                }
                m = m.NextMatch();
            }
            logger.Debug("   ScanForTokens - returning:{0}",String.Join(FS, sb.ToArray()));
            return String.Join(FS, sb.ToArray());
        }
    }
}
