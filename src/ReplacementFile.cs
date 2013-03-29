using System;
using System.Collections.Generic;
using System.IO;

namespace kgrep
{
    public class ReplacementFile
    {
        private List<Replacement> replacementList = new List<Replacement>();
        private bool _isReplaceAll = true;
        public bool isReplaceAll { get { return _isReplaceAll; } }
        private String COMMENT = "#";
        private String DELIM = "~";
        private IHandleInput sr;

        public ReplacementFile(String filename) {
            sr = (new ReadFileFactory()).GetSource(filename);
        }
 
        public List<Replacement> GetReplacements() {
 
            String line;
            while ((line = sr.ReadLine()) != null) {

                line = line.Trim();

                // Remove comment lines.
                if (line == String.Empty || line.StartsWith(COMMENT)) {    
                    continue;
                }

                if (line.ToLower().StartsWith("comment="))
                    COMMENT = line.Substring("comment=".Length,1);

                if (line.ToLower().StartsWith("delim="))
                    DELIM = line.Substring("delim=".Length, 1);

                // Once true, it's true for the remaining replacements.
                if (line.ToLower().StartsWith("ReplacementMode=All"))   
                    _isReplaceAll = true;

                // Remove any trailing comments.
                int i = line.IndexOf(COMMENT);
                if (i >= 0)
                    line = line.Remove(i);

                String[] parts = line.Split(DELIM.ToCharArray(),4);
                if (parts.Length == 2) {
                    // input pattern: from ~ to         # assuming ~ is delim char.
                    //Console.WriteLine("Adding {0} to {1} with {2}", parts[0], parts[1], multiple.ToString());
                    replacementList.Add( new Replacement("", parts[0].Trim(), parts[1].Trim()));
                }
                if (parts.Length == 3) {
                    // input pattern: anchor ~ from ~ to
                    replacementList.Add(new Replacement(parts[0].Trim(), parts[1].Trim(), parts[2].Trim()));
                }
            }
            sr.Close();
            return replacementList;
        }

    }
}
