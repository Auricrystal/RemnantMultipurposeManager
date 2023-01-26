using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using static RemnantMultipurposeManager.MainWindow;


namespace RemnantMultipurposeManager
{

    public class WorldSave
    {
        public Guid Guid { get; private set; }
        public string Type { get; private set; }
        public string Name { get; private set; }
        public string World { get; private set; }
        public string Difficulty { get; private set; }
        public string Modifier { get; private set; }

        [JsonConstructor]
        public WorldSave(string type = "Type", string name = "BossName", string world = "World", string difficulty = "Difficulty", string modifier = "Modifier", Guid? guid = null)
        {
            Guid = guid ?? Guid.NewGuid();
            Type = type;
            Name = name;
            World = world;
            Difficulty = difficulty;
            Modifier = modifier;
        }

        public static List<WorldSave> LoadList(string path)
        {
            if (!File.Exists(path))
                File.Create(path);
            var list = JsonConvert.DeserializeObject<List<WorldSave>>(File.ReadAllText(path));

            return list;
        }

        public string GetURL()
        {
            return "https://raw.githubusercontent.com/Auricrystal/RemnantMultipurposeManager/" + MainWindow.Branch + "/Resources/SaveLibrary/" + Guid + ".sav";
        }

        public bool DownloadFile(string filedest)
        {
            using (WebClient client = new())
            {

                try
                {
                    MainWindow.Instance.LogMessage("Loading: " +Name+" | "+Modifier, LogType.Success);
                    client.DownloadFile(GetURL(), filedest);
                }
                catch (WebException we)
                {
                    if (we.Message.Contains("raw.githubusercontent.com"))
                        MainWindow.Instance.LogMessage("Trouble contacting database, check if online.", MainWindow.LogType.Error);
                    else
                        MainWindow.Instance.LogMessage("Unknown Online Error", MainWindow.LogType.Error);
                    return false;
                }
                return true;
            }
        }

        public void LocalLoad(string filedest)
        {
            MainWindow.Instance.LogMessage("Loading: " + Name + " | " + Modifier, LogType.Success);
            File.Copy(RmmInstallPath+@"\LocalSaves\"+Guid+".sav", filedest,true);
        }

        public void grabFileFromZip(string filedest)
        {
            Debug.WriteLine("Grabbing From Zip File!");
            using (ZipArchive zip = ZipFile.Open(RmmInstallPath + "\\SaveLibrary.zip", ZipArchiveMode.Read))
            {
                zip.GetEntry(Guid.ToString()).ExtractToFile(filedest, true);
            }
            MainWindow.Instance.LogMessage("Loading: " + Name + " | " + Modifier, LogType.Success);
        }
        public override bool Equals(object obj)
        {
            if (obj is not WorldSave)
                return false;
            return (obj as WorldSave).Guid == Guid;
        }
        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }


        public override string ToString()
        {
            return string.Format("SaveType:{0} Difficulty:{1} Name:{2} Modifier:{3} WorldZone:{4}", Type, Difficulty, Name, Modifier, World);
        }
    }

   
}
