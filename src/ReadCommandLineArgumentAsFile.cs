using System.Linq;
using System.Collections.Generic;

namespace kgrep {
    class ReadCommandLineArgumentAsFile : IHandleInput {
        private List<string> ParsedArguments = new List<string>();

        public ReadCommandLineArgumentAsFile(string commandLineArgument) {
            ParsedArguments = (from g in commandLineArgument.Split(';') select g).ToList();
        }

        public string ReadLine() {
            string first = null;
            if (ParsedArguments.Count > 0) {
                first = ParsedArguments[0];
                ParsedArguments.RemoveAt(0);
                return first;
            }
            return first;
        }

        public void Close() {
        }
    }
}
