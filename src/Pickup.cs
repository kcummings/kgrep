using System;
using System.Text.RegularExpressions;

namespace kgrep {
    public class Pickup {

        public String PickupName = null;
        public Regex PickupPattern = null;
        public  String PickupValue = null;

        public Pickup() {}

        public Pickup(string name, string pattern) {
            try {
                PickupName = name;
                PickupPattern = new Regex(pattern.Trim(), RegexOptions.Compiled);
                PickupValue = null;
            }
            catch (Exception e) {
                Console.WriteLine("Regex error Pickup, name '{0}'  pattern '{1}'",name,pattern);
                throw new Exception(e.Message);
            }
        }
    }
}
