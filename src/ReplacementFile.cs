using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace kgrep
{
    public class ReplacementFile
    {
        public List<Replacement> ReplacementList = new List<Replacement>();
        private bool _ScopeAll = true;
        public bool ScopeAll { get { return _ScopeAll; } }
        private String COMMENT = "#";
        private String DELIM = "~";
        private IHandleInput sr;
        public Dictionary<string, Pickup> PickupDefinitions = new Dictionary<string, Pickup>();
        private Regex _pickupDefinitionPattern = new Regex(@"\s*(\{[a-zA-Z][a-zA-Z0-9]*?\})\s*=\s*(.+)", RegexOptions.Compiled);


        public ReplacementFile(String filename) {
            sr = (new ReadFileFactory()).GetSource(filename);
            ReplacementList = GetReplacements();
        }
 
        private List<Replacement> GetReplacements() {

            String line;
            while ((line = sr.ReadLine()) != null) {
                line = line.Trim();

                // Remove comment lines.
                if (line == String.Empty || line.StartsWith(COMMENT)) {    
                    continue;
                }

                if (line.ToLower().StartsWith("comment=")) {
                    COMMENT = line.Substring("comment=".Length, 1);
                    continue;
                }

                if (line.ToLower().StartsWith("delim=")) {
                    DELIM = line.Substring("delim=".Length, 1);
                    continue;
                }

                // Once true, it's true for the remaining replacements.
                if (line.ToLower().StartsWith("scope=first")) {
                    _ScopeAll = false;
                    continue;
                }

                // Remove any trailing comments.
                int i = line.IndexOf(COMMENT);
                if (i >= 0)
                    line = line.Remove(i);

                // Extract and store any Pickups.
                Match m = _pickupDefinitionPattern.Match(line);
                if (m.Success) {
                    string pickupName = m.Groups[1].Value;
                    string pickupPattern = m.Groups[2].Value;
                    PickupDefinitions.Add( pickupName, new Pickup(pickupName, pickupPattern) );
                    continue;
                }

                String[] parts = line.Split(DELIM.ToCharArray(),4);
                if (parts.Length == 2) {  // just a~b pattern
                    ReplacementList.Add( new Replacement("", parts[0].Trim(), parts[1].Trim()));
                }
                if (parts.Length == 3) {    // anchored a~b pattern
                    ReplacementList.Add(new Replacement(parts[0].Trim(), parts[1].Trim(), parts[2].Trim()));
                }
            }
            sr.Close();
            return ReplacementList;
        }
    }
}
