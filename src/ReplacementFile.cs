using System;
using System.Collections.Generic;
using System.IO;
using MS.Internal.Xml.XPath;

namespace kgrep
{
    public class ReplacementFile
    {
        public List<Replacement> ReplacementList = new List<Replacement>();
        private bool _ScopeAll = true;
        public bool ScopeAll { get { return _ScopeAll; } }
        private String _comment = "#";
        private String _delim = "~";
        private IHandleInput sr;

        public ReplacementFile(String filename) {
            sr = (new ReadFileFactory()).GetSource(filename);
            ReplacementList = GetReplacementList();
        }
 
        public List<Replacement> GetReplacementList() {

            List<Replacement> replacementList = new List<Replacement>();
            String line;
            while ((line = sr.ReadLine()) != null) {

                line = line.Trim();

                // Remove comment lines.
                if (line == String.Empty || line.StartsWith(_comment)) {    
                    continue;
                }

                if (line.ToLower().StartsWith("comment=")) 
                    _comment = line.Substring("comment=".Length, 1);
                else if (line.ToLower().StartsWith("delim=")) 
                    _delim = line.Substring("delim=".Length, 1);
                else if (line.ToLower().StartsWith("scope=first"))  // Once true, it's true for the remaining replacements.
                    _ScopeAll = false;
                else if (line.ToLower().StartsWith("scope=all"))  
                    _ScopeAll = true;
                else {
                    // Remove any trailing comments.
                    int i = line.IndexOf(_comment);
                    if (i >= 0)
                        line = line.Remove(i);

                    String[] parts = line.Split(_delim.ToCharArray(), 4);
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
            return replacementList;
        }

    }
}
