using System;
using System.Collections.Generic;

namespace kgrep {

    public class ReplaceAllMatches : ReplaceTokens {

        override public string ApplyCommands(ParseCommandFile rf, List<string> inputFilenames) {
            try {
                string line;
                string alteredLine;

                foreach (string filename in inputFilenames) {
                    DateTime startParse = DateTime.Now;
                    logger.Debug("Replace All Matches - Processing input file:{0}", filename);
                    IHandleInput sr = (new ReadFileFactory()).GetSource((filename));
                    _lineNumber = 0;
                    _countOfMatchesInFile = 0;
                    while ((line = sr.ReadLine()) != null) {
                        _lineNumber++;
                        // TODO: Print Info after every 5000 lines, i.e. I'm still processing
                        alteredLine = ApplyCommandsAllMatches(line, rf.CommandList);
                        if (!String.IsNullOrEmpty(alteredLine)) sw.Write(alteredLine);
                    }
                    sr.Close();
                    TimeSpan ts = DateTime.Now - startParse;
                    logger.Info("File {0} found {1} matches on {2} input lines [{3:d} miliseconds]"
                                , filename, _countOfMatchesInFile, _lineNumber, ts.Milliseconds);
                }
            } catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
           return sw.Close();
        }

        public string ApplyCommandsAllMatches(string line, List<Command> commandList) {
            logger.Trace("ApplyCommandsAllMatches before:{0}", line);
            foreach (Command command in commandList) {
                logger.Trace("   ApplyCommandsAllMatches - applying '{0}' --> '{1}'  AnchorString:'{2}'", command.SubjectString.ToString(), command.ReplacementString, command.AnchorString);
                logger.Trace("   ApplyCommandsAllMatches - line before:'{0}'", line);
                if (isCandidateForReplacement(line, command)) {
                    CollectPickupValues(line, command);
                    if (command.Style != Command.CommandType.Pickup) {
                        line = ApplySingleCommand(line, command);
                    }
                }
                logger.Trace("   ApplyCommandsAllMatches - line  after:'{0}'",line);
            }
            logger.Trace("ApplyCommandsAllMatches  after:'{0}'", line);
            return line;
        }
    }
}
