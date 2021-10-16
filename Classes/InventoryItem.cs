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
using static RemnantMultipurposeManager.InventoryItem;
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
        public string Name { get; set; }
        public Dictionary<SlotType, List<int?>> Data { get; set; }

        [JsonConstructor]
        public Build(string name = null, Dictionary<SlotType, List<int?>> items = null)
        {
            this.Name = name ?? "";
            this.Data = items ?? new Dictionary<SlotType, List<int?>>();
        }
        public Build(string name, params List<int?>[] items)
        {
            this.Data = new Dictionary<SlotType, List<int?>>();
            this.Name = name;
            foreach(List<int?> t in items)
            {
                InventoryItem ii = GearInfo.GetItem(t?[0]);
                if (!Data.ContainsKey(ii.Slot))
                    Data.Add(ii.Slot, t);
            }
        }

        public List<int> getItems() {
            var list = new List<int>();
            foreach(List<int?> t in Data.Values)
            {
                list.AddRange(t.ToList().Where(x=>x!=null).Select(x=>x.Value));
            }
            return list;
        }
        public void TryRemove(SlotType st) {
            if (Data.ContainsKey(st))
                Data[st].Clear();
        
        }

        public override string ToString()
        {
            return string.Join(", ",getItems().Select(x=>GearInfo.GetItem(x).Name));
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
