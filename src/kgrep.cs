using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Diagnostics;

namespace kgrep
{
    // Usage: kgrep matchpattern [topattern] filename
    // Finds ALL occurances of matchpattern in filename.
    // All output is written to stdout (console).
    class kgrep
    {
        static void Main(string[] args) {
            string filename;
            string matchpattern;
            string topattern = null;
            const string READ_STDIN = "stdin";  // can be any literal as long as it's not a valid file name
            List<Replacement> repList = new List<Replacement>();

            //Console.WriteLine(args.Length.ToString());
            if (args.Length == 1) {  // cat filename|kgrep matchpattern
                matchpattern = args[0];
                grep(READ_STDIN, matchpattern);  // read from stdin
            }
            else if (args.Length == 2) {// kgrep matchpattern filename OR cat filename|kgrep matchpattern topattern
                filename = args[1];
                switch (args[0])
	            {
                    case "-f":  // kgrep -f replacementFile
                        repList = (new ReadReplacementFile(filename)).GetReplacements();
                        grep(READ_STDIN, repList);
                        break;
                    default:
                        matchpattern = args[0];
                        if (File.Exists(filename))
                            grep(filename, matchpattern);  // kgrep matchpattern filename 
                        else {
                            SingleFindAndReplace(matchpattern, topattern);
                        }
                        break;
                }
            }
            else if (args.Length > 2) {
                // kgrep matchpattern filename1 .... filenameN
                matchpattern = args[0];
                topattern = args[1];
                if (File.Exists(topattern)) { // no pattern given, it's a file instead
                    for (int i = 1; i < args.Length; i = i + 1) {
                        if (File.Exists(args[i])) {
                            grep(args[i], matchpattern);  // kgrep matchpattern filename1 ... filenameN
                        }
                    }
                }
                else {  // kgrep matchpattern topattern
                    SingleFindAndReplace(matchpattern, topattern);
                }
            } 
            else {
                Console.WriteLine("kgrep (Kevin's grep) v0.03");
                Console.WriteLine("Usage: kgrep matchpattern [topattern] filename1 ... filenameN");
                Console.WriteLine("       kgrep -f patternfile filename1 ... filenameN");
                Console.WriteLine("       cat filename|kgrep matchpattern [topattern]");
            }
            //Console.ReadLine();
        }

        // kgrep matchpattern topattern
        static void SingleFindAndReplace(string fromPattern, string toPattern) {
            List<Replacement> repList = new List<Replacement>();
            Replacement rep = new Replacement(false, "", fromPattern, toPattern);
            repList.Add(rep);
            grep("stdin", repList);
        }

        // kgrep being used as a scanner/grep.
        static void grep(string filename, string matchpattern) {
            string line;
            Regex r = null;
            HandleInput sr = new HandleInput(filename);

            try {
                KgrepEngine engine = new KgrepEngine();
                r = new Regex(matchpattern);
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
        static void grep(string filename, List<Replacement> repList) {
            HandleInput sr = new HandleInput(filename);
            if (repList.Count == 0)
                return;

             try {
                 KgrepEngine engine = new KgrepEngine();
                 string line;
                 while ((line = sr.ReadLine()) != null) {
                     string alteredLine = engine.ApplyReplacements(line, repList);
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
