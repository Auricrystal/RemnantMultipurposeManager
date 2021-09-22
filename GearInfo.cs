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

        //private static Dictionary<string, string> archetypes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) { { "Undefined", "Undefined" }, { "Scrapper", "Scrapper" }, { "Cultist", "Cultist" }, { "Hunter", "Hunter" } };

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
        public static InventoryItem getItem(string s) {
            return items.Find(x => x.Name == s);
        }
        public static List<InventoryItem> getItems(params string[] str)
        {
            List<InventoryItem> list = new List<InventoryItem>();
            foreach (string s in str) { list.Add(items.Find(x=>x.Name==s)); }
            return list;
        }
        public static BitmapImage GetImage(ZipArchive za, string value)
        {
            BitmapImage bmp = null;
            var entry = za.GetEntry(value);
            if (entry != null)
            {
                using (var zipStream = entry.Open())
                using (var memoryStream = new MemoryStream())
                {
                    zipStream.CopyTo(memoryStream); // here
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

}

