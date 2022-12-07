using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace RemnantMultipurposeManager
{
    class JWorldSave
    {
        //Important info: Boss/Event/Vendor, Planet, Modifiers,  Difficulty
        public enum SaveType { Unknown, Boss, Event, Vendor };
        public enum WorldZone { Unknown, Earth, Rhom, Corsus, Yaesha, Reisum, Ward17, WardPrime }
        public enum DifficultyType { Unknown, Normal, Hard, Nightmare, Apocalypse }


        public  SaveType Type { get; private set; }//Boss
        public WorldZone World { get; private set; }//Corsus
        public DifficultyType Difficulty { get; private set; }//Apocalypse
        public string Name { get; private set; }//BarbedTerror
        public Dictionary<string, byte[]> File { get; private set; }//sav.sav
        [JsonConstructor]
        public JWorldSave(SaveType type, WorldZone world, DifficultyType difficulty, string name, Dictionary<string, byte[]> file)
        {
            Type = type;
            World = world;
            Difficulty = difficulty;
            Name = name ?? "Unknown";
            File = file ?? new Dictionary<string, byte[]>();
        }


        //public JWorldSave(SaveType type, WorldZone world, DifficultyType difficulty, string name, Dictionary<string, byte[]> file)
        //{
        //    Type = type;
        //    World = world;
        //    Difficulty = difficulty;
        //    Name = name;
        //    File = file;
        //    Debug.WriteLine("JWorldSave Constructed!");
        //}



        public static void Save(JWorldSave worldSave, string path)
        {
            string text = JsonConvert.SerializeObject(worldSave, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            System.IO.File.WriteAllBytes(path,Encoding.ASCII.GetBytes(text).Compress() );
        }
        public static JWorldSave Load(string path)
        {
            byte[] arr = System.IO.File.ReadAllBytes(path).Decompress();
            return JsonConvert.DeserializeObject<JWorldSave>(System.Text.Encoding.Default.GetString(arr));
        }

        public override string ToString()
        {
            return String.Format("Name:{0}\nSaveType:{1}\nDifficulty:{2}\nWorldZone:{3}\nModifiers:{4}",Name, Type, Difficulty, World,string.Join(",",File.Keys));
        }
    }

}
