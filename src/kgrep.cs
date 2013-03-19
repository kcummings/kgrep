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
            string filename;
            string matchpattern;
            string topattern = null;
            const string READ_STDIN = "stdin";  // can be any literal as long as it's not a valid file name

            // cat filename|kgrep matchpattern
            if (args.Length == 1) {  
                matchpattern = args[0];
                ScanAndPrintTokens(READ_STDIN, matchpattern);  
            }
            
            else if (args.Length == 2) {
                filename = args[1];
                switch (args[0])
	            {
                    case "-f":  // cat filename|kgrep -f replacementFile
                        ReplacementFile rf = new ReplacementFile(filename);
                        SearchAndReplaceTokens(rf, READ_STDIN);
                        break;
                    default:
                        matchpattern = args[0];
                        if (File.Exists(filename))
                            ScanAndPrintTokens(matchpattern, filename);  // kgrep matchpattern filename 
                        else 
                            SingleFindAndReplaceStdin(matchpattern, topattern); // cat filename|kgrep matchpattern topattern
                        break;
                }
            }

            // kgrep matchpattern filename1 .... filenameN
            else if (args.Length > 2) {
                matchpattern = args[0];
                topattern = args[1];
                if (File.Exists(topattern))
                    ScanAndPrintFromMultipleFiles(matchpattern, args);
                else  
                    SingleFindAndReplaceStdin(matchpattern, topattern);
            } 
            else {
                Usage();
            }
        }

        private static void Usage() {
            Console.WriteLine("kgrep (Kevin's grep) v0.05");
            Console.WriteLine("Usage: kgrep matchpattern [topattern] filename1 ... filenameN");
            Console.WriteLine("       kgrep -f patternfile filename1 ... filenameN");
            Console.WriteLine("       cat filename|kgrep matchpattern [topattern]");
        }

        // kgrep matchpattern filename1 ... filenameN
        private static void ScanAndPrintFromMultipleFiles(string matchpattern, string[] filelist) {
            for (int i = 1; i < filelist.Length; i = i + 1) {
                if (File.Exists(filelist[i])) {
                    ScanAndPrintTokens(filelist[i], matchpattern);  
                }
            }
        }

        // kgrep matchpattern topattern
        static void SingleFindAndReplaceStdin(string fromPattern, string toPattern) {
            List<Replacement> repList = new List<Replacement>();
            Replacement rep = new Replacement("", fromPattern, toPattern);
            repList.Add(rep);
            SearchAndReplaceTokens(repList, "stdin", true);
        }

        // kgrep being used as a scanner/grep.
        static void ScanAndPrintTokens(string matchpattern, string filename) {
            HandleInput sr = new HandleInput(filename);

            try {
                KgrepEngine engine = new KgrepEngine();
                string line;
                while ((line = sr.ReadLine()) != null) {
                    string alteredLine = engine.ScanForTokens(line, matchpattern, "\n");
                    if (!String.IsNullOrEmpty(alteredLine)) Console.WriteLine(alteredLine);
                }
            }
            catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
            finally {
                sr.Close();
            }
        }

        // Kgrep being used as sed.
        static void SearchAndReplaceTokens(ReplacementFile rf, string filename) {
            List<Replacement> repList = new List<Replacement>();
            repList = rf.GetReplacements();
            SearchAndReplaceTokens(repList, filename, rf.replaceAll);
        }

        static void SearchAndReplaceTokens(List<Replacement> repList, string filename, bool replaceAll) {
            HandleInput sr = new HandleInput(filename);
            if (repList.Count == 0)
                return;

             try {
                 KgrepEngine engine = new KgrepEngine();
                 string line;
                 string alteredLine;
                 while ((line = sr.ReadLine()) != null) {
                     if (replaceAll) 
                         alteredLine = engine.ApplyReplacementsAll(line, repList);
                     else 
                         alteredLine = engine.ApplyReplacementsFirst(line, repList);
                     if (!String.IsNullOrEmpty(alteredLine)) Console.WriteLine(alteredLine);
                 }
            }
            catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
            finally {
                sr.Close();
            }
        }
    }
}
