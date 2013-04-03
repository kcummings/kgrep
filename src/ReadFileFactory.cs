﻿using System.IO;

namespace kgrep {

    // Return the appropiate IHandleInput to parse the input source.
    public class ReadFileFactory {
        public ReadFileFactory() {}

        public IHandleInput GetSource(string inputSource) {
            if (inputSource.Contains("~") || inputSource.Contains("delim="))  // reading command line data as if a Replacement file
                return new ReadCommandLineArgumentAsFile(inputSource);
            else if (File.Exists(inputSource))  // kgrep replacement inuptSource
                return new ReadFile(inputSource);
            else if (inputSource.Length > 0)  
                return new ReadCommandLineArgumentAsFile(inputSource);  // reading command line data as if a text file; used for unit testing
            else
                return new ReadStdin();
        }
    }
}
