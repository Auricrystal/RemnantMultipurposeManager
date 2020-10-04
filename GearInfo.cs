using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using static RemnantBuildRandomizer.RemnantItem;

namespace RemnantBuildRandomizer
{
    class GearInfo
    {
        public static Dictionary<SlotType, List<RemnantItem>> GearList = new Dictionary<SlotType, List<RemnantItem>>();
        public static Dictionary<string, RemnantItem> reflist = new Dictionary<string, RemnantItem>();
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
        public static List<Build> presets = new List<Build>();
        public static void updateBlacklist() {
            string path = @"Resources/Blacklist.txt";
            File.Delete(path);
            getBlacklist();
        }


        public static void getBlacklist()
        {
            string path = @"Resources/Blacklist.txt";

            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    foreach (Build b in presets) {
                        sw.WriteLine("preset:" + b.Code);
                    }
                    foreach (RemnantItem ri in reflist.Values) {
                        sw.WriteLine(ri.Itemname+"="+ri.Disabled);
                    }
                }
            }
            

            // Open the file to read from.
            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    if (s.Contains(":"))
                    {
                        int pos = s.LastIndexOf(":");
                        presets.Add(Build.fromCode(s.Substring(pos+1)));
                    }
                    if (s.Contains("="))
                    {
                        int pos = s.LastIndexOf("=");
                        reflist[s.Substring(0, pos)].Disabled=bool.Parse(s.Substring(pos + 1));
                    }
                }
            }
        }


        public static void ReadXML()
        {
            GearList.Clear();
            doc.Load("Resources/GearInfo.xml");
            parseItems("RegularMods");
            parseItems("LongMod");
            parseItems("HandMod");
            parseItems("Chest");
            parseItems("Head");
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
            foreach (XmlElement  xe in doc.GetElementsByTagName(tag))
            {
                RemnantItem ri = new RemnantItem(XmlElementExtension.GetXPath(xe).Replace("/GearInfo", ""), xe.GetAttribute("desc"), Slots[tag]);
                ri.Mod = xe.GetAttribute("mod");
                ri.Dlc = xe.GetAttribute("DLC");
                list.Add(ri);
                if (ri.Slot == SlotType.HG || ri.Slot == SlotType.LG || ri.Slot == SlotType.M)
                {
                    ri.Description = ri.Itemname;
                }
                reflist.Add(ri.Itemname, ri);
            }
            SlotType st = Slots[tag];
            if (GearList.ContainsKey(st)) { GearList[st].AddRange(list); } else { GearList[st] = list; }
        }


    }

    
}
