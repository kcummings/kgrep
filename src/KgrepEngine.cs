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
    }
}
