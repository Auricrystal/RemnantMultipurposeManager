using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemnantBuildRandomizer
{
    class JWorldSave
    {
        //Important info: Boss/Event/Vendor, Planet, Modifiers,  Difficulty
        public string Type { get; set; }//Boss
        public string World { get; set; }//Corsus
        public string Name { get; set; }//BarbedTerror
        public string Difficulty { get; set; }//Apocalypse
        public Dictionary<string,byte[]> File { get; set; }//sav.sav
    }
}
