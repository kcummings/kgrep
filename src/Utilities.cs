using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace kgrep {
    public class Utilities : IUtilities {
        public Utilities() {
        }

        public List<string> ExpandFileNameWildCards(string globPattern) {
            DirectoryInfo di = new DirectoryInfo(".");
            FileInfo[] files = di.GetFiles(globPattern);
            return (from file in files select file.ToString()).ToList();
        }
    }
}