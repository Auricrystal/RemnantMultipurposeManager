using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;

namespace RemnantMultipurposeManager
{
    public class RemnantProfile
    {

        public List<RemnantCharacter> Characters { get; protected set; }
        public Dictionary<int, int> SavePair { get; }
        public Dictionary<int, List<Build>> Builds { get; }
        public Dictionary<int, List<int>> Blacklists { get; }
        public RemnantProfile(string read)
        {
            try
            {
                Characters = RemnantCharacter.GenerateCharacters(read);
                SavePair = new Dictionary<int, int>() { { 0, 0 }, { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } };
                Builds = new Dictionary<int, List<Build>>() { { 0, new List<Build>() }, { 1, new List<Build>() }, { 2, new List<Build>() }, { 3, new List<Build>() }, { 4, new List<Build>() } };
                Blacklists = new Dictionary<int, List<int>>() { { 0, new List<int>() }, { 1, new List<int>() }, { 2, new List<int>() }, { 3, new List<int>() }, { 4, new List<int>() } };

            }
            catch (Exception)
            {
                MainWindow.MW.LogMessage("Error Generating Profile", MainWindow.LogType.Error);
                Characters = new List<RemnantCharacter>();
                SavePair = new Dictionary<int, int>();
                Builds = new Dictionary<int, List<Build>>();
                Blacklists = new Dictionary<int, List<int>>();
            }
        }
        [JsonConstructor]
        private RemnantProfile()
        {
            Characters = new List<RemnantCharacter>();
            SavePair = new Dictionary<int, int>() { { 0, 0 }, { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 } };
            Builds = new Dictionary<int, List<Build>>() { { 0, new List<Build>() }, { 1, new List<Build>() }, { 2, new List<Build>() }, { 3, new List<Build>() }, { 4, new List<Build>() } };
            Blacklists = new Dictionary<int, List<int>>() { { 0, new List<int>() }, { 1, new List<int>() }, { 2, new List<int>() }, { 3, new List<int>() }, { 4, new List<int>() } };
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

        public void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            }));
        }
        public static RemnantProfile Load(string path)
        {
            return JsonConvert.DeserializeObject<RemnantProfile>(File.ReadAllText(path));
        }
    }
}

