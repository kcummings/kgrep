﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace kgrep 
{
    public class ParseCommandLine  {

        // kgrep [-v] replacementfile filename1 ... filenameN     
        // cat filename|kgrep [-v] replacementfile   

        // Input is either stdin or files, not both.
        // If matchpattern is a file, it contains ReplacementPatterns.
        // If matchpattern contains a "~" it is treated as a ReplacementPattern, else a ScanToPrintPattern.

        public string ReplacementFileName = null;
        public bool OutputAllLines = true;
        public List<string> InputSourceList {
            get { return _inputSourceList; }
        }
        private List<string> _inputSourceList = new List<string>();
        public string STDIN = "stdin?";
        public IUtilities utilities;

        public ParseCommandLine() {
            utilities = new Utilities();
        }
        public void Init(string[] args) {
            Init(new List<string>(args));
        }

        public void Init(List<string> args) {
           
            if (args.Count == 0) return;

            if (args[0] == "-o") {
                OutputAllLines = false;
                args.RemoveAt(0);
            }

            if (args.Count == 1) {   // cat filename|kgrep [-v] replacementfile
                ReplacementFileName = args[0];
                _inputSourceList.Add(STDIN);
                return;
            }

            // kgrep [-v] replacementfile filename1 .... filenameN
            ReplacementFileName = args[0];
            args.RemoveAt(0);
            
            foreach (string arg in args) {
                foreach (string filename in utilities.ExpandFileNameWildCards(arg)) {
                    _inputSourceList.Add(filename);                    
                }
            } 
        }
    }
}
