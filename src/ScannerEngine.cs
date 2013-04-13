using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace kgrep {
    public class ScannerEngine {

        public IHandleOutput sw = new WriteStdout();

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
