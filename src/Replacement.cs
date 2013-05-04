using System;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using NLog;

namespace kgrep
{
    public class Replacement
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [XmlIgnore] 
        public String anchor = "";
        public String topattern = null;
        public Regex frompattern;
        public Style style;
        public string ScannerFS = "\n";

        public enum Style {
            Scan,
            WithAnchor,
            Normal
        }
        public Replacement() {}

        public Replacement(string anchor, string frompattern, string topattern) {
            _replacement(anchor, frompattern, topattern);
            style = Style.WithAnchor;
        }

        public Replacement(string frompattern, string topattern) {
            _replacement("", frompattern, topattern);
            style = Style.Normal;
        }

        public Replacement(string scanpattern) {
            _replacement("", scanpattern, "");
            style = Style.Scan;
        }

        private void _replacement(string arganchor, string argfrompattern, string argtopattern) {
            try {
                logger.Trace("   _replacement - anchor:{0} frompattern:{1} topattern:{2}", arganchor, argfrompattern, argtopattern);
                anchor = RemoveEnclosingQuotesIfPresent(arganchor.Trim());
                string frompat = RemoveEnclosingQuotesIfPresent(argfrompattern.Trim());
                frompattern = new Regex(frompat, RegexOptions.Compiled);
                topattern = RemoveEnclosingQuotesIfPresent(argtopattern.Trim());

                // Just validation here
                Regex topat = new Regex(topattern);
                Regex anc = new Regex(anchor);
            } catch (Exception e) {
                Console.WriteLine("Regex error Replacement, from '{0}'  to '{1}'  anchor '{2}'", argfrompattern, argtopattern, anchor);
                throw new Exception(e.Message);
            }
        }

        private string RemoveEnclosingQuotesIfPresent(string pattern) {
            string pat = pattern.Trim();
            if (pattern.StartsWith("\"") && pattern.EndsWith("\"")) {
                string patWithoutQuotes = pat.Substring(1, pat.Length - 2);
                logger.Trace("Removed quotes ({0} --> {1})", pattern, patWithoutQuotes);
                return patWithoutQuotes;
            }
            return pattern; // return the original string untouched
        }
    }
}
