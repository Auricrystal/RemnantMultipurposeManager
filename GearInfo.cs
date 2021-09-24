using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Media.Imaging;

namespace RemnantBuildRandomizer
{
    public class GearInfo
    {
        private static List<InventoryItem> items = new List<InventoryItem>();

        public static List<InventoryItem> Items
        {
            get
            {
                if (items.Count == 0) { 
                    items = JsonConvert.DeserializeObject<List<InventoryItem>>(File.ReadAllText(@"C:\Users\AuriCrystal\Documents\VisualProjects\RemnantBuildRandomizer\Resources\RemnantItemIndex.txt"));
                }
                return items;
            }
        }
        public static InventoryItem GetEmpty(InventoryItem.SlotType st)
        {
            return GearInfo.Items.Empties().Where(x => x.Slot == st).FirstOrDefault();
        }
        public static InventoryItem GetItem(string s) {
            return items.Find(x => x.Name == s);
        }
        public static List<InventoryItem> GetItems(params string[] str)
        {
            List<InventoryItem> list = new List<InventoryItem>();
            foreach (string s in str) { list.Add(items.Find(x=>x.Name==s)); }
            return list;
        }
        
    }

}

