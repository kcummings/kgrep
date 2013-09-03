using System;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using NLog;

namespace kgrep
{
    public class Replacement {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [XmlIgnore] public String AnchorPattern = "";
        public String ToPattern = null;
        public Regex FromPattern;
        public ReplacementType Style;
        public string ScannerFS = "\n";
        public int PickupCount = 0;
        public int PickupPlaceholderCount = 0;

        public enum ReplacementType {
            Scan,
            WithAnchor,
            Normal
        }

        public Replacement() {
        }

        public Replacement(string anchor, string fromPattern, string toPattern) {
            _replacement(anchor, fromPattern, toPattern);
            Style = ReplacementType.WithAnchor;
        }

        public Replacement(string fromPattern, string toPattern) {
            _replacement("", fromPattern, toPattern);
            Style = ReplacementType.Normal;
        }

        public Replacement(string scanPattern) {
            _replacement("", scanPattern, "");
            Style = ReplacementType.Scan;
        }

        private void _replacement(string anchor, string fromPattern, string toPattern) {
            try {
                logger.Trace("   _replacement - AnchorPattern:'{0}' FromPattern:'{1}' ToPattern:'{2}'", anchor, fromPattern,
                             toPattern);
                AnchorPattern = RemoveEnclosingQuotesIfPresent(anchor.Trim());
                fromPattern = RemoveEnclosingQuotesIfPresent(fromPattern.Trim());
                PickupCount = GetPickupCount(@"\(\?<.+?>.+?\)", fromPattern);  // how many named group captures are present?
                PickupPlaceholderCount = GetPickupCount(@"\$\{.+?\}", toPattern);
                FromPattern = new Regex(fromPattern, RegexOptions.Compiled);
                ToPattern = RemoveEnclosingQuotesIfPresent(toPattern.Trim());

                // Just validation here. Will the given pattern throw an exception?
                Regex topat = new Regex(ToPattern);
                Regex anc = new Regex(AnchorPattern);
            }
            catch (Exception e) {
                Console.WriteLine("Regex error Replacement, from '{0}'  to '{1}'  AnchorPattern '{2}'", fromPattern,
                                  toPattern, AnchorPattern);
                throw new Exception(e.Message);
            }
        }

        private string RemoveEnclosingQuotesIfPresent(string pattern) {
            string pat = pattern.Trim();
            if (pattern.StartsWith("\"") && pattern.EndsWith("\"")) {
                string patternWithoutQuotes = pat.Substring(1, pat.Length - 2);
                logger.Trace("Removed quotes ({0} --> {1})", pattern, patternWithoutQuotes);
                return patternWithoutQuotes;
            }
            return pattern; // return the original string untouched
        }

        private static int GetPickupCount(string pattern, string line) {
            try {
               Regex regex = new Regex(pattern);
               return regex.Matches(line).Count;
            }
            catch(Exception e)
            {
                logger.Debug(String.Format("GetPickupCount - Looking for '{0}' in '{1}' \n{2}",pattern,line,e.Message));
                return 0;
            }
        }
    }
}
