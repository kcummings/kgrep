using System;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace kgrep
{
    public class Replacement
    {
        [XmlIgnore] public String Criteria = null;
      //  public String pattern = null;
        public String topattern = null;
        public Regex frompattern;

        public Replacement() {}

        public Replacement(string pCriteria, string pFromPattern, string pToPattern) {
            try {
                Criteria = pCriteria;
                //pattern = ppattern.Trim();
                frompattern = new Regex(pFromPattern.Trim(), RegexOptions.Compiled);
                topattern = pToPattern.Trim();

                // Just validation here
                Regex topat = new Regex(pToPattern.Trim());
                Regex anchor = new Regex(pCriteria.Trim());
            }
            catch (Exception e) {
                Console.WriteLine("Regex error Replacement, from '{0}'  to '{1}'  anchor '{2}'",pFromPattern,pToPattern,pCriteria);
                Console.WriteLine( e.GetType());
                throw new Exception(e.Message);
            }
        }
    }
}
