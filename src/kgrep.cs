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

    // kgrep matchpattern filename1 ... filenameN     
    // cat filename|kgrep matchpattern               

    // If matchpattern is a file, it contains a ReplacementPatterns.
    // If matchpattern contains a "~" it is treated as a ReplacementPattern, else a ScanToPrintPattern.

    class Kgrep
    {
        static void Main(string[] args) {

            ParseCommandLine cmdarg = new ParseCommandLine(args);
            if (cmdarg.InputSourceNames.Count == 0) {
                Usage("No input sources given/recognized.");
            }

            KgrepEngine engine = new KgrepEngine();
            if (cmdarg.ReplacementFileName != null) 
                engine.SearchAndReplaceTokens(cmdarg.ReplacementFileName, cmdarg.InputSourceNames);
            else if (cmdarg.SearchPattern != null)  
                engine.ScanAndPrintTokens(cmdarg.SearchPattern, cmdarg.InputSourceNames);  
            else {
                Usage("unknown command line argument pattern");
            }
        }

        private static void Usage(string message) {
            if (!string.IsNullOrEmpty(message)) Console.WriteLine(message);
            Console.WriteLine("kgrep (Kevin's grep) v0.6");
            Console.WriteLine("Usage: kgrep matchpattern filename1 ... filenameN");
            Console.WriteLine("       cat filename|kgrep matchpattern");
            Console.WriteLine(" matchpattern can be either a regex string to scan or a replacement commands");
            Environment.Exit(1);
        }
    }
}
