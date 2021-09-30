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
        public SlotType Slot { get; set; }
        public string Name { get; set; }
        public string IMG { get; set; }
        public string File { get; set; }
        public InventoryItem Mod { get; set; }

        public bool Equals(InventoryItem b)
        {
            if (Name.Equals(b.Name)) { return true; }
            return false;
        }
        public string Index()
        {
            return GearInfo.Items.Except(GearInfo.Items.Empties()).ToList().IndexOf(this).ToString("X");
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
            return new InventoryItem { File = this.File, IMG = this.IMG, Name = this.Name, Mod = this.Mod, Slot = this.Slot };
        }
    }


    public class Build
    {

        public InventoryItem HandGun, LongGun, Melee, Head, Chest, Legs, Amulet, Ring1, Ring2;

        public Build(
            InventoryItem handGun = null,
            InventoryItem longGun = null,
            InventoryItem melee = null,
            InventoryItem head = null,
            InventoryItem chest = null,
            InventoryItem legs = null,
            InventoryItem amulet = null,
            InventoryItem ring1 = null,
            InventoryItem ring2 = null)
        {
            HandGun = handGun ?? GearInfo.GetEmpty(HG);
            LongGun = longGun ?? GearInfo.GetEmpty(LG);
            Melee = melee ?? GearInfo.GetEmpty(M);
            Head = head ?? GearInfo.GetEmpty(HE);
            Chest = chest ?? GearInfo.GetEmpty(CH);
            Legs = legs ?? GearInfo.GetEmpty(LE);
            Amulet = amulet ?? GearInfo.GetEmpty(AM);
            Ring1 = ring1 ?? GearInfo.GetEmpty(RI);
            Ring2 = ring2 ?? GearInfo.GetEmpty(RI);
        }


        public List<InventoryItem> ToInventory()
        {
            return new List<InventoryItem>() { HandGun, HandGun?.Mod, LongGun, LongGun?.Mod, Melee, Head, Chest, Legs, Amulet, Ring1, Ring2, }.Where(x => x != null && !x.Name.Contains("_")).ToList();
        }
        public string Code()
        {
            return string.Join("", ToInventory().Select(x => x.Index()));
        }

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
