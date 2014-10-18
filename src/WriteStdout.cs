using System;

namespace kgrep {
    public class WriteStdout : IHandleOutput {

        public void Write(string line) {
            if (!String.IsNullOrEmpty(line))
                Console.WriteLine(line);
        }
            
        public string Close() {
            return "";
        }
    }
}
