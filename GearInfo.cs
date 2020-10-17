using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Xml;
using static RemnantBuildRandomizer.DataObj;
using static RemnantBuildRandomizer.RemnantItem;

namespace RemnantBuildRandomizer
{
    class GearInfo
    {
        private static Dictionary<string, RemnantItem> strToRI = new Dictionary<string, RemnantItem>();
        private static Dictionary<BitmapImage, RemnantItem> imgToRI = new Dictionary<BitmapImage, RemnantItem>();
        private static Dictionary<SlotType, List<RemnantItem>> getEquipment = new Dictionary<SlotType, List<RemnantItem>>() {
            {SlotType.HG,new List<RemnantItem>()},
            {SlotType.LG,new List<RemnantItem>()},
            {SlotType.M,new List<RemnantItem>()},
            {SlotType.HE,new List<RemnantItem>()},
            {SlotType.CH,new List<RemnantItem>()},
            {SlotType.LE,new List<RemnantItem>()},
            {SlotType.AM,new List<RemnantItem>()},
            {SlotType.RI,new List<RemnantItem>()},
            {SlotType.MO,new List<RemnantItem>()}
        };
        public static Dictionary<string, RemnantItem> StrToRI { get => strToRI; set => strToRI = value; }
        public static Dictionary<BitmapImage, RemnantItem> ImgToRI { get => imgToRI; set => imgToRI = value; }
        public static Dictionary<SlotType, List<RemnantItem>> GetEquipment { get => getEquipment; set => getEquipment = value; }

        private static Dictionary<int, List<Build>> presets;
        private static Dictionary<int, int> saveSlot;

        public static readonly Dictionary<string, SlotType> Slots = new Dictionary<string, SlotType>() {
            {"Chest",SlotType.CH },{"Head",SlotType.HE }, {"Legs",SlotType.LE },{"BossHand",SlotType.HG },
            {"RegHand",SlotType.HG },{"BossLong",SlotType.LG },{"RegLong",SlotType.LG },{"Melee",SlotType.M },
            {"Amulets",SlotType.AM },{"Rings",SlotType.RI }, {"RegularMods",SlotType.MO }, {"LongMod",SlotType.MO },
            {"HandMod",SlotType.MO },
        };

        private static readonly XmlDocument doc = new XmlDocument();


        private static List<Item> items = new List<Item>();

        private static Dictionary<string, string> archetypes = new Dictionary<string, string>() { { "Undefined", "Undefined" }, { "Scrapper", "Scrapper" }, { "Cultist", "Cultist" }, { "Hunter", "Hunter" } };

        public static List<Item> Items
        {
            get { return items; }
        }


        public static Dictionary<string, string> Archetypes
        {
            get { return archetypes; }
        }

        public static Dictionary<int, List<Build>> Presets
        {
            get
            {
                if (presets == null) { presets = new Dictionary<int, List<Build>>(); }
                if (presets.Values.Count == 0)
                {
                    if (File.Exists(MainWindow.SaveDirPath + "\\profile.sav"))
                    {
                        foreach (RemnantCharacter rc in MainWindow.ActiveSave.Characters) { presets.Add(rc.charNum, new List<Build>()); }
                    }
                    else { presets.Add(0, new List<Build>()); }
                }

                return presets;
            }
        }

        public static Dictionary<int, int> SaveSlot
        {
            get
            {
                if (saveSlot == null) { saveSlot = new Dictionary<int, int>(); }
                if (saveSlot.Values.Count == 0)
                {
                    foreach (RemnantCharacter rc in MainWindow.ActiveSave.Characters) { saveSlot.Add(rc.charNum, rc.charNum); }
                }
                return saveSlot;
            }
        }

        public static void CreateData(string path, List<RemnantCharacter> chars)
        {

            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    if (File.Exists(MainWindow.SaveDirPath + "\\profile.sav"))
                    {
                        foreach (RemnantCharacter rc in chars)
                        {

                            Debug.WriteLine(rc.charNum + " has" + Presets[rc.charNum].Count + " Builds");
                            foreach (Build b in Presets[rc.charNum])
                            {
                                sw.WriteLine("$:" + rc.charNum + ":" + b);
                            }
                        }


                    }
                    else
                    {
                        foreach (Build b in Presets[0])
                        {
                            sw.WriteLine("$:" + 0 + ":" + b);
                        }
                    }

                    foreach (RemnantItem ri in StrToRI.Values)
                    {
                        if (ri.disabled.Count == 0)
                        {
                            ri.missing.Clear();
                            ri.disabled.Clear();
                            for (int i = 0; i < chars.Count; i++)
                            {
                                ri.disabled.Add(false);
                                ri.missing.Add(false);
                            }
                        }
                        sw.WriteLine("#:" + ri.ToData());
                    }
                }
            }

        }
        public static void ReadData(string path, List<RemnantCharacter> chars)
        {
            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    //try{

                    string[] args = s.Split(':');
                    switch (args[0].ToCharArray()[0])
                    {
                        case '$':
                            Build b = new Build(args[2], args[3]);
                            b.Disabled = (int.Parse(args[4]) == 1);
                            Presets[int.Parse(args[1])].Add(b);
                            break;
                        case '#':
                            if (File.Exists(MainWindow.SaveDirPath + "\\profile.sav"))
                            {
                                string[] sown = args[2].Split('|');
                                List<bool> owned = new List<bool>();
                                string[] sdis = args[3].Split('|');
                                List<bool> disabled = new List<bool>();
                                for (int i = 0; i < chars.Count; i++)
                                {
                                    bool val;
                                    if (i < sown.Length) { val = (int.Parse(sown[i]) == 1); } else { val = false; }
                                    owned.Add(val);
                                    if (i < sdis.Length) { val = (int.Parse(sdis[i]) == 1); } else { val = false; }
                                    disabled.Add(val);
                                }
                                StrToRI[args[1]].missing = owned;
                                StrToRI[args[1]].disabled = disabled;
                            }
                            else
                            {
                                StrToRI[args[1]].missing = new List<bool>() { (int.Parse(args[2]) == 1) };
                                StrToRI[args[1]].disabled = new List<bool>() { (int.Parse(args[3]) == 1) };
                            }

                            break;
                    }

                    // }catch (Exception pd) { MainWindow.SlogMessage("Unknown Error While Parsing Data: " + pd.Message); }
                }

            }
        }
        public static void GetData()
        {
            string path = MainWindow.BackupDirPath + @"/Data.txt";

            List<RemnantCharacter> chars;
            if (File.Exists(MainWindow.SaveDirPath + "\\profile.sav"))
            {
                chars = MainWindow.ActiveSave.Characters;

            }
            else { chars = new List<RemnantCharacter>(); }
            CreateData(path, chars);
            ReadData(path, chars);
        }


        public static void ReadXML()
        {
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
            foreach (XmlElement xe in doc.GetElementsByTagName(tag))
            {
                RemnantItem ri = new RemnantItem(XmlElementExtension.GetXPath(xe).Replace("/GearInfo", ""), xe.GetAttribute("desc"), Slots[tag]);
                if (xe.InnerText != null && xe.InnerText != "")
                {

                    Item rItem = new Item(xe.InnerText);
                    rItem.ItemAltName = ri.Itemname;
                    items.Add(rItem);
                    // Debug.WriteLine("Adding: "+ri.Itemname+" Inner text: "+xe.InnerText);
                }
                ri.Mod = xe.GetAttribute("mod");
                StrToRI.Add(ri.Itemname, ri);
                ImgToRI.Add(ri.Data.GetImage(), ri);
                GetEquipment[Slots[tag]].Add(ri);
            }
        }
    }
}

