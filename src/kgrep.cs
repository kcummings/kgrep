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

            //Console.WriteLine(args.Length.ToString());
            if (args.Length == 1) {  // cat filename|kgrep matchpattern
                matchpattern = args[0];
                grep(READ_STDIN, matchpattern);  // read from stdin
            }
            else if (args.Length == 2) {// kgrep matchpattern filename OR cat filename|kgrep matchpattern topattern
                filename = args[1];
                List<Replacement> repList;
                switch (args[0])
	            {
                    case "-f":  // kgrep -f replacementFile
                        repList = (new ReadReplacementFile(filename)).GetReplacements();
                        //foreach (Replacement r in repList) {
                        //    Console.WriteLine(String.Concat("{", r.fromPattern, "} {", r.topattern, "}"));
                        //}
                        grep(READ_STDIN, repList);
                        break;
                    default:
                        matchpattern = args[0];
                        if (File.Exists(filename))
                            grep(filename, matchpattern);  // kgrep matchpattern filename 
                        break;
                }
            }
            else if (args.Length > 2) {
                // kgrep matchpattern [topattern] filename1 .... filenameN
                // kgrep matchpattern filename1 .... filenameN
                int firstFile = 2;  
                matchpattern = args[0];
                topattern = args[1];
                if (File.Exists(topattern)) { // no pattern given, it's a file instead.
                    topattern = null;
                    firstFile = 1;
                }

                for (int i=firstFile; i<args.Length; i=i+1) {
                    if (File.Exists(args[i])) {
                        if (topattern == null)
                            grep(args[i], matchpattern);  // kgrep matchpattern filename1 ... filenameN
                    }
                }
            } 
            else {
                Console.WriteLine("kgrep (Kevin's grep) v0.02");
                Console.WriteLine("Usage: kgrep matchpattern [topattern] filename1 ... filenameN");
                Console.WriteLine("       kgrep -f patternfile filename1 ... filenameN");
                Console.WriteLine("       cat filename|kgrep matchpattern [topattern]");
            }
            //Console.ReadLine();
        }

        static void grep(string filename, string matchpattern) {
            string line;
            Regex r = null;
            HandleInput sr = new HandleInput(filename);

            try {
                KgrepEngine engine = new KgrepEngine();
                r = new Regex(matchpattern);
                while ((line = sr.ReadLine()) != null) {
                    engine.ScanForTokens(line, matchpattern, "\n");
                }
            }
            catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
            finally {
                sr.Close();
            }
        }

        // Use replacements from a collection rather than the command line.
        static void grep(string filename, List<Replacement> repList) {
            HandleInput sr = new HandleInput(filename);
            if (repList.Count == 0)
                return;

             try {
                 KgrepEngine engine = new KgrepEngine();
                 string line;
                 while ((line = sr.ReadLine()) != null) {
                     Console.WriteLine(engine.ApplyReplacements(line, repList));
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
