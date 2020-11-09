using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;
using static RemnantBuildRandomizer.MyExtensions;

namespace RemnantBuildRandomizer
{
    public class WorldSave
    {

        public static Dictionary<string, WorldSave> getSave = new Dictionary<string, WorldSave>();
        public string path;
        private string diff = "";
        private string world = "";
        private string name = "";
        private string[] modifiers = { "", "" };
        private string description = "";
        private static readonly Dictionary<string, string> AdventureMode = new Dictionary<string, string>() {
            { "Quest_AdventureMode_City_C", "Earth"},
            { "Quest_AdventureMode_Wasteland_C", "Rhom"},
            { "Quest_AdventureMode_Swamp_C", "Corsus"},
            { "Quest_AdventureMode_Jungle_C", "Yaesha"},
            { "Quest_AdventureMode_Snow_C", "Reisum"}};

        private static readonly Dictionary<string, string> CampaignMode = new Dictionary<string, string>() {
            { "/Game/Campaign_Main/Quest_Campaign_Main.Quest_Campaign_Main_C", "Earth"},
            { "Quest_AdventureMode_Wasteland_C", "Rhom"},
            { "Quest_AdventureMode_Swamp_C", "Corsus"},
            { "Quest_AdventureMode_Jungle_C", "Yaesha"},
            { "Quest_AdventureMode_Snow_C", "Reisum"}};
        private static readonly Dictionary<string, string> Difficulty = new Dictionary<string, string>() {
            { "Apocalypse", "Apoc"},
            { "Normal", "Norm"},
            { "Hard", "Hard"},
            { "Nightmare", "Night"} };
        public string Diff { get => diff; set => diff = value; }
        public string World { get => world; set => world = value; }
        public string Name { get => name; set => name = value; }
        public string Modifiers
        {
            get
            {
                return string.Join("\n", modifiers);
            }
            set { modifiers = value.Split(' '); }

        }
        public string Description { get => description; set => description = value; }



        private static Dictionary<char, string> bossModifiers;
        public class CharComparer : IEqualityComparer<char>
        {
            public bool Equals(char c1, char c2)
            {
                return char.ToLowerInvariant(c1) == char.ToLowerInvariant(c2);
            }
            public int GetHashCode(char c1)
            {
                return char.ToLowerInvariant(c1).GetHashCode();
            }

        }
        private static Dictionary<char, string> BossModifiers
        {
            //Hearty,Regenerator,Vicious,Skullcracker,Enchanter,World,None
            get
            {
                if (bossModifiers == null)
                {
                    bossModifiers = new Dictionary<char, string>(new CharComparer())
                    {
                        {'H',"Hearty"},{'R',"Regenerator"},{'V',"Vicious"},{'S',"Skullcracker"},{'E',"Enchanter"},{'W',"World"},{'N',"None"}
                    };
                }
                return bossModifiers;
            }
        }



        public WorldSave(string path, string diff, string world, string name, params string[] m)
        {
            this.path = path;
            this.Diff = diff;
            this.World = world;
            this.Name = name;
            this.modifiers = m;
        }
        public static void addWS(WorldSave ws)
        {

            if (!getSave.ContainsKey(ws.path))
            {
                //Debug.WriteLine("ADDWORLD:"+ws.ToData());
                try { getSave.Add(ws.path, ws); } catch (Exception ex) { Debug.WriteLine(ex.Message); }
            }

        }

        public static WorldSave Parse(string file)
        {
            if (!file.Contains(".sav")) { throw new Exception("Invalid Format"); }
            else
            {
                string localfile = file.Split(new char[] { '\\', '|', '/' }).Last();
                string[] name = localfile.Replace(".sav", "").Split('_');
                WorldSave rb;


                string world = FindWorld(new FileStream(file, FileMode.Open));

                string diff = FindDiff(new FileStream(file, FileMode.Open));

                rb = new WorldSave(file, diff, world, name.First(), name.Skip(1).ToArray());
                WorldSave.addWS(rb);

                return rb;
            }
        }
        public static string FindDiff(string parse)
        {
            string diff = "";
            foreach (string s in new string[] { "Apocalypse", "Normal", "Hard", "Nightmare" }) { if (parse.Contains(s)) { diff = Difficulty[s]; break; } }
            return diff;
        }

        public static string FindWorld(string parse)
        {

            string adventureZone = "Ward13";
            foreach (string s in AdventureMode.Keys.Reverse()) { if (parse.Contains(s)) { adventureZone = AdventureMode[s]; break; } }
            return adventureZone;
        }

        public static string FindDiff(Stream st)
        {
            string diff = "";
            using (StreamReader sr = new StreamReader(st))
            {
                diff = FindDiff(sr.ReadToEnd());
            }

            return diff;
        }

        public static string FindWorld(Stream st)
        {

            Dictionary<string, string> world = new Dictionary<string, string>();
            world.Add("City", "Earth");
            world.Add("Wasteland", "Rhom");
            world.Add("Swamp", "Corsus");
            world.Add("Jungle", "Yaesha");
            world.Add("Snow", "Reisum");

            string adventureZone = "Ward13";
            using (StreamReader sr = new StreamReader(st))
            {
                Match m = new Regex(@"Quest_AdventureMode_(?<world>\w*)_C").Match(sr.ReadToEnd());
                string match;
                if ((match = m.Groups["world"].Value) != "")
                {
                    adventureZone = world[match];
                }

            }
            st.Dispose();
            return adventureZone;
        }

        public List<string> ReadFile()
        {
            
         
            if (this.path.Contains(".zip"))
            {
                using (ZipArchive zip = ZipFile.Open(path.Split('|').First(), ZipArchiveMode.Update))
                {
                  return parseData(zip.GetEntry(path.Split('|').Last()).Open());
                }
            }
            else
            {
              return parseData(new FileStream(this.path, FileMode.Open));
            }
            

        }
        private List<string> parseData(Stream s) {
            List<string> data = new List<string>();
            using (StreamReader sr = new StreamReader(s))
            {
                Regex r = new Regex(@"Quests\/(?<Test>\w*\/\w*)");
                MatchCollection mc = r.Matches(sr.ReadToEnd());
                int i = 0;
                foreach (Match m in mc)
                {
                    string test;
                    if (!data.Contains((test = m.Groups["Test"].Value))) {data.Add(test); }
                }
            }
            return data;
        }
        public string fullpath()
        {
            string filename;
            if (path.Contains(".zip"))
            {
                filename = path.Split('|').Last().Split('/').Last();
                path = path.Replace(filename, Filename() + ".sav");
                return path;
            }
            filename = path.Split('\\').Last();
            path = path.Replace(filename, Filename() + ".sav");
            return path;
        }

        public string Filename()
        {
            return Name + "_" + string.Join("_", this.modifiers);
        }

        public string ToData()
        {
            return string.Join(":", this.path + this.Description);
        }
        public void FromData(string s)
        {
            string[] d = s.Split(':');
            WorldSave ws;
            if ((ws = getSave[d.First()]) != null) { ws.Description = d.Last(); }
        }

        public bool Contains(string s)
        {
            s = s.ToLower();
            return Name.ToLower().Contains(s) || World.ToLower().Contains(s) || Diff.ToLower().Contains(s) || Modifiers.ToLower().Contains(s) || Description.ToLower().Contains(s);
        }

        public bool Contains(params string[] st)
        {
            foreach (string s in st) { if (!Contains(s)) { return false; } }
            return true;
        }
    }
}
