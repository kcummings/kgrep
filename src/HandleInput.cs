using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

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

        /// <summary>
        /// Read entire input stream into a string with newlines
        /// so can later regex on multi-lines.
        /// </summary>
        /// <returns></returns>
        public string ReadToEnd() {
            StringBuilder sb = new StringBuilder();
            string line;
            while ((line = ReadLine()) != null) {
                sb.Append(line);
                sb.Append('\n');
            }
            return sb.ToString();
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
