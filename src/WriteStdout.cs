using System;

namespace kgrep {
    public class WriteStdout : IHandleOutput {

        public void Write(string line) {
            if (line.EndsWith("\n")) 
               Console.Write(line);
            else
               Console.Write(line);
        }
            
        public string Close() {
            return "";
        }
    }
}
