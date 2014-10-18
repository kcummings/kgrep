using System.Collections.Generic;
using System.IO;
using NLog;
namespace kgrep 
{
    public class ParseCommandLine {

        // kgrep matchpattern filename1 ... filenameN     
        // cat filename|kgrep matchpattern   

        // Input is either stdin or files, not both.
        // If matchpattern is a file, it contains ReplacementPatterns.
        // If matchpattern contains a "~" it is treated as a ReplacementPattern, else a ScanToPrintPattern.

      //  public string SearchPattern = null;
        public string ReplacementFileName = null;
        public List<string> InputSourceNames = new List<string>();
        public string STDIN = "stdin?";
        private static Logger logger = LogManager.GetCurrentClassLogger();
 
        public ParseCommandLine(string[] args) {

            if (args.Length == 1) {   // cat filename|kgrep matchpattern
                ReplacementFileName = args[0];
                InputSourceNames.Add(STDIN);
                logger.Info("Reading stdin with command file {0}", ReplacementFileName);
            }

            // kgrep matchpattern filename1 .... filenameN
            if (args.Length > 1) {
                ReplacementFileName = args[0];
                logger.Info("Reading from source files with command file {0}", ReplacementFileName);
                for (int i = 1; i < args.Length; i = i + 1) {
                    logger.Debug("   adding source file:{0}",args[i]);
                    InputSourceNames.Add(args[i]);
                }
            }
        }
    }
}
