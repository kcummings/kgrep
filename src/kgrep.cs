using System;
/*
##The MIT License (MIT)
####*Copyright (c) 2013, Kevin Cummings <kevin.cummings@gmx.com>*

 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
 * documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

namespace kgrep
{
    // kgrep can scan an input source for all occurances of a pattern 
    // or scan and replace based on regular expressions.
    // All output is written to stdout (console).
    class Kgrep
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

            KgrepEngine engine = new KgrepEngine();
            if (cmdarg.ReplacementFileName != null) {  // #C
                ReplacementFile rf = new ReplacementFile(cmdarg.ReplacementFileName);
                engine.SearchAndReplaceTokens(rf, cmdarg.InputSourceNames);
            } else if (cmdarg.ReplacementPattern != null && cmdarg.SearchPattern != null)  // #A
                engine.SearchAndReplaceTokens(cmdarg.SearchPattern, cmdarg.ReplacementPattern, cmdarg.InputSourceNames);
            else if (cmdarg.ReplacementPattern == null && cmdarg.SearchPattern != null)
                engine.ScanAndPrintTokens(cmdarg.SearchPattern, cmdarg.InputSourceNames);  // #B
            else {
                Console.WriteLine("unknown command line argument pattern");
                Usage();
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
