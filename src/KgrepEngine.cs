using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace kgrep {
    public class KgrepEngine {

        private bool _stopReplacements = false;

        // "ReplacementMode=First" in effect.
        public string ApplyReplacementsFirst(string line, List<Replacement> repList) {
            if (_stopReplacements) return line;
            if (repList.Count == 0)
                return line;

            foreach (Replacement rep in repList) {
                if (isCandidateForReplacement(line, rep)) {
                    if (rep.frompattern.IsMatch(line)) {
                        _stopReplacements = true;
                        line = rep.frompattern.Replace(line, rep.topattern);
                        break;
                    }
                }
            }
            return line;
        }

        // "ReplacementMode=All" in effect.
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

        public string ScanForTokens(string line, string tokenpattern, string delim) {
            StringBuilder sb = new StringBuilder();
            Regex re = new Regex(tokenpattern);
            Match m = re.Match(line);

            // Only return submatches if found, otherwise return any matches.
            while (m.Success) {
                int[] gnums = re.GetGroupNumbers();
                if (gnums.Length > 1) {  
                    for (int i = 1; i < gnums.Length; i++) {
                        sb.Append((m.Groups[gnums[i]]));
                        sb.Append(delim);
                    }
                } else {
                    // Only print the substring that was matched.
                    sb.Append((m.Value));
                    sb.Append(delim);
                }
                m = m.NextMatch();
            }
            return sb.ToString();
        }

        // kgrep being used as a scanner/grep.
        public void ScanAndPrintTokens(string matchpattern, List<string> filenames) {
            try {
                KgrepEngine engine = new KgrepEngine();
                foreach (string filename in filenames) {
                    HandleInput sr = new HandleInput(filename);
                    string line;
                    while ((line = sr.ReadLine()) != null) {
                        string alteredLine = engine.ScanForTokens(line, matchpattern, "\n");
                        if (!String.IsNullOrEmpty(alteredLine)) Console.WriteLine(alteredLine);
                    }
                    sr.Close();
                }
            } catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
        }

        // Kgrep being used as sed.
        public void SearchAndReplaceTokens(string searchPattern, string toPattern, List<String> filenames) {
            List<Replacement> reps = new List<Replacement>();
            Replacement rep = new Replacement("", searchPattern, toPattern);
            reps.Add(rep);
            SearchAndReplaceTokens(reps, filenames, true);
        }

        public void SearchAndReplaceTokens(ReplacementFile rf, List<string> filenames) {
            List<Replacement> repList = new List<Replacement>();
            repList = rf.GetReplacements();
            SearchAndReplaceTokens(repList, filenames, rf.isReplaceAll);
        }

        public void SearchAndReplaceTokens(List<Replacement> repList, List<string> filenames, bool isReplaceAll) {
            if (repList.Count == 0)
                return;

            try {
                KgrepEngine engine = new KgrepEngine();
                string line;
                string alteredLine;
                foreach (string filename in filenames) {
                    HandleInput sr = new HandleInput(filename);
                    while ((line = sr.ReadLine()) != null) {
                        if (isReplaceAll)
                            alteredLine = engine.ApplyReplacementsAll(line, repList);
                        else
                            alteredLine = engine.ApplyReplacementsFirst(line, repList);
                        if (!String.IsNullOrEmpty(alteredLine)) Console.WriteLine(alteredLine);
                    }
                    sr.Close();
                }
            } catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
        }
    }
}
