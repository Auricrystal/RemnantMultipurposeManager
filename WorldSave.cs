using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

using System.Text.RegularExpressions;


namespace RemnantBuildRandomizer
{
    public class WorldSave
    {
        private string filepath = "";
        private string diff = "";
        private string world = "";
        private string name = "";
        private string modifiers = "";


        public string Diff { get => diff; set { diff = (value != "") ? value : "Unknown"; } }
        public string World { get => world; set { world = (value != "") ? value : "Unknown"; } }
        public string Name { get => name; set { name = (value != "") ? value : "Unknown"; } }
        public string Modifiers { get => modifiers; set { modifiers = (value != "") ? value : "Unknown"; } }
        public string Filepath { get => filepath; set => filepath = value; }

        public string Data { get => filepath.Replace("save.sav", "data.txt"); }
        public WorldSave(string diff, string world, string name, string m, string path)
        {
            this.Diff = diff;
            this.Name = name;
            this.World = world;
            this.Modifiers = m;
            this.Filepath = path;
        }

        public static WorldSave ParseZip(string path)
        {
            if (!path.Contains(".zip")) { throw new Exception("Invalid Format"); }
            else
            {
                var file = path.Split('|').Last().Split('/').Skip(1).ToArray();
                return new WorldSave(file[0], file[1], file[2], file[3], path);
            }
        }
        public static WorldSave ParseFile(string path)
        {
            if (!File.Exists(path) || path.Contains(".zip")) { throw new Exception("Invalid Format"); }
            else
            {
                var file = path.Replace(MainWindow.MiscDirPath + "\\", "").Split('\\').ToArray();
                if (file.Length < 4)
                {
                    WorldSave ws = new WorldSave("Unknown", "Unknown", file.First().Replace(".sav", ""), "Unknown", path);
                    ws.FixPath();
                    return ws;
                }
                return new WorldSave(file[0], file[1], file[2], file[3], path);
            }
        }


        public static string ReadFile(string path)
        {
            try
            {
                if (path.Contains(".zip"))
                {
                    string text = "";

                    using (ZipArchive za = ZipFile.Open(path.Split('|').First(), ZipArchiveMode.Read))
                    {
                        StreamReader sr;
                        var s = za.GetEntry(path.Split('|').Last())?.Open();
                        if (s != null)
                        {
                            //Debug.WriteLine("ReadZip: " + path.Split('|').Last());
                            sr = new StreamReader(s);
                            text = sr.ReadToEnd();
                            sr.Close();
                        }
                        else { text = ""; }
                    }
                    return text;
                }
                else
                {
                    return File.ReadAllText(path);
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        public bool Contains(string s)
        {
            s = s.ToLower();
            return Name.ToLower().Contains(s) || World.ToLower().Contains(s) || Diff.ToLower().Contains(s) || Modifiers.ToLower().Contains(s);
        }

        public bool Contains(params string[] st)
        {
            foreach (string s in st) { if (!Contains(s)) { return false; } }
            return true;
        }

        public override string ToString()
        {
            return String.Join("|", Diff, Name, Modifiers);
        }

        public void MoveTo(string path)
        {

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.Move(Filepath, path);
                if (File.Exists(Data)) { File.Move(Data, path.Replace("save.sav", "data.txt")); }

                Filepath = path;
            }
            catch (Exception)
            {
                FixPath();
            }

        }
        public void FixPath()
        {
            int dupe = 0;
            string backup = "";
            while (File.Exists((backup = MainWindow.MiscDirPath + "\\Unknown\\Unknown\\Unknown\\" + Name + dupe + "\\save.sav"))) { dupe++; }
            Directory.CreateDirectory(Path.GetDirectoryName(backup));
            File.Move(Filepath, backup);
            Filepath = backup;
        }

        public string ToData()
        {

            return string.Join(";", new string[] { Diff, World, Name, Modifiers, Filepath });
        }
        public static WorldSave FromData(string s)
        {
            //string diff, string world, string name, string m, string path
            string[] p = s.Split(';');
            WorldSave ws = new WorldSave(p[0], p[1], p[2], p[3], p[4]);
            return ws;
        }
    }
}
