using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NLog;

namespace kgrep {
    public class PrintTokensInSourceFiles : IFileAction {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public IHandleOutput sw = new WriteStdout();
        private int _countOfMatchesInFile = 0;
        private int _lineNumber = 0;

        public string ApplyCommands(ParseCommandFile rf, List<string> inputFilenames) {
            try {
                string line;

                foreach (string filename in inputFilenames) {
                    logger.Debug("Scanning - Processing input file:{0}", filename);
                    IHandleInput sr = (new ReadFileFactory()).GetSource((filename));
                    _lineNumber = 0;
                    _countOfMatchesInFile = 0;
                    while ((line = sr.ReadLine()) != null) {
                        _lineNumber++;
                        logger.Trace("Scanning line:{0}", line);
                        foreach (Command command in rf.CommandList) {
                            sw.Write(ScanForTokens(line, command.SubjectString, command.ScannerFS));
                        }
                    }
                    sr.Close();
                    logger.Info("File {0} found {1} matches on {2} input lines", filename, _countOfMatchesInFile, _lineNumber);
                }
            } catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
            return sw.Close();
        }

        public string ScanForTokens(string line, Regex pattern, string FS) {
            List<string> sb = new List<string>();
            Match m = pattern.Match(line);
            logger.Trace("   scanpatten:{0} FS:{1}", pattern.ToString(), FS);

            // Only return submatches if found, otherwise return any matches.
            while (m.Success) {
                int[] gnums = pattern.GetGroupNumbers();
                if (gnums.Length > 1) {
                    for (int i = 1; i < gnums.Length; i++) {
                        string token = m.Groups[gnums[i]].ToString();
                        logger.Trace("      Found grouped token:{0}", token);
                        sb.Add(token);
                    }
                } else {
                    // Only include the substring that was matched.
                    logger.Trace("      Found token:{0}",m.Value);
                    sb.Add(m.Value);
                }
                m = m.NextMatch();
            }
            logger.Trace("   returning:{0}", String.Join(FS, sb.ToArray()));
            _countOfMatchesInFile += sb.Count;
            if (sb.Count>0)
                logger.Debug("At line {0} found {1} occurances of '{2}' in '{3}'", _lineNumber, sb.Count, pattern.ToString(), line);
            return String.Join(FS, sb.ToArray());
        }
    }
}