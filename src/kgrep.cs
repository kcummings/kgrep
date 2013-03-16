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
                        else {
                            topattern = args[1];
                            grep(READ_STDIN, matchpattern, topattern);  // cat filename|kgrep matchpattern topattern
                        }
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
                        else
                            grep(args[i], matchpattern, topattern);  // kgrep matchpattern topattern filename1 ... filenameN
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
                r = new Regex(matchpattern);
                while ((line = sr.ReadLine()) != null) {
                    Match m = Regex.Match(line,matchpattern);
                    while (m.Success) {
                        int[] gnums = r.GetGroupNumbers();
                        if (gnums.Length > 1) {
                            for (int i = 1; i < gnums.Length; i++) {
                                Console.Write(m.Groups[gnums[i]]);
                            }
                            Console.Write("\n");
                        }
                        else
                            // Only print the substring that was matched.
                            Console.WriteLine(m.Value);
                        m = m.NextMatch();
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
            finally {
                sr.Close();
            }
        }

        static void grep(string filename, string matchpattern, string topattern) {
            string fileAsOneString;
            HandleInput sr = new HandleInput(filename);

            try {
                fileAsOneString = Regex.Replace(sr.ReadToEnd(), matchpattern, markToCharacters(topattern), RegexOptions.Multiline);
                Console.Write(fileAsOneString.Replace("\\n","\n"));
            }
            catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
            finally {
                sr.Close();
            }
        }

        static string markToCharacters(string tostring) {
            tostring = tostring.Replace(@"\s", " ");
            tostring = tostring.Replace(@"\n", "\n");
            return tostring.Replace(@"\t", "\t");
        }

        // Use replacements from a collection rather than the command line.
        static void grep(string filename, List<Replacement> repList) {
            HandleInput sr = new HandleInput(filename);
            if (repList.Count == 0)
                return;

             try {

                 // First occurance replaces are on a line basis.
                 string line;
                 StringBuilder sb = new StringBuilder();
                 while ((line = sr.ReadLine()) != null) {
                    foreach (Replacement rep in repList) {
                        if (rep.criteria.Length > 0) {
                            if (!Regex.IsMatch(line, rep.criteria))
                                continue;
                        }                        
                        if (!rep.multiple) {
                        //    if (Regex.IsMatch(line, rep.pattern)) {
                                //line = Regex.Replace(line, rep.fromPattern, rep.topattern);
                                line = rep.fromPattern.Replace(line, rep.topattern);
                        //        break;
                         //   }
                        }
                     }
                     sb.Append(line);
                     sb.Append('\n');
                    //Console.WriteLine(line);
                 }

                 // Multiple occurance replacements are on a one string file basis.
                 string fileAsOneString = sb.ToString();
                 foreach (Replacement rep in repList) {
                     if (rep.multiple) {
                         //fileAsOneString = Regex.Replace(fileAsOneString, rep.pattern, markToCharacters(rep.topattern), RegexOptions.Multiline);
                         fileAsOneString = rep.fromPattern.Replace(fileAsOneString, markToCharacters(rep.topattern));
                     }
                 }
                 Console.Write(fileAsOneString.Replace("\n", Environment.NewLine));
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
