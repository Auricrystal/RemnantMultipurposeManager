using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO.Compression;

namespace RemnantMultipurposeManager
{
    public class RemnantProfile
    {
        public string Name { get; set; }
        public List<RemnantCharacter> Characters { get; protected set; }
        public Dictionary<int, int> SavePair { get; }
        public Dictionary<int, List<Build>> Builds { get; }
        public Dictionary<int, List<int>> Blacklists { get; }
        public byte[] Data { get; protected set; }

        

        public RemnantProfile(string read)
        {
            try
            {
                PackProfile(read);
                Characters = RemnantCharacter.GenerateCharacters(read);
                SavePair = new Dictionary<int, int>() { { 0, 0 }, { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } };
                Builds = new Dictionary<int, List<Build>>() { { 0, new List<Build>() }, { 1, new List<Build>() }, { 2, new List<Build>() }, { 3, new List<Build>() }, { 4, new List<Build>() } };
                Blacklists = new Dictionary<int, List<int>>() { { 0, new List<int>() }, { 1, new List<int>() }, { 2, new List<int>() }, { 3, new List<int>() }, { 4, new List<int>() } };

            }
            catch (Exception)
            {
                MainWindow.MW.LogMessage("Error Generating Profile", MainWindow.LogType.Error);
                Data = null;
                Characters = new List<RemnantCharacter>();
                SavePair = new Dictionary<int, int>();
                Builds = new Dictionary<int, List<Build>>();
                Blacklists = new Dictionary<int, List<int>>();
            }
        }
        [JsonConstructor]
        private RemnantProfile(List<RemnantCharacter> characters, Dictionary<int, int> savePair, Dictionary<int, List<Build>> builds, Dictionary<int, List<int>> blacklists, byte[] data)
        {
            Characters = characters ?? new List<RemnantCharacter>();
            SavePair = savePair ?? new Dictionary<int, int>() { { 0, 0 }, { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } };
            Builds = builds ?? new Dictionary<int, List<Build>>() { { 0, new List<Build>() }, { 1, new List<Build>() }, { 2, new List<Build>() }, { 3, new List<Build>() }, { 4, new List<Build>() } };
            Blacklists = blacklists ?? new Dictionary<int, List<int>>() { { 0, new List<int>() }, { 1, new List<int>() }, { 2, new List<int>() }, { 3, new List<int>() }, { 4, new List<int>() } };
            Data = data;
        }

        public void UpdateCharacters(string path)
        {
            try
            {
                Characters = RemnantCharacter.GenerateCharacters(path);
            }
            catch (FileNotFoundException)
            {
                MainWindow.MW.LogMessage("Error Updating Profile", MainWindow.LogType.Error);
            }
        }
        public void PackProfile(string path)
        {
            Debug.WriteLine("Pack Profile: "+path);
            Data=File.ReadAllBytes(path);
        }
        public void UnpackProfile(string path)
        {
            File.WriteAllBytes(path, Data);
        }

        public void Save(string path)
        {
            Debug.WriteLine("Save Profile: " + path);

            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling=TypeNameHandling.Auto
            }));
        }
        public static RemnantProfile Load(string path)
        {

            var profile = JsonConvert.DeserializeObject<RemnantProfile>(File.ReadAllText(path), new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.Auto});
            
            
            return profile;
        }
    }
}

