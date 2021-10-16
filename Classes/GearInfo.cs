using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Media.Imaging;

using System.Reflection;

namespace RemnantMultipurposeManager
{
    public class GearInfo
    {
        private static List<InventoryItem> items = new List<InventoryItem>();

        public static List<InventoryItem> Items
        {
            get
            {
                if (items.Count == 0)
                {

                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceName = "RemnantMultipurposeManager.Resources.RemnantItemIndex.json";

                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        items = JsonConvert.DeserializeObject<List<InventoryItem>>(reader.ReadToEnd());
                    }
                }
                return items;
            }


        }
        public static InventoryItem GetEmpty(InventoryItem.SlotType st)
        {
            return Items.Empties().Where(x => x.Slot == st).FirstOrDefault();
        }
        public static InventoryItem GetItem(string s)
        {
            return items.Find(x => x.Name == s);
        }
        public static InventoryItem GetItem(int? i)
        {
            if (i.HasValue) { return items[i.Value] ?? null; }
            return null;
        }
        
        public static List<InventoryItem> GetItems(params string[] str)
        {
            List<InventoryItem> list = new List<InventoryItem>();
            foreach (string s in str) { list.Add(items.Find(x => x.Name == s)); }
            return list;
        }
        public static List<InventoryItem> GetItems(params int[] items)
        {
            List<InventoryItem> list = new List<InventoryItem>();
            foreach (int i in items) { list.Add(Items[i]); }
            return list;
        }


    }

}

