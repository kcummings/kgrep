using System;
using System.IO;
using kgrep;

namespace kgrep {

    // Return the appropiate IHandleInput to parse the input source.
    public class FileActionFactory {
        public FileActionFactory() { }

        public IFileAction GetFileAction(ParseCommandFile.RunningAs runas) {
            switch (runas) {
                case ParseCommandFile.RunningAs.Scanner:
                    return new PrintTokensInSourceFiles();
                default:
                    return new ReplaceTokens();
            }
        }
    }
}
