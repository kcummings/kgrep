using System;
using System.Text.RegularExpressions;

namespace kgrep {
    public class ShorthandRegex {
        public ShorthandRegex() {
        }

        // pickup syntax: {shorthandName=pattern} where shorthandName must begin with a letter
        public  string ReplaceShorthandPatternWithFormalRegex(string field) {
            Regex shorthandPattern = new Regex(@"\{([a-zA-Z]\w+?)(=.*?)?\}"); 

            MatchCollection mc = shorthandPattern.Matches(field);
            foreach (Match m in mc) {
                string shorthandName = m.Groups[1].Value;
                string pattern = m.Groups[2].Value;
                if (string.IsNullOrEmpty(pattern)) {
                    pattern = ".+?";
                    field = field.Replace("{" + shorthandName + "}",
                                                          String.Format(@"(?<{0}>{1})", shorthandName, pattern));
                }
                else {
                    pattern = pattern.Substring(1); // ignore the '=' delimiter
                    field = field.Replace("{" + shorthandName + "=" + pattern + "}",
                                                          String.Format(@"(?<{0}>{1})", shorthandName, pattern));
                }
            }
            return field;
        }
    }
}