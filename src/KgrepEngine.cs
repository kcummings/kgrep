using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace kgrep {
    public class KgrepEngine {

        public string ApplyReplacements(string line, List<Replacement> repList) {
            if (repList.Count == 0)
                return line;

            foreach (Replacement rep in repList) {
                if (isCandidateForReplacement(line, rep)) {
                    if (rep.multiple) break;
                  //  Console.WriteLine("{0} {1} {2}", rep.criteria, rep.fromPattern, rep.topattern);
                    line = rep.fromPattern.Replace(line, rep.topattern);
                }
            }
            return line;
        }

        private bool isCandidateForReplacement(string line, Replacement rep) {
            // Has a matching anchor?
            if (rep.criteria.Length > 0) {
                if (Regex.IsMatch(line, rep.criteria))
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
    }
}
