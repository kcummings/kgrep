using System;
using System.Collections.Generic;
using System.Text;
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
            Criteria = pCriteria;
            //pattern = ppattern.Trim();
            frompattern = new Regex(pFromPattern.Trim(), RegexOptions.Compiled);
            topattern = pToPattern.Trim();
        }
    }
}
