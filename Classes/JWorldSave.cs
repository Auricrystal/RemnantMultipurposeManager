using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace RemnantMultipurposeManager
{
    public class JWorldSave
    {
        //Important info: Boss/Event/Vendor, Planet, Modifiers,  Difficulty
        public enum SaveType { Unknown, Bosses, Events, Vendors };
        public enum WorldZone { Unknown, Earth, Rhom, Corsus, Yaesha, Reisum, Ward17, WardPrime, Labyrinth, Ward13 }
        public enum DifficultyType { Unknown, Normal, Hard, Nightmare, Apocalypse, Vendor }

        public string Name { get; private set; }//BarbedTerror

        public WorldZone World { get; private set; }//Corsus
        public SaveType Type { get; private set; }//Boss
        
        public Dictionary<DifficultyType, List<string>> Checkpoints { get; private set; }



        [JsonConstructor]
        public JWorldSave(string name, WorldZone world,SaveType type, Dictionary<DifficultyType, List<string>> checkpoints )
        { 
            Type = type;
            World = world;
            Checkpoints = checkpoints;
            Name = name ?? "Unknown";
        }

        public bool DownloadFile(string path, DifficultyType dt, string modifier)
        {
            using (WebClient client = new())
            {
                if (!Checkpoints[dt].Contains(modifier))
                {
                    MainWindow.MW.LogMessage("Modifier Not Found!", MainWindow.LogType.Error);
                    return false;
                }
                try
                {
                    Debug.WriteLine("URL: " +GetURL(dt, modifier));
                    client.DownloadFile(GetURL(dt, modifier), path);
                }
                catch (WebException we) { MainWindow.MW.LogMessage(we.Message, MainWindow.LogType.Error); return false; }
                return true;
            }
        }
        public string GetURL(DifficultyType dt, string modifier) {
            

            string zipURL = "https://raw.githubusercontent.com/Auricrystal/RemnantMultipurposeManager/master/Resources/";
            return zipURL + string.Join("/", Type, Type != SaveType.Vendors ? dt : "No Difficulty" , (World == JWorldSave.WorldZone.WardPrime) ? "Ward Prime" : World, Name, modifier, "save.sav");
             
        }

        public static void Save(string path, params JWorldSave[] worldSave)
        {
            string text = JsonConvert.SerializeObject(worldSave, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            File.WriteAllText(path, text);
        }
        public static JWorldSave Load(string path)
        {
            return JsonConvert.DeserializeObject<JWorldSave>(File.ReadAllText(path));
        }
        public static List<JWorldSave> LoadList(string path)
        {
            return JsonConvert.DeserializeObject<List<JWorldSave>>(File.ReadAllText(path));
        }

        public override string ToString()
        {
            string c = "\n";
            foreach (DifficultyType dt in Checkpoints.Keys)
            {
                c += dt.ToString() + ":\n (" + string.Join(",", Checkpoints[dt]) + ")\n";
            }

            return String.Format("Name:{0}\nSaveType:{1}\nCheckpoints:{2}\nWorldZone:{3}", Name, Type, c, World);
        }

        public override bool Equals(object obj)
        {
            return obj is JWorldSave save &&
                   Type == save.Type &&
                   World == save.World &&
                   Name == save.Name &&
                   EqualityComparer<Dictionary<DifficultyType, List<string>>>.Default.Equals(Checkpoints, save.Checkpoints);
        }

        public override int GetHashCode()
        {
            int hashCode = -442467166;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + World.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<DifficultyType, List<string>>>.Default.GetHashCode(Checkpoints);
            return hashCode;
        }
    }

}
