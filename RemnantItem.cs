using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static RemnantBuildRandomizer.RemnantItem;
using static RemnantBuildRandomizer.GearInfo;
using System.Text.RegularExpressions;
using static RemnantBuildRandomizer.DataObj;

namespace RemnantBuildRandomizer
{
    public class RemnantItem : IEquatable<RemnantItem>
    {

        private string itemname;
        private string description;
        private string mod;
        private DataObj data;
        private int character;
        public List<Boolean> missing;
        public List<Boolean> disabled;


        public RemnantItem(string path, string description, SlotType slot)
        {
            this.Itemname = path.Substring(path.LastIndexOf("/") + 1).Replace(".png", "");
            this.Description = description;
            int ID;
            switch (Itemname)
            {
                case "_No Hand Gun":
                case "_No Long Gun":
                case "_Fists":
                case "_No Head":
                case "_No Chest":
                case "_No Legs":
                case "_No Amulet":
                case "_No Mod":
                case "_No Ring": ID = 0; Index = 1; break;
                default: ID = Index++; break;
            }
            this.Data = new DataObj(path, slot, ID);
            this.missing = new List<bool>();

            this.disabled = new List<bool>();
            this.Character = 0;
        }
        public bool Missing { get => missing[character]; set => missing[character] = value; }
        public bool Disabled { get => disabled[character]; set => disabled[character] = value; }
        public SlotType Slot { get => Data.Slot;}
        public string Itemname { get => itemname; set => itemname = value; }
        public string Description { get => description; set => description = value; }
        public string Mod { get => mod; set => mod = value; }
        public DataObj Data { get => data; set => data = value; }
        public int Character { get => character; set => character = value; }

        private string listToString(List<bool> l, char sep)
        {
            string s = "";
            for (int i = 0; i < l.Count - 1; i++)
            {
                s = s + (l[i] ? 1 : 0) + sep;
            }
            s = s + (l[l.Count - 1] ? 1 : 0);
            return s;
        }


        public override string ToString()
        {
            return Itemname;
        }
        public string ToData() {
            return Itemname + ":" + listToString(missing, '|') + ":" + listToString(disabled, '|');
        }
        public bool Equals(RemnantItem b)
        {
            if (this.itemname.Equals(b.Itemname)) { return true; }
            return false;

        }
    }
    public class DataObj
    {

        public enum SlotType { HG, LG, M, HE, CH, LE, AM, RI, MO };
        private BitmapImage image;
        private SlotType slot;
        static private int index = 0;
        private int id;

        public DataObj(string path, SlotType st, int ID)
        {
            this.SetImage(path);
            this.Slot = st;
            this.ID = ID;
        }

        //public string Path { get => path; set => path = value; }
        public BitmapImage GetImage()
        {
            return image;
        }

        public void SetImage(string value)
        {
            this.image = new BitmapImage(new Uri("pack://application:,,,/Resources/IMG" + value, UriKind.RelativeOrAbsolute));

        }
        public SlotType Slot { get => slot; set => slot = value; }
        public int ID { get => id; set => id = value; }
        public static int Index { get => index; set => index = value; }
    }

    public class Build : IEquatable<Build>
    {
        public RemnantItem hg;
        public RemnantItem hgm;
        public RemnantItem lg;
        public RemnantItem lgm;
        public RemnantItem m;
        public RemnantItem he;
        public RemnantItem ch;
        public RemnantItem le;
        public RemnantItem am;
        public RemnantItem r1;
        public RemnantItem r2;

        private string name;
        private string code;
        private bool disabled;

        public bool Disabled { get => disabled; set => disabled = value; }
        public string BuildName { get => name; set => name = value; }
        public string Code { get => code; set => code = value; }

        public void init(int[] arr)
        {
            string code = "";
            foreach (int i in arr)
            {
                code += i + "-";
            }
            code = code.Remove(code.Length - 1, 1);
            init(code);
        }
        public void init(string code)
        {
            string[] arr = code.Split('-');
            int index = 0;
            init(FindItem(SlotType.HG, arr[index++]),
            FindItem(SlotType.MO, arr[index++]),
            FindItem(SlotType.LG, arr[index++]),
            FindItem(SlotType.MO, arr[index++]),
            FindItem(SlotType.M, arr[index++]),
            FindItem(SlotType.HE, arr[index++]),
            FindItem(SlotType.CH, arr[index++]),
            FindItem(SlotType.LE, arr[index++]),
            FindItem(SlotType.AM, arr[index++]),
            FindItem(SlotType.RI, arr[index++]),
            FindItem(SlotType.RI, arr[index++]));
        }
        public void init(RemnantItem hg, RemnantItem hgm, RemnantItem lg, RemnantItem lgm, RemnantItem m, RemnantItem he, RemnantItem ch, RemnantItem le, RemnantItem am, RemnantItem r1, RemnantItem r2)
        {
            this.hg = hg;
            this.hgm = hgm;
            this.lg = lg;
            this.lgm = lgm;
            this.m = m;
            this.he = he;
            this.ch = ch;
            this.le = le;
            this.am = am;
            this.r1 = r1;
            this.r2 = r2;
        }

        public Build(string name, RemnantItem hg, RemnantItem hgm, RemnantItem lg, RemnantItem lgm, RemnantItem m, RemnantItem he, RemnantItem ch, RemnantItem le, RemnantItem am, RemnantItem r1, RemnantItem r2)
        {
            this.Disabled = false;
            this.BuildName = name;
            try
            {
                init(hg, hgm, lg, lgm, m, he, ch, le, am, r1, r2);
            }
            catch (Exception)
            {
                init("0-0-0-0-0-0-0-0-0-0-0");
            }
            Code = this.toCode();
        }


        public Build(string name, int[] arr)
        {

            this.Disabled = false;
            this.BuildName = name;
            try
            {
                init(arr);
            }
            catch (Exception)
            {
                init("0-0-0-0-0-0-0-0-0-0-0");
            }
            Code = this.toCode();

        }
        public Build(string name, string code)
        {
            this.Disabled = false;
            this.BuildName = name;
            try
            {
                init(code);
            }
            catch (Exception)
            {
                init("0-0-0-0-0-0-0-0-0-0-0");
            }
            Code = this.toCode();

        }
        public RemnantItem FindItem(SlotType st, string index)
        {
            int val = int.Parse(index);
            return FindItem(st, val);
        }
        public RemnantItem FindItem(SlotType st, int index)
        {
            try
            {
                return GearList[st][index];
            }
            catch (ArgumentOutOfRangeException)
            {

                int val = MainWindow.rd.Next(GearList[st].Count);
                Debug.WriteLine("OutOfRangeException: " + index + " Choosing random instead:" + val);
                return GearList[st][val];
            }
        }
        public string toCode()
        {
            string code = "";
            //code += BuildName+":";
            code += hg.Data.ID + "-";
            code += hgm.Data.ID + "-";
            code += lg.Data.ID + "-";
            code += lgm.Data.ID + "-";
            code += m.Data.ID + "-";
            code += he.Data.ID + "-";
            code += ch.Data.ID + "-";
            code += le.Data.ID + "-";
            code += am.Data.ID + "-";
            code += r1.Data.ID + "-";
            code += r2.Data.ID;

            return code;
        }

        public bool Equals(Build b)
        {
            if (this.Code.Equals(b.Code)) { return true; }
            return false;

        }

        public override string ToString()
        {
            return BuildName + ":" + Code + ":" + (Disabled ? 1 : 0);
        }
    }

    public class Item : IEquatable<Object>, IComparable
    {
        public enum Type { Uncat, Weapon, Armor, Trinket, Mod, };
        private string itemKey;
        private Type itemType;

        private string itemName;
        private string itemAltName;
        private string ItemKey
        {
            get { return itemKey; }
            set
            {
                try
                {
                    itemKey = value;
                    itemType = Type.Uncat;
                    itemName = itemKey.Substring(itemKey.LastIndexOf('/') + 1);
                    if (itemKey.Contains("/Weapons/"))
                    {
                        itemType = Type.Weapon;
                        if (itemName.Contains("Mod_")) itemName = itemName.Replace("/Weapons/", "/Mods/");
                    }
                    if (itemKey.Contains("/Armor/") || itemKey.Contains("TwistedMask"))
                    {
                        itemType = Type.Armor;
                        if (itemKey.Contains("TwistedMask"))
                        {
                            itemName = "TwistedMask (Head)";
                        }
                        else
                        {
                            string[] parts = itemName.Split('_');
                            itemName = parts[2] + " (" + parts[1] + ")";
                        }
                    }
                    if (itemKey.Contains("/Trinkets/") || itemKey.Contains("BrabusPocketWatch")) itemType = Type.Trinket;
                    if (itemKey.Contains("/Mods/")) itemType = Type.Mod;
                    itemName = itemName.Replace("Weapon_", "").Replace("Root_", "").Replace("Wasteland_", "").Replace("Swamp_", "").Replace("Pan_", "").Replace("Atoll_", "").Replace("Mod_", "").Replace("Trinket_", "").Replace("Trait_", "").Replace("Quest_", "").Replace("Emote_", "").Replace("Rural_", "").Replace("Snow_", "");
                    if (!itemType.Equals("Armor"))
                    {
                        itemName = Regex.Replace(itemName, "([a-z])([A-Z])", "$1 $2");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing item name: " + ex.Message);
                    itemName = value;
                }
            }
        }

        public string ItemName
        {
            get
            {
                if (itemAltName != null) return itemAltName;
                return itemName;
            }
        }
        public Type ItemType { get { return itemType; } }

        public string ItemNotes { get; set; }
        public string ItemAltName { get { return itemAltName; } set { itemAltName = value; } }

        public Item(string key)
        {
            this.ItemKey = key;
            this.ItemNotes = "";
        }

        public string GetKey()
        {
            return this.itemKey;
        }

        public override string ToString()
        {
            return itemType + ": " + ItemName;
        }

        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null))
            {
                return false;
            }
            else if (!this.GetType().Equals(obj.GetType()))
            {
                if (obj.GetType() == typeof(string))
                {
                    return (this.GetKey().Equals(obj));
                }
                return false;
            }
            else
            {
                Item rItem = (Item)obj;
                return (this.GetKey().Equals(rItem.GetKey()));
            }
        }

        public override int GetHashCode()
        {
            return this.itemKey.GetHashCode();
        }

        public int CompareTo(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null))
            {
                return 1;
            }
            else if (!this.GetType().Equals(obj.GetType()))
            {
                if (obj.GetType() == typeof(string))
                {
                    return (this.GetKey().CompareTo(obj));
                }
                return this.ToString().CompareTo(obj.ToString());
            }
            else
            {
                Item rItem = (Item)obj;
                return this.itemKey.CompareTo(rItem.GetKey());
            }
        }
    }


}

