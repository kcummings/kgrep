using System;
using System.IO;

namespace kgrep
{
    class HandleInput
    {
        StreamReader sr = null;       

        public HandleInput(string filename) {

            // Either read a file or stdin.
            // If filename doesn't exist, assuming reading from stdin.
            if (File.Exists(filename)) {
                sr = File.OpenText(filename);
            }
        }

        // Read in a line from the appropiate stream.
        public string ReadLine() {
            if (sr == null)
                return Console.ReadLine();
            return sr.ReadLine();
        }

        public void Close() {
            if (sr != null)
                sr.Close();
        }
    }
}
