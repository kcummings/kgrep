using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Diagnostics;

namespace kgrep
{
    // Finds ALL occurances of matchpattern in filename.
    // All output is written to stdout (console).
    class kgrep
    {
        static void Main(string[] args) {

            ParseCommandLine cmdarg = new ParseCommandLine(args);
            if (cmdarg.InputSourceNames.Count == 0) {
                Console.WriteLine("No input sources given.");
                Usage();
            }

            // kgrep matchpattern topattern filename1 ... filenameN     #A
            // cat filename|kgrep matchpattern topattern                #A
            // kgrep matchpattern filename1 ... filenameN               #B
            // cat filename|kgrep matchpattern                          #B   
            // kgrep -f patternfile filename1 ... filenameN             #C
            // kgrep -f patternfile filename1                           #C          
            // cat filename|kgrep -f patternfile                        #C
            if (cmdarg.ReplacementFileName != null) {  // #C
                ReplacementFile rf = new ReplacementFile(cmdarg.ReplacementFileName);
                SearchAndReplaceTokens(rf, cmdarg.InputSourceNames);
            } else if (cmdarg.ReplacementPattern != null && cmdarg.SearchPattern != null)  // #A
                SearchAndReplaceTokens(cmdarg.SearchPattern, cmdarg.ReplacementPattern, cmdarg.InputSourceNames);
            else if (cmdarg.SearchPattern != null && cmdarg.ReplacementPattern == null)
                ScanAndPrintTokens(cmdarg.SearchPattern, cmdarg.InputSourceNames);  // #B
            else {
                Console.WriteLine("unknown command line argument pattern");
                Usage();
            }
        }

        // kgrep being used as a scanner/grep.
        static void ScanAndPrintTokens(string matchpattern, List<string> filenames) {
            try {
                KgrepEngine engine = new KgrepEngine();
                foreach (string filename in filenames) {
                    HandleInput sr = new HandleInput(filename);
                    string line;
                    while ((line = sr.ReadLine()) != null) {
                        string alteredLine = engine.ScanForTokens(line, matchpattern, "\n");
                        if (!String.IsNullOrEmpty(alteredLine)) Console.WriteLine(alteredLine);
                    }
                    sr.Close();
                }
            }
            catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
        }

        // Kgrep being used as sed.
        static void SearchAndReplaceTokens(string searchPattern, string toPattern, List<String> filenames) {
            List<Replacement> reps = new List<Replacement>();
            Replacement rep = new Replacement("", searchPattern, toPattern);
            reps.Add(rep);
            SearchAndReplaceTokens(reps, filenames, true);
        }

        static void SearchAndReplaceTokens(ReplacementFile rf, List<string> filenames) {
            List<Replacement> repList = new List<Replacement>();
            repList = rf.GetReplacements();
            SearchAndReplaceTokens(repList, filenames, rf.isReplaceAll);
        }

        static void SearchAndReplaceTokens(List<Replacement> repList, List<string> filenames, bool isReplaceAll) {
            if (repList.Count == 0)
                return;

             try {
                 KgrepEngine engine = new KgrepEngine();
                 string line;
                 string alteredLine;
                 foreach (string filename in filenames) {
                     HandleInput sr = new HandleInput(filename);
                     while ((line = sr.ReadLine()) != null) {
                         if (isReplaceAll)
                             alteredLine = engine.ApplyReplacementsAll(line, repList);
                         else
                             alteredLine = engine.ApplyReplacementsFirst(line, repList);
                         if (!String.IsNullOrEmpty(alteredLine)) Console.WriteLine(alteredLine);
                     }
                     sr.Close();
                 }
            }
            catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
        }

        private static void Usage() {
            Console.WriteLine("kgrep (Kevin's grep) v0.2");
            Console.WriteLine("Usage: kgrep matchpattern [topattern] filename1 ... filenameN");
            Console.WriteLine("       kgrep -f patternfile filename1 ... filenameN");
            Console.WriteLine("       cat filename|kgrep matchpattern [topattern]");
            Environment.Exit(1);
        }
    }
}
