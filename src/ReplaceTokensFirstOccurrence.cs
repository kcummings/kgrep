﻿using System;
using System.Collections.Generic;

namespace kgrep {

    public class ReplaceTokensFirstOccurrence : ReplaceTokens{

        override public string ApplyCommands(ParseCommandFile rf, List<string> inputFilenames) {
            try {
                string line;
                string alteredLine;

                foreach (string filename in inputFilenames) {
                    logger.Debug("Replace First Match - Processing input file:{0}", filename);
                    IHandleInput sr = (new ReadFileFactory()).GetSource((filename));
                    _lineNumber = 0;
                    _countOfMatchesInFile = 0;
                    while ((line = sr.ReadLine()) != null) {
                        _lineNumber++;
                        if (rf.ScopeAll)
                            alteredLine = ApplyCommandsAllMatches(line, rf.CommandList);
                        else
                            alteredLine = ApplyCommandsFirstMatch(line, rf.CommandList);
                        if (!String.IsNullOrEmpty(alteredLine)) sw.Write(alteredLine);
                    }
                    sr.Close();
                    logger.Info("File {0} found {1} matches on {2} input lines", filename, _countOfMatchesInFile, _lineNumber);
                }
            } catch (Exception e) {
                Console.WriteLine("{0}", e.Message);
            }
           return sw.Close();
        }

        // "scope=First" in effect.
        public string ApplyCommandsFirstMatch(string line, List<Command> commandList) {
            logger.Trace("ApplyCommandsFirstMatch before:{0}", line);
            foreach (Command command in commandList) {
                logger.Trace("   ApplyCommandsFirstMatch - ({0} --> {1})  AnchorString:{2}", command.SubjectString.ToString(), command.ReplacementString, command.AnchorString);
                if (isCandidateForReplacement(line, command)) {
                    if (command.SubjectString.IsMatch(line)) {
                        CollectPickupValues(line, command);
                        if (command.Style != Command.CommandType.Pickup) {
                            line = ApplySingleCommand(line, command);
                            break;
                        }
                    }
                }
            }
            logger.Trace("ApplyCommandsFirstMatch  after:{0}", line);
            return line;
        }

        // "scope=All" in effect.
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
