using System.IO;
using kgrep;

namespace kgrep {

    // Return the appropiate IHandleInput to parse the input source.
    public class ReplaceTokensFactory {
        public ReplaceTokensFactory() { }

        public ReplaceTokens GetReplaceEngine(bool replaceAll) {
            if (replaceAll) 
                return new ReplaceTokensAllOccurrences();
            else 
                return new ReplaceTokensFirstOccurrence();
        }
    }
}
