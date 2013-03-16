using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;

namespace kgrep
{
    class ReadReplacementFile
    {
        private List<Replacement> replacementList = new List<Replacement>();
        private String inputFile;
        String COMMENT = "#";

        public ReadReplacementFile(String filename) {
            inputFile = filename;
        }
        // using List = List<Replacement> ;
        public List<Replacement> GetReplacements() {
            StreamReader sr;
            Boolean multiple = false;

            if (File.Exists(inputFile)) {
                String line;

                sr = File.OpenText(inputFile);
                while ((line = sr.ReadLine()) != null) {

                    line = line.Trim();

                    // Remove comment lines.
                    if (line == String.Empty || line.StartsWith(COMMENT)) {    
                        continue;
                    }

                    if (line.StartsWith("comment="))
                        COMMENT = line.Substring("comment=".Length,1);

                    // Once true, it's true for the remaining replacements.
                    if (line.StartsWith("START MULTIPLE REPLACMENTS"))
                        multiple = true;

                    // Remove any trailing comments.
                    int i = line.IndexOf(COMMENT);
                    if (i >= 0)
                        line = line.Remove(i);

                    String[] parts = line.Split("~".ToCharArray(),4);
                    if (parts.Length == 2) {
                        // input pattern: from ~ to
                        //Console.WriteLine("Adding {0} to {1} with {2}", parts[0], parts[1], multiple.ToString());
                        replacementList.Add( new Replacement( multiple, "", parts[0].Trim(), parts[1].Trim()));
                    }
                    if (parts.Length == 3) {
                        // input pattern: anchor ~ from ~ to
                        replacementList.Add(new Replacement(multiple, parts[0].Trim(), parts[1].Trim(), parts[2].Trim()));
                    }
                }
                sr.Close();
                return replacementList;
           }
           return null;
        }

    }
}
