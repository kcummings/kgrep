using System.Collections.Generic;
using System.IO;

namespace kgrep 
{
    public class ParseCommandLine {

        // kgrep matchpattern filename1 ... filenameN     
        // cat filename|kgrep matchpattern   

        // Input is either stdin or files, not both.
        // If matchpattern is a file, it contains ReplacementPatterns.
        // If matchpattern contains a "~" it is treated as a ReplacementPattern, else a ScanToPrintPattern.

        public string SearchPattern = null;
        public string ReplacementFileName = null;
        public List<string> InputSourceNames = new List<string>();
        public string STDIN = "stdin?";

        public ParseCommandLine(string[] args) {

            if (args.Length == 1) {   // cat filename|kgrep matchpattern
                string firstArgument = args[0];
                if (isReplacementString(firstArgument))
                    ReplacementFileName = firstArgument;
                else
                    SearchPattern = firstArgument;
                InputSourceNames.Add(STDIN);
            }

            // kgrep matchpattern filename1 .... filenameN
            if (args.Length > 1) {
                string firstArgument = args[0];
                if (isReplacementString(firstArgument))
                    ReplacementFileName = firstArgument;
                else
                    SearchPattern = firstArgument;
                for (int i = 1; i < args.Length; i = i + 1) {
                    if (File.Exists(args[i]))
                        InputSourceNames.Add(args[i]);
                }
            }
        }

        private bool isReplacementString(string arg) {
            if (File.Exists(arg) || arg.Contains("~") || arg.Contains("delim="))
                return true;
            return false;
        }
    }
}
