using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace kgrep {
    public class KgrepEngine {

        public IHandleOutput sw = new WriteStdout();

        // kgrep being used as a scanner/grep.
        public string ScanAndPrintTokens(string matchpattern, List<string> filenames) {
            try {
                foreach (string filename in filenames) {
                    IHandleInput sr = (new ReadFileFactory()).GetSource(filename);
                    string line;
                    while ((line = sr.ReadLine()) != null) {
                        string alteredLine = ScanForTokens(line, matchpattern);
                        if (!String.IsNullOrEmpty(alteredLine)) sw.Write(alteredLine);
                    }
                    sr.Close();
                }
            } catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
            return sw.Close();
        }

        public string SearchAndReplaceTokens(string replacementFileName, List<string> inputFilenames) {
            try {
                ReplacementFile rf = new ReplacementFile(replacementFileName);
                string line;
                string alteredLine;
                List<Replacement> repList = rf.GetReplacements();

                foreach (string filename in inputFilenames) {
                    IHandleInput sr = (new ReadFileFactory()).GetSource((filename));
                    while ((line = sr.ReadLine()) != null) {
                        if (rf.ScopeAll)
                            alteredLine = ApplyReplacementsAll(line, repList);
                        else
                            alteredLine = ApplyReplacementsFirst(line, repList);
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
            if (repList.Count == 0)
                return line;

            foreach (Replacement rep in repList) {
                if (isCandidateForReplacement(line, rep)) {
                    if (rep.frompattern.IsMatch(line)) {
                        return rep.frompattern.Replace(line, rep.topattern);
                    }
                }
            }
            return line;
        }

        // "scope=All" in effect.
        public string ApplyReplacementsAll(string line, List<Replacement> repList) {
            if (repList.Count == 0)
                return line;

            foreach (Replacement rep in repList) {
                if (isCandidateForReplacement(line, rep)) {
                    line = rep.frompattern.Replace(line, rep.topattern);
                }
            }
            return line;
        }

        private bool isCandidateForReplacement(string line, Replacement rep) {
            // Has a matching anchor?
            if (rep.Criteria.Length > 0) {
                if (Regex.IsMatch(line, rep.Criteria))
                    return true;
                return false;
            }
            return true;
        }

        public string ScanForTokens(string line, string tokenpattern) {
            StringBuilder sb = new StringBuilder();
            Regex re = new Regex(tokenpattern);
            Match m = re.Match(line);

            // Only return submatches if found, otherwise return any matches.
            while (m.Success) {
                int[] gnums = re.GetGroupNumbers();
                if (gnums.Length > 1) {
                    for (int i = 1; i < gnums.Length; i++) {
                        sb.Append(m.Groups[gnums[i]].ToString());
                        sb.Append("\n");
                    }
                } else {
                    // Only include the substring that was matched.
                    sb.Append(m.Value);
                    sb.Append("\n");
                }
                m = m.NextMatch();
            }
            return sb.ToString();
        }
    }
}
