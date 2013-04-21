using System.IO;

namespace kgrep {

    // Return the appropiate IHandleInput to parse the input source.
    public class ReadFileFactory {
        public ReadFileFactory() {}

        public IHandleInput GetSource(string inputSource) {
            if (File.Exists(inputSource))  // kgrep replacement inuptSource
                return new ReadFile(inputSource);
            else if (inputSource == "stdin?")  
                return new ReadStdin();
            else
               return new ReadCommandLineArgumentAsFile(inputSource);  // reading command line data as if a Replacement file
        }
    }
}
