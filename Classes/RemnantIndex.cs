using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemnantMultipurposeManager
{
    public static class RemnantIndex
    {
        private static List<string> Tags { get; set; }

        public static string HexToTags(string b)
        {
            try
            {
                if (Tags?.Count == 0 | Tags == null) { return ""; }
                if (b.Length == 2) { return Tags[Convert.ToInt32(b.Substring(0, 2), 16)]; }
                return Tags[Convert.ToInt32(b.Substring(0, 2), 16)] + ", " + HexToTags(b.Substring(2));
            }
            catch (Exception) {
                Debug.WriteLine("Remnant Index Problem");
                return ""; }
        }
        public static void setIndex(string path)
        {
            Tags = File.ReadAllLines(path).ToList();
        }
    }
}
