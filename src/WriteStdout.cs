using System;

namespace kgrep {
    public class WriteStdout : IHandleOutput {

        public void Write(string line) {
            Console.WriteLine(line);
        }
            
        public string Close() {
            return "";
        }
    }
}
