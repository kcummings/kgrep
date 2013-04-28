using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NLog;

namespace kgrep
{
    public class ReplacementFile
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public List<Replacement> ReplacementList = new List<Replacement>();
        private bool _ScopeAll = true;
        public bool ScopeAll { get { return _ScopeAll; } }
        private String _comment = "#";
        private String _delim = "~";
        private IHandleInput sr;
        public string ScannerFS = "\n";

        public ReplacementFile(String filename) {
            logger.Debug("Start reading replacementfile:{0}",filename);
            sr = (new ReadFileFactory()).GetSource(filename);
            ReplacementList = GetReplacementList();
        }
 
        public List<Replacement> GetReplacementList() {
            List<Replacement> replacementList = new List<Replacement>();
            String line;
            while ((line = sr.ReadLine()) != null) {
                logger.Trace("   replacement source line:{0}",line);
                line = line.Trim();

                // Remove comment lines.
                if (line == String.Empty || line.StartsWith(_comment)) {    
                    continue;
                }

                // Remove any trailing comments.
                int i = line.IndexOf(_comment);
                if (i >= 0)
                    line = line.Remove(i);

                if (line.ToLower().StartsWith("comment=")) 
                    _comment = GetOption(line, "comment"); 
                else if (line.ToLower().StartsWith("delim="))
                    _delim = GetOption(line, "delim");
                else if (line.ToLower().StartsWith("scope=first"))  // Once true, it's true for the remaining replacements.
                    _ScopeAll = false;
                else if (line.ToLower().StartsWith("scope=all"))  
                    _ScopeAll = true;
                else if (line.ToLower().StartsWith("scannerfs=")) 
                    ScannerFS = GetOption(line, "FS");
                else {
                    String[] parts = line.Split(_delim.ToCharArray(), 4);
                    if (parts.Length == 1) { // just scan pattern
                        replacementList.Add(new Replacement(parts[0]) {ScannerFS = ScannerFS });
                    }
                    if (parts.Length == 2) {
                        // just a~b pattern
                        replacementList.Add(new Replacement(parts[0], parts[1]));
                    }
                    if (parts.Length == 3) {
                        // anchored a~b pattern
                        replacementList.Add(new Replacement(parts[0], parts[1], parts[2]));
                    }
                }
            }
            sr.Close();
            logger.Debug("There are {0} replacements in replacement file", replacementList.Count);
            return replacementList;
        }

        // Get the provided value for the given Control Option.
        // allow optional enclosing in quotes
        private string GetOption(string line, string type) {
            Match m = Regex.Match(line, type+"=\"(.+)\"", RegexOptions.IgnoreCase);  
            if (!m.Success)
                m = Regex.Match(line, type+"=(.+)", RegexOptions.IgnoreCase);
            return m.Groups[1].Value;
        }
    }
}
