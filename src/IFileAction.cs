using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kgrep {
    public interface IFileAction {
        string ApplyCommandsToInputFiles(ParseCommandFile rf, List<string> inputFilenames);
    }
}
