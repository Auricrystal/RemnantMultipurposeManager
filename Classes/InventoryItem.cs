using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static RemnantMultipurposeManager.InventoryItem.SlotType;

namespace RemnantMultipurposeManager
{
    public class InventoryItem : IEquatable<InventoryItem>, IComparable, ICloneable
    {
        public enum SlotType { HG, LG, M, HE, CH, LE, AM, RI, MO };
        public int Index
        {
            get;
            set;

        }
        public SlotType Slot
        {
            get; set;
        }
        public string Name { get; set; }
        public string IMG { get; set; }
        public string File { get; set; }
        public bool Boss { get; set; }
        public int? ModIndex { get; set; }

        public bool Equals(InventoryItem b)
        {
            if (Name.Equals(b.Name)) { return true; }
            return false;
        }


        public BitmapImage GetImage()
        {
            BitmapImage bmp = null;
            var zip = ZipFile.OpenRead(MainWindow.RBRDirPath + "\\IMG.zip");
            var entry = IMG != null ? zip.GetEntry(IMG) : null;
            if (entry != null)
            {
                using (var zipStream = entry.Open())
                using (var memoryStream = new MemoryStream())
                {
                    zipStream.CopyTo(memoryStream);
                    memoryStream.Position = 0;

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = memoryStream;
                    bitmap.EndInit();

                    bmp = bitmap;
                }
            }
            return bmp;
        }
        public int CompareTo(object obj)
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
                    return Name.CompareTo(obj);
                }
                return ToString().CompareTo(obj.ToString());
            }
            else
            {
                InventoryItem rItem = (InventoryItem)obj;
                return this.Name.CompareTo(rItem.Name);
            }
        }

        public object Clone()
        {
            return new InventoryItem { File = this.File, IMG = this.IMG, Name = this.Name, ModIndex = this.ModIndex, Slot = this.Slot, Index = this.Index, Boss = this.Boss };
        }

        public override string ToString()
        {
            return String.Join(":", Index, Slot, Name, Boss);
        }
    }


    public class Build
    {
        //private string name;
        private int handGun;
        private int handGunMod;
        private int longGun;
        private int longGunMod;
        private int melee;
        private int head;
        private int chest;
        private int legs;
        private int amulet;
        private int ring1;
        private int ring2;
        public string Name { get; set; }
        public int HandGun { get => handGun; set => handGun = ((int?)value) ?? GearInfo.GetEmpty(HG).Index; }
        public int HandGunMod { get => handGunMod; set => handGunMod = ((int?)value) ?? GearInfo.GetEmpty(MO).Index; }
        public int LongGun { get => longGun; set => longGun = ((int?)value) ?? GearInfo.GetEmpty(LG).Index; }
        public int LongGunMod { get => longGunMod; set => longGunMod = ((int?)value) ?? GearInfo.GetEmpty(MO).Index; }
        public int Melee { get => melee; set => melee = ((int?)value) ?? GearInfo.GetEmpty(M).Index; }
        public int Head { get => head; set => head = ((int?)value) ?? GearInfo.GetEmpty(HE).Index; }
        public int Chest { get => chest; set => chest = ((int?)value) ?? GearInfo.GetEmpty(CH).Index; }
        public int Legs { get => legs; set => legs = ((int?)value) ?? GearInfo.GetEmpty(LE).Index; }
        public int Amulet { get => amulet; set => amulet = ((int?)value) ?? GearInfo.GetEmpty(AM).Index; }
        public int Ring1 { get => ring1; set => ring1 = ((int?)value) ?? GearInfo.GetEmpty(RI).Index; }
        public int Ring2 { get => ring2; set => ring2 = ((int?)value) ?? GearInfo.GetEmpty(RI).Index; }
        public Build(string Name = null,
            InventoryItem HandGun = null,
            InventoryItem HandGunMod = null,
            InventoryItem LongGun = null,
            InventoryItem LongGunMod = null,
            InventoryItem Melee = null,
            InventoryItem Head = null,
            InventoryItem Chest = null,
            InventoryItem Legs = null,
            InventoryItem Amulet = null,
            InventoryItem Ring1 = null,
            InventoryItem Ring2 = null)
            : this(Name, HandGun?.Index, HandGunMod?.Index,
                 LongGun?.Index, LongGunMod?.Index,
                 Melee?.Index,
                 Head?.Index, Chest?.Index, Legs?.Index,
                 Amulet?.Index, Ring1?.Index, Ring2?.Index)
        { }

        [JsonConstructor]
        public Build(string Name = null,
            int? HandGun = null,
            int? HandGunMod = null,
            int? LongGun = null,
            int? LongGunMod = null,
            int? Melee = null,
            int? Head = null,
            int? Chest = null,
            int? Legs = null,
            int? Amulet = null,
            int? Ring1 = null,
            int? Ring2 = null)
        {
            this.Name = Name;
            this.HandGun = HandGun.GetValueOrDefault();
            this.HandGunMod = HandGunMod.GetValueOrDefault();
            this.LongGun = LongGun.GetValueOrDefault();
            this.LongGunMod = LongGunMod.GetValueOrDefault();
            this.Melee = Melee.GetValueOrDefault();
            this.Head = Head.GetValueOrDefault();
            this.Chest = Chest.GetValueOrDefault();
            this.Legs = Legs.GetValueOrDefault();
            this.Amulet = Amulet.GetValueOrDefault();
            this.Ring1 = Ring1.GetValueOrDefault();
            this.Ring2 = Ring2.GetValueOrDefault();
        }
        public List<int> ToInventory() => new List<int>() { HandGun, HandGunMod, LongGun, LongGunMod, Melee, Head, Chest, Legs, Amulet, Ring1, Ring2 };
    }
    class InventoryItemComparer : IEqualityComparer<InventoryItem>
    {
        public bool Equals(InventoryItem b1, InventoryItem b2)
        {

            if (b2 == null && b1 == null)
                return true;
            else if (b1 == null || b2 == null)
                return false;
            else if (b1.Name == b2.Name)
                return true;
            else
                return false;
        }

        public int GetHashCode(InventoryItem bx)
        {
            int hCode = bx.Name.GetHashCode();
            return hCode.GetHashCode();
        }
    }
}
