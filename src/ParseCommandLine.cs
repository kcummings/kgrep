using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace kgrep 
{
    public class ParseCommandLine {

        public string SearchPattern = null;
        public string ReplacementPattern = null;
        public string ReplacementFileName = null;
        public List<string> InputSourceNames = new List<string>();
        public string STDIN = "stdin?";

        public ParseCommandLine(string[] args) {

            // gather all filenames if present. stdin is considered a input source
            if (args.Length == 1) {
                SearchPattern = args[0];
                InputSourceNames.Add(STDIN);
            }

            if (args.Length == 2) {
                switch (args[0]) {
                    case "-f":  // cat filename|kgrep -f replacementFile
                        ReplacementFileName = args[1];
                        InputSourceNames.Add(STDIN);
                        break;
                    default:  // kgrep pattern filename(s)
                        SearchPattern = args[0];
                        if (File.Exists(args[1])) // kgrep matchpattern filename 
                            InputSourceNames.Add(args[1]);
                        else {
                            ReplacementPattern = args[1]; // cat filename|kgrep matchpattern topattern
                            InputSourceNames.Add(STDIN);
                        }
                        break;
                }
            }

            // kgrep matchpattern filename1 .... filenameN
            if (args.Length > 2) {
                int startArg = 1;
                SearchPattern = args[0];
                if (!File.Exists(args[1])) {
                    ReplacementPattern = args[1];
                    startArg = 2;
                }
                for (int i = startArg; i < args.Length; i = i + 1) {
                    if (File.Exists(args[i]))
                        InputSourceNames.Add(args[i]);
                }
            }
        }
    }
}
