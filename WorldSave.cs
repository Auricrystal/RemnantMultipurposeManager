using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

using System.Text.RegularExpressions;


namespace RemnantBuildRandomizer
{
    public class WorldSave
    {

        private static ConcurrentDictionary<string, WorldSave> getSave = new ConcurrentDictionary<string, WorldSave>();
        public string path;
        private string diff = "";
        private string world = "";
        private string name = "";
        private string modifiers = "";
        private string description = "";
        private static WorldData[] worldstuff =
        {
            new WorldData("Ward13","Ace"),
            new WorldData("Earth","Shroud Gorefist Brabus Mangler Riphide Ent Singe Dreamer MudTooth"),
            new WorldData("Rhom","AncientConstruct Scourge Maul Raze Shatter&Shade UndyingKing Claviger Harrow"),
            new WorldData("Corsus","Canker Thrall BarbedTerror Dreameater IskalQueen UncleanOne Ixillis GraveyardElf"),
            new WorldData("Yaesha","TheWarden Onslaught Stormcaller Scald&Sear RootHorror Ravager TotemFather StuckMerchant"),
            new WorldData("Reisum","Tian Obryk Ikro Erfor Brudvaak&Vargr Sebum"),
            new WorldData("WardPrime","Harsgaard")

        };
        private static string GetWorld(string name)
        {
            string output = "";
            foreach (WorldData w in worldstuff)
            {
                if ((output = w.getWorld(name)) != "") { break; }
            }
            return output;

        }
        public string Diff { get => diff; set => diff = value; }
        public string World { get => SplitByCapitalization(world); set => world = value; }

        public string Name
        {
            get
            {
                return SplitByCapitalization(name);
            }
            set => name = value;
        }
        public string SplitByCapitalization(string s)
        {
            string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
            return string.Join("\n", split);
        }
        public string Modifiers
        {
            get
            {
                while (modifiers.StartsWith("_")) { modifiers=modifiers.Remove(0,1); }
                return modifiers.Replace('_', '\n').Replace(' ', '\n');
            }
            set {
                string input = value;
                if (input.StartsWith("_")) { input = input.Remove(0,1); }
                modifiers = input.Replace("__","_");
            }

        }
        public string Description { get => description; set => description = value; }



        private static Dictionary<char, string> bossModifiers;
        private static Dictionary<char, string> DiffLevels;
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
        private static Dictionary<char, string> Difficulty
        {
            //Apocalypse,Nightmare,Hard,Normal
            get
            {
                if (DiffLevels == null)
                {
                    DiffLevels = new Dictionary<char, string>(new CharComparer())
                    {
                        {'A',"Apoc"},{'I',"Night"},{'H',"Hard"},{'N',"Norm"}
                    };
                }
                return DiffLevels;
            }
        }



        public static ConcurrentDictionary<string, WorldSave> GetSave
        {
            get
            {
                if (getSave == null) { getSave = new ConcurrentDictionary<string, WorldSave>(); }
                return getSave;
            }
            set => getSave = value;
        }

        public WorldSave(string path, string diff, string world, string name, string m, string desc)
        {
            this.path = path;
            this.Diff = diff;
            this.World = world;
            this.Name = name;
            this.Modifiers = m;
            this.Description = desc;
        }
        public static void addWS(WorldSave ws)
        {

            if (!GetSave.ContainsKey(ws.path))
            {
                try
                {
                    string path = ws.path;
                    WorldSave w = ws.Copy();
                    if (!GetSave.ContainsKey(path))
                    {
                        GetSave.TryAdd(path, w);
                    }
                    else
                    {
                        GetSave[path] = w;
                    }
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }
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
                string diff = "";
                var list = new string[] { "Apocalypse", "Nightmare", "Hard", "Normal" }.Where(x => file.Contains(x));
                if (list.Count() > 0) { diff = list.First(); }
                string world = GetWorld(name.First());
                Debug.WriteLine(file);
                rb = new WorldSave(file, diff, world, name.First(), string.Join(" ", name.Skip(1).ToArray()), "");
                addWS(rb);

                return rb;
            }
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
        private List<string> parseData(Stream s)
        {
            List<string> data = new List<string>();
            using (StreamReader sr = new StreamReader(s))
            {
                Regex r = new Regex(@"Quests\/(?<Test>\w*\/\w*)");
                MatchCollection mc = r.Matches(sr.ReadToEnd());
                int i = 0;
                foreach (Match m in mc)
                {
                    string test;
                    if (!data.Contains((test = m.Groups["Test"].Value))) { data.Add(test); }
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
            return Name.Replace(" &\n", "&").Replace("\n", "").Replace(" ", "") + "_" + this.Modifiers.Replace("\n", "_").Replace(" ", "_");
        }

        public string ToData()
        {
            string[] arr = Filename().Split('_');
            return string.Join(";", new string[] { path, diff, world, arr.First(), string.Join("_", arr.Skip(1)), description });
        }
        public static WorldSave FromData(string s)
        {
            string[] p = s.Split(';');
            WorldSave ws = new WorldSave(p[0], p[1], p[2], p[3], p[4], p[5]);
            return ws;
        }

        public bool Contains(string s)
        {
            s = s.ToLower();
            return Name.Replace(" &\n", "&").ToLower().Contains(s) || World.ToLower().Contains(s) || Diff.ToLower().Contains(s) || Modifiers.ToLower().Contains(s) || Description.ToLower().Contains(s);
        }

        public bool Contains(params string[] st)
        {
            foreach (string s in st) { if (!Contains(s)) { return false; } }
            return true;
        }
    }
    public class WorldData
    {
        private string input;
        private string output;
        public WorldData(string output, params string[] inputs)
        {
            this.input = string.Join(" ", inputs);
            this.output = output;
        }
        public string getWorld(string name)
        {
            if (input.Contains(name)) { return output; } else { return ""; }
        }


    }

}
