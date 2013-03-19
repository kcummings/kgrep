using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace kgrep
{
    public class Replacement
    {
        [XmlIgnore] public String criteria = null;
      //  public String pattern = null;
        public String topattern = null;
        public Regex fromPattern;

        public Replacement() {}

        public Replacement(string pcriteria, string pfromPattern, string ptoPattern) {
            criteria = pcriteria;
            //pattern = ppattern.Trim();
            fromPattern = new Regex(pfromPattern.Trim(), RegexOptions.Compiled);
            topattern = ptoPattern.Trim();
        }
    }
}
