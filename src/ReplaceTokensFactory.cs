using System;
using System.IO;
using kgrep;

namespace kgrep {

    // Return the appropiate IHandleInput to parse the input source.
    public class ReplaceTokensFactory {
        public ReplaceTokensFactory() { }

        public ReplaceTokens GetReplaceEngine(ParseCommandFile.RunMode runas) {
            switch (runas) {
                case ParseCommandFile.RunMode.ReplaceAll:   
                    return new ReplaceAllMatches();
                case ParseCommandFile.RunMode.ReplaceFirst:
                    return new ReplaceFirstMatch();
                default:
                    throw new Exception("No valid object for ReplaceTokensFactory");
            }
        }
    }
}
