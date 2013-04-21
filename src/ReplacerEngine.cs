using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace kgrep {
    public class ReplacerEngine {

        public IHandleOutput sw = new WriteStdout();

        public string ApplyReplacements(string replacementFileName, List<string> inputFilenames) {
            try {
                ReplacementFile rf = new ReplacementFile(replacementFileName);
                string line;
                string alteredLine;

                foreach (string filename in inputFilenames) {
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
            foreach (Replacement rep in repList) {
                if (isCandidateForReplacement(line, rep)) {
                    if (rep.style == Replacement.Style.Scan)
                        line = ScanForTokens(line, rep.frompattern);
                    else
                        line = rep.frompattern.Replace(line, rep.topattern);
                }
            }
            return line;
        }

        private bool isCandidateForReplacement(string line, Replacement rep) {
            // Has a matching anchor?
            if (rep.anchor.Length > 0) {
                if (Regex.IsMatch(line, rep.anchor))
                    return true;
                return false;
            }
            return true;
        }

        public string ScanForTokens(string line, Regex pattern) {
            StringBuilder sb = new StringBuilder();
            Match m = pattern.Match(line);

            // Only return submatches if found, otherwise return any matches.
            while (m.Success) {
                int[] gnums = pattern.GetGroupNumbers();
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
