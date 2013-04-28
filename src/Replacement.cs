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
                logger.Trace("   _replacement - anchor:{0} frompattern:{1} topattern:{2}",arganchor, argfrompattern, argtopattern);
                anchor = arganchor.Trim();
                frompattern = new Regex(argfrompattern.Trim(), RegexOptions.Compiled);
                topattern = argtopattern.Trim().Replace(@"\s", " ");  // allow \s to represent a space in to pattern

                // Just validation here
                Regex topat = new Regex(topattern);
                Regex anc = new Regex(anchor); 
            } catch (Exception e) {
                Console.WriteLine("Regex error Replacement, from '{0}'  to '{1}'  anchor '{2}'", argfrompattern, argtopattern, anchor);
                throw new Exception(e.Message);
            }
        }
    }
}
