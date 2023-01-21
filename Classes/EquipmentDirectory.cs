using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Media.Imaging;

using System.Reflection;
using System.Collections.ObjectModel;

namespace RemnantMultipurposeManager
{
    public class EquipmentDirectory
    {

        private static List<Equipment> _list;
       
        public static ReadOnlyCollection<Equipment> Items
        {
            get
            {
                if (_list is null)
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceName = "RemnantMultipurposeManager.Resources.RemnantItemIndex.json";

                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        _list = JsonConvert.DeserializeObject<List<Equipment>>(reader.ReadToEnd(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                    }
                }
                return _list.AsReadOnly();
            }
        }
        public static Equipment DefaultEquipment(Equipment.SlotType st)
        {
            return Items.Where(x => x.Slot == st &&x.Name.Contains("_")).FirstOrDefault();
        }
        public static Equipment FindEquipmentByName(string s)
        {
            return Items.FirstOrDefault(x=>x.Name==s);
        }
    }
}

