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


        public BitmapImage GetImage

        {
            get
            {
                BitmapImage bmp = null;

                if (!System.IO.File.Exists(MainWindow.RBRDirPath + "\\IMG.zip"))
                    if (!MainWindow.DownloadZip("IMG"))
                        return new BitmapImage();

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

        public Dictionary<SlotType, object> Data { get; set; }

        [JsonConstructor]
        public Build(string name = null, Dictionary<SlotType, object> items = null)
        {
            this.Name = name ?? "";
            this.Data = items ?? new Dictionary<SlotType, object>();
        }
        public Build(string name, params object[] items)
        {
            this.Data = new Dictionary<SlotType, object>();
            this.Name = name;
            foreach (object t in items)
            {
                InventoryItem ii;
                if (t is int)
                {
                    ii = GearInfo.GetItem((int)t);
                    if (!Data.ContainsKey(ii.Slot))
                        Data.Add(ii.Slot, t);
                }
                if(t is List<int>)
                {
                    ii = GearInfo.GetItem(((List<int>)t)[0]);
                    if (!Data.ContainsKey(ii.Slot))
                        Data.Add(ii.Slot, t);
                }
            }
        }

        public List<int> getItems()
        {
            var list = new List<int>();
            foreach (object t in Data.Values)
            {
                //Debug.WriteLine("Test: " + (t is int) + (t is List<int>));
                if (t is int)
                    list.Add((int)t);
                if (t is List<int>)
                    list.AddRange(((List<int>)t).ToList());
            }
            //Debug.WriteLine("List Size: "+list.Count);
            return list;
        }
        public void TryRemove(SlotType st)
        {

            if (!Data.ContainsKey(st))
                return;

            Data.Remove(st);
        }


        public override string ToString()
        {
            //Debug.WriteLine("Build Count: "+getItems().Count);
            return string.Join(", ", getItems().Select(x => GearInfo.GetItem(x).Name));
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
