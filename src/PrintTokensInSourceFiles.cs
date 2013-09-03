using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NLog;

namespace kgrep {
    public class PrintTokensInSourceFiles {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public IHandleOutput sw = new WriteStdout();

        public string ApplyScanner(ParseReplacementFile rf, List<string> inputFilenames) {
            try {
                string line;

                foreach (string filename in inputFilenames) {
                    logger.Debug("Scanning - Processing input file:{0}", filename);
                    IHandleInput sr = (new ReadFileFactory()).GetSource((filename));
                    while ((line = sr.ReadLine()) != null) {
                        logger.Debug("Scanning line:{0}", line);
                        foreach (Replacement rep in rf.ReplacementList) {
                            sw.Write(ScanForTokens(line, rep.FromPattern, rep.ScannerFS));
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
            logger.Debug("   ScanForTokens - scanpatten:{0} FS:{1}", pattern.ToString(), FS);

            // Only return submatches if found, otherwise return any matches.
            while (m.Success) {
                int[] gnums = pattern.GetGroupNumbers();
                if (gnums.Length > 1) {
                    for (int i = 1; i < gnums.Length; i++) {
                        string token = m.Groups[gnums[i]].ToString();
                        logger.Trace("      ScanForTokens - Found grouped token:{0}", token);
                        sb.Add(token);
                    }
                } else {
                    // Only include the substring that was matched.
                    logger.Trace("      ScanForTokens - Found token:{0}",m.Value);
                    sb.Add(m.Value);
                }
                m = m.NextMatch();
            }
            logger.Debug("   ScanForTokens - returning:{0}",String.Join(FS, sb.ToArray()));
            return String.Join(FS, sb.ToArray());
        }
    }
}