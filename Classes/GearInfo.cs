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

        private static ReadOnlyCollection<Equipment> test;
       
        public static ReadOnlyCollection<Equipment> ItemsTest
        {
            get
            {
                if (test is null)
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceName = "RemnantMultipurposeManager.Resources.RemnantItemIndex2.json";

                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        test = JsonConvert.DeserializeObject<List<Equipment>>(reader.ReadToEnd(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }).AsReadOnly();
                    }
                }
                return test;
            }
        }
        public static Equipment DefaultEquipment(Equipment.SlotType st)
        {
            return ItemsTest.Where(x => x.Slot == st &&x.Name.Contains("_")).FirstOrDefault();
        }
        public static Equipment FindEquipmentByName(string s)
        {
            return ItemsTest.Where(x=>x.Name==s).FirstOrDefault();
        }
    }
}

