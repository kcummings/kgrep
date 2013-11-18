using System;
using System.IO;
using kgrep;

namespace kgrep {

    // Return the appropiate IHandleInput to parse the input source.
    public class FileActionFactory {
        public FileActionFactory() { }

        public IFileAction GetFileAction(ParseCommandFile.RunMode runas) {
            switch (runas) {
                case ParseCommandFile.RunMode.ReplaceAll:   
                    return new ReplaceAllMatches();
                case ParseCommandFile.RunMode.ReplaceFirst:
                    return new ReplaceFirstMatch();
                case ParseCommandFile.RunMode.Scanner:
                    return new PrintTokensInSourceFiles();
                default:
                    throw new Exception("No valid object for FileActionFactory");
            }
        }
    }
}
