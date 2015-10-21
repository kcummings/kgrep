using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace kgrep {
    public class PrintTokensInSourceFiles : IFileAction {
        public IHandleOutput sw = new WriteStdout();
        private int _countOfMatchesInFile = 0;
        private int _lineNumber = 0;

        public string ApplyCommandsToInputFileList(ParseCommandFile rf, List<string> inputFilenames) {
            try {
                string line;

                foreach (string filename in inputFilenames) {
                    IHandleInput sr = (new ReadFileFactory()).GetSource((filename));
                    _lineNumber = 0;
                    _countOfMatchesInFile = 0;
                    while ((line = sr.ReadLine()) != null) {
                        _lineNumber++;
                        foreach (Command command in rf.CommandList) {
                            sw.Write(ScanForTokens(line, command.SubjectRegex, rf.ScannerFS));
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
            _countOfMatchesInFile += sb.Count;
            return String.Join(FS, sb.ToArray());
        }
    }
}