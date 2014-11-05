using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace kgrep {


    // Values are in Named and unnamed Captures are only in SubjectString.
    // e.g. named capture syntax: (?<digit>[0-9]+)  yeilds pickup name ${digit} 
    //    unnamed capture syntax: ([0-9]+)    yeilds pickup name ${1}
    public class Pickup {
        private static Dictionary<string, string> PickupList = new Dictionary<string, string>();


        public void CollectAllPickupsInLine(string line, Command command) {
            if (command.IsCaptureInSubjectString) {
                Match m = command.SubjectString.Match(line);
                if (m.Success) {
                    foreach (int groupNumber in command.SubjectString.GetGroupNumbers()) {
                        string name = command.SubjectString.GroupNameFromNumber(groupNumber);
                        if (PickupList.ContainsKey(name))
                            PickupList[name] = m.Groups[groupNumber].Value;
                        else
                            PickupList.Add(name, m.Groups[groupNumber].Value);
                    }
                }
            }
        }

        public string ReplacePickupsWithStoredValue(string line) {
            string PickupValue;

            MatchCollection mc = Command.PickupPattern.Matches(line);
            foreach (Match m in mc) {
                PickupValue = m.Groups[1].Value;
                if (PickupList.ContainsKey(PickupValue))
                    line = line.Replace("${" + PickupValue + "}", PickupList[PickupValue]);
            }
            return line;
        }

        public void ClearPickupList() {  
            PickupList.Clear();
        }

        // pickup syntax: {shorthandName=pattern} where shorthandName must begin with a letter
        public string ReplaceShorthandPatternWithFormalRegex(string field) {
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