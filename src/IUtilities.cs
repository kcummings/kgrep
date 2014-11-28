using System.Collections.Generic;

namespace kgrep {
    public interface IUtilities {
        List<string> ExpandFileNameWildCards(string globPattern);
    }
}