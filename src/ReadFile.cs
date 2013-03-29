using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace kgrep {
    class ReadFile : IHandleInput {
        StreamReader sr = null;

        public ReadFile(string filename) {

            try {
                sr = File.OpenText(filename);
            }
            catch (Exception e) {
                throw (e);
            }
        }

        public string ReadLine() {
            if (sr == null) return null;
            return sr.ReadLine();
        }

        public void Close() {
            if (sr != null)
                sr.Close();
        }
    }
}
