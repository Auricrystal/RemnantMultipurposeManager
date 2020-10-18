using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents.DocumentStructures;
namespace RemnantBuildRandomizer
{
    public class RemnantBoss
    {
        private static Dictionary<string, string> getWorld;


        private string name = "";
        private string m1 = "";
        private string m2 = "";
        private string d = "";
        private string w = "";
        public string Difficulty { get => d; set => d = value; }
        public string World { get => w; set => w = value; }
        public string Name { get => name; set => name = value; }
        public string Modifier1 { get => m1; set => m1 = value; }
        public string Modifier2 { get => m2; set => m2 = value; }
        private static Dictionary<string, string> GetWorld
        {
            get
            {
                if (getWorld == null)
                {
                    getWorld = new Dictionary<string, string>() {
                        //Earth
                        {"Ace" ,"Ward13"},
                        { "Ent","Earth" }, {"Singe","Earth" }, {"Brabus","Earth" }, {"Gorefist","Earth" }, {"Shroud","Earth" }, {"Riphide","Earth" }, {"Mangler","Earth" }, {"Dreamer","Earth" },
                        { "MudTooth","Earth" },
                        //Rhom
                        { "Claviger","Rhom" }, {"Harrow","Rhom" }, {"UndyingKing","Rhom" }, {"Scourge","Rhom" }, {"ShatterShade","Rhom" }, {"Raze","Rhom" }, {"AncientConstruct","Rhom" }, {"Maul","Rhom" },
                        //Corsus
                        { "Ixillis","Corsus" }, {"UncleanOne","Corsus" }, {"IskalQueen","Corsus" }, {"Thrall","Corsus" }, {"Dreameater","Corsus" }, {"Canker","Corsus" }, {"BarbedTerror","Corsus" },
                        { "GraveyardElf","Corsus" }, 
                        //Yaesha
                        { "TotemFather","Yaesha" }, {"Ravager","Yaesha" },{ "Stormcaller","Yaesha" }, {"ScaldSear","Yaesha" },{ "TheWarden","Yaesha" }, {"Onslaught","Yaesha" },
                        //Reisum
                        { "BrudvaakVargr","Reisum" }, {"Harsgaard","WardPrime" }, {"Tian","Reisum" },{ "Obryk","Reisum" },{ "Ikro","Reisum" }, {"Erfor","Reisum" },
                        { "Sebum","Reisum" }
                    };
                }
                return getWorld;
            }
            
            


        }
        public RemnantBoss(string w, string name, string d, string m1)
        {
            this.Name = name;
            this.Modifier1 = m1;
            this.Modifier2 = null;
            this.Difficulty = d;
            this.World = w;
        }
        public RemnantBoss(string w, string name, string d, string m1, string m2)
        {
            this.Name = name;
            this.Modifier1 = m1;
            this.Modifier2 = m2;
            this.Difficulty = d;
            this.World = w;
        }

        public static RemnantBoss FromFilename(string s)
        {
            s = s.Replace(".sav", "");
            string[] data = s.Split('_');
            string world;
            try { world = GetWorld[data[0]]; } catch (Exception) { world = ""; }
            if (data.Length > 3)
            {
                return new RemnantBoss(world, data[0], data[1], data[2], data[3]);
            }
            else
            {
                return new RemnantBoss(world, data[0], data[1], data[2]);
            }

        }

        public override string ToString()
        {
            string mods;
            if (Modifier2 != null) { mods = Modifier1 + "_" + Modifier2; } else { mods = Modifier1 + ""; }

            return Name + "_" + Difficulty + "_" + mods;
        }
        public bool Contains(string s)
        {
            s = s.ToLower();
            if (Modifier2 != null)
            {
                return (Name.ToLower().Contains(s) || World.ToLower().Contains(s) || Difficulty.ToLower().Contains(s) || Modifier1.ToLower().Contains(s) || Modifier2.ToLower().Contains(s));
            }
            else
            {
                return (Name.ToLower().Contains(s) || World.ToLower().Contains(s) || Difficulty.ToLower().Contains(s) || Modifier1.ToLower().Contains(s));
            }
        }

        public bool Contains(params string[] st)
        {
            foreach (string s in st) { if (!Contains(s)) { return false; } }
            return true;
        }


    }
}
