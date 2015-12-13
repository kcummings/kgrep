using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace kgrep {
    public class PrintTokensInSourceFiles : IFileAction {
        public IHandleOutput sw = new WriteStdout();

        public string ApplyCommandsToInputFileList(ParseCommandFile rf, List<string> inputFilenames) {
            try {
                string line;

                foreach (string filename in inputFilenames) {
                    IHandleInput sr = (new ReadFileFactory()).GetSource((filename));
                    while ((line = sr.ReadLine()) != null) {
                        foreach (Command command in rf.CommandList) {
                            if (isCandidateForPrinting(line, command))
                               sw.Write(ScanForTokens(line, command.SubjectRegex, command.OFS));
                        }
                    }
                    sr.Close();
                }
            } catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
            return sw.Close();
        }

        public string ScanForTokens(string line, Regex pattern, string FS) {
            List<string> sb = new List<string>();
            Match m = pattern.Match(line);

            // Only return submatches if found, otherwise return any matches.
            while (m.Success) {
                int[] gnums = pattern.GetGroupNumbers();
                if (gnums.Length > 1) {
                    for (int i = 1; i < gnums.Length; i++) {
                        string token = m.Groups[gnums[i]].ToString();
                        sb.Add(token);
                    }
                } else {
                    // Only include the substring that was matched.
                    sb.Add(m.Value);
                }
                m = m.NextMatch();
            }
            return String.Join(FS, sb.ToArray());
        }

        private bool isCandidateForPrinting(string line, Command command) {
            if (!command.IsAnchored)   
                return true;

            if (command.IsPickupOnly)
                return false;

            if (Regex.IsMatch(line, command.AnchorString))
                return true;
            return false;
        }
    }
}