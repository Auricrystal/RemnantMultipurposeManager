using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;
using static RemnantBuildRandomizer.DataObj;
using static RemnantBuildRandomizer.RemnantItem;

namespace RemnantBuildRandomizer
{
    class GearInfo
    {


        //public static event EventHandler<GearInfoUpdateEventArgs> GearInfoUpdate;
        public static Dictionary<SlotType, List<RemnantItem>> GearList = new Dictionary<SlotType, List<RemnantItem>>();
        public static Dictionary<string, RemnantItem> reflist = new Dictionary<string, RemnantItem>();
        private static Dictionary<int, List<Build>> presets;

        public static Dictionary<string, SlotType> Slots = new Dictionary<string, SlotType>() {
            {"Chest",SlotType.CH },
            {"Head",SlotType.HE },
            {"Legs",SlotType.LE },
            {"BossHand",SlotType.HG },
            {"RegHand",SlotType.HG },
            {"BossLong",SlotType.LG },
            {"RegLong",SlotType.LG },
            {"Melee",SlotType.M },
            {"Amulets",SlotType.AM },
            {"Rings",SlotType.RI },
            {"RegularMods",SlotType.MO },
            {"LongMod",SlotType.MO },
            {"HandMod",SlotType.MO },
        };
        private static readonly XmlDocument doc = new XmlDocument();

        //public static List<Build> presets = new List<Build>();
        private static List<Item> items = new List<Item>();

        private static Dictionary<string, string> archetypes = new Dictionary<string, string>() {
            {"Undefined","Undefined" },
            {"Scrapper","Scrapper" },
            {"Cultist","Cultist" },
            {"Hunter","Hunter" },
        };

        public static List<Item> Items
        {
            get
            {

                return items;
            }
        }


        public static Dictionary<string, string> Archetypes
        {
            get
            {

                return archetypes;
            }
        }

        public static Dictionary<int, List<Build>> Presets
        {
            get
            {
                if (presets == null)
                {
                    presets = new Dictionary<int, List<Build>>();
                }

                if (presets.Values.Count == 0)
                {
                    foreach (RemnantCharacter rc in MainWindow.ActiveSave.Characters)
                    {
                        presets.Add(rc.charNum, new List<Build>());
                    }
                }
                return presets;

            }
        }

        



        public static void getBlacklist()
        {
            string path =MainWindow.BackupDirPath + @"/Data.txt";
            List<RemnantCharacter> chars = MainWindow.ActiveSave.Characters;
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    foreach (RemnantCharacter rc in MainWindow.ActiveSave.Characters)
                    {
                        Debug.WriteLine(rc.charNum + " has" + Presets[rc.charNum].Count + " Builds");
                        foreach (Build b in Presets[rc.charNum])
                        {
                            sw.WriteLine("$:" + rc.charNum + ":" + b);
                        }
                    }
                    foreach (RemnantItem ri in reflist.Values)
                    {
                        if (ri.disabled.Count == 0)
                        {
                            ri.missing.Clear();
                            ri.disabled.Clear();
                            for (int i = 0; i < MainWindow.ActiveSave.Characters.Count; i++)
                            {
                                ri.disabled.Add(false);
                                ri.missing.Add(false);
                            }
                        }
                        sw.WriteLine("#:" + ri.ToData());
                    }
                }
            }


            // Open the file to read from.
            using (StreamReader sr = File.OpenText(path))
            {

                string s = "";
                while ((s = sr.ReadLine()) != null)
                {

                    string[] args = s.Split(':');
                    switch (args[0].ToCharArray()[0])
                    {
                        case '$':
                            Build b = new Build(args[2], args[3]);
                            b.Disabled = (int.Parse(args[4]) == 1);
                            Presets[int.Parse(args[1])].Add(b);
                            break;
                        case '#':
                            string[] sown = args[2].Split('|');
                            List<bool> owned = new List<bool>();
                            string[] sdis = args[3].Split('|');
                            List<bool> disabled = new List<bool>();
                            for (int i = 0; i < chars.Count; i++)
                            {
                                owned.Add((int.Parse(sown[i]) == 1));
                                disabled.Add((int.Parse(sdis[i]) == 1));
                            }
                            reflist[args[1]].missing = owned;
                            reflist[args[1]].disabled = disabled;
                            break;
                    }
                }
                foreach (RemnantCharacter rc in chars)
                {
                    MainWindow.SlogMessage(rc + " Has " + Presets[rc.charNum].Count + " Builds");
                }

            }
        }

        public static void ReadXML()
        {
            GearList.Clear();
            Stream s = typeof(GearInfo).Assembly.GetManifestResourceStream("RemnantBuildRandomizer.Resources.GearInfo.xml");
            doc.Load(s);

            parseItems("RegularMods");
            parseItems("LongMod");
            parseItems("HandMod");
            parseItems("Head");
            parseItems("Chest");
            parseItems("Legs");
            parseItems("RegHand");
            parseItems("BossHand");
            parseItems("RegLong");
            parseItems("BossLong");
            parseItems("Melee");
            parseItems("Amulets");
            parseItems("Rings");
        }
        public static void parseItems(string tag)
        {
            List<RemnantItem> list = new List<RemnantItem>();
            foreach (XmlElement xe in doc.GetElementsByTagName(tag))
            {
                RemnantItem ri = new RemnantItem(XmlElementExtension.GetXPath(xe).Replace("/GearInfo", ""), xe.GetAttribute("desc"), Slots[tag]);
                if (xe.InnerText != null)
                {
                    Item rItem = new Item(xe.InnerText);
                    rItem.ItemAltName = ri.Itemname;
                    items.Add(rItem);
                }
                ri.Mod = xe.GetAttribute("mod");
                list.Add(ri);
                reflist.Add(ri.Itemname, ri);
            }
            SlotType st = Slots[tag];
            if (GearList.ContainsKey(st)) { GearList[st].AddRange(list); } else { GearList[st] = list; }
        }


        /*
        public static void CheckForNewGameInfo()
        {
            GearInfoUpdateEventArgs args = new GearInfoUpdateEventArgs();
            try
            {
                WebClient client = new WebClient();
                client.DownloadFile("https://raw.githubusercontent.com/Auricrystal/RemnantBuildRandomizer/master/Resources/GearInfo.xml", "TempGearInfo.xml");

                XmlTextReader reader = new XmlTextReader("TempGearInfo.xml");
                reader.WhitespaceHandling = WhitespaceHandling.None;
                int remoteversion = 0;
                int localversion = 0;
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name.Equals("GearInfo"))
                        {
                            remoteversion = int.Parse(reader.GetAttribute("version"));
                            break;
                        }
                    }
                }
                args.RemoteVersion = remoteversion;
                reader.Close();
                Stream s = typeof(GearInfo).Assembly.GetManifestResourceStream("RemnantBuildRandomizer.Resources.GearInfo.xml");

                reader = new XmlTextReader(s);
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name.Equals("GearInfo"))
                        {
                            localversion = int.Parse(reader.GetAttribute("version"));
                            break;
                        }
                    }
                }
                reader.Close();
                args.LocalVersion = localversion;

                if (remoteversion > localversion)
                {
                    File.Delete("Resources/GearInfo.xml");
                    File.Move("TempGearInfo.xml", "Resources/GearInfo.xml");
                    ReadXML();
                    args.Result = GearInfoUpdateResult.Updated;
                    args.Message = "Gear info updated from v" + localversion + " to v" + remoteversion + ".";
                }
                else
                {
                    File.Delete("TempGearInfo.xml");
                }
            }
            catch (Exception ex)
            {
                args.Result = GearInfoUpdateResult.Failed;
                args.Message = "Error checking for new game info: " + ex.Message;
            }

            OnGameInfoUpdate(args);

        }
        

        protected static void OnGameInfoUpdate(GearInfoUpdateEventArgs e)
        {
            EventHandler<GearInfoUpdateEventArgs> handler = GearInfoUpdate;
            handler?.Invoke(typeof(GearInfo), e);
        }
    }

    public class GearInfoUpdateEventArgs : EventArgs
    {
        public int LocalVersion { get; set; }
        public int RemoteVersion { get; set; }
        public string Message { get; set; }
        public GearInfoUpdateResult Result { get; set; }

        public GearInfoUpdateEventArgs()
        {
            this.LocalVersion = 0;
            this.RemoteVersion = 0;
            this.Message = "No new game info found.";
            this.Result = GearInfoUpdateResult.NoUpdate;
        }
    }

    public enum GearInfoUpdateResult
    {
        Updated,
        Failed,
        NoUpdate
    }
        */
    }
}

