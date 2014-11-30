using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using NLog;

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
    // All output is written to stdout.

    // kgrep matchpattern filename1 ... filenameN     
    // cat filename|kgrep matchpattern               

    class Kgrep
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args) {

            Stopwatch timer = new Stopwatch();
            timer.Start();

            logger.Info("<<<<< Starting kgrep >>>>>");
            ParseCommandLine commandLine = new ParseCommandLine();
            commandLine.Init(args);
            if (commandLine.InputSourceList.Count == 0)
                Usage("No input sources given/recognized.");

            ParseCommandFile commands = new ParseCommandFile(commandLine.ReplacementFileName);
            IFileAction engine = (new FileActionFactory()).GetFileAction(commands.kgrepMode);
            engine.ApplyCommandsToInputFiles(commands, commandLine.InputSourceList); 

            timer.Stop();
            logger.Info("<<<<< Ending kgrep [{0}] >>>>>", timer.Elapsed);
        }

        private static void Usage(string message) {
            string versionNumber = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (!string.IsNullOrEmpty(message)) Console.WriteLine(message);
            Console.WriteLine("kgrep (Kevin's grep) v{0}",versionNumber);
            Console.WriteLine("Usage: kgrep matchpattern filename1 ... filenameN");
            Console.WriteLine("       cat filename|kgrep matchpattern");
            Console.WriteLine(" matchpattern can be either a regex string to scan or replace based on a regex");
            Environment.Exit(1);
        }
    }
}
