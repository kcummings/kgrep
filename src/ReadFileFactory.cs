using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace kgrep {

    // Return the appropiate IHandleInput to parse the input source.
    public class ReadFileFactory {
        public ReadFileFactory() {}

        public IHandleInput GetSource(string inputSource) {
            if (inputSource.Contains("~"))  // assume one ~ implies command line argument given as a Replacement command
                return new ReadCommandLineArgumentAsFile(inputSource);
            else if (File.Exists(inputSource))
                return new ReadFile(inputSource);
            else
                return new ReadStdin();
        }
    }
}
