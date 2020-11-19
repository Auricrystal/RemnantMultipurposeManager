using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using static RemnantBuildRandomizer.DataObj;
using static RemnantBuildRandomizer.GearInfo;


namespace RemnantBuildRandomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static MainWindow MW = null;
        private static string saveDirPath;
        readonly public static Random rd = new Random();
        private static RemnantSave activeSave;
        private static int badluck = 0;

        private FileSystemWatcher saveWatcher;
        private FileSystemWatcher worldWatcher;
        private static Properties.Settings set = Properties.Settings.Default;
        private string[] saveFiles;



        private DateTime lastUpdateCheck;

        public static RemnantSave ActiveSave
        {
            get
            {
                if (activeSave == null)
                {
                    ActiveSave = new RemnantSave(SaveDirPath);
                }
                return activeSave;

            }
            set => activeSave = value;
        }

        public string[] GetSaveFiles
        {
            get
            {
                if (saveFiles == null)
                {
                    string[] files = Directory.GetFiles(SaveDirPath, "save_*.sav");
                    for (int i = 0; i < files.Length; i++) { files[i] = files[i].Replace(SaveDirPath + @"\", ""); }
                    saveFiles = files;
                }

                return saveFiles;
            }
        }

        public RemnantCharacter ActiveCharacter
        {
            get
            {
                try
                {
                    return ActiveSave.Characters[cmbCharacter.SelectedIndex];
                }
                catch (ArgumentOutOfRangeException) { return new RemnantCharacter(0); }
            }
        }
        public string ActiveSaveSlot
        {
            get => (string)cmbSaveSlot.SelectedItem;
            set => cmbSaveSlot.SelectedItem = value;

        }
        private void SetSettings(int Char, int Save)
        {
            GetSettings()[Char] = Save;
            Properties.Settings.Default.SaveSlotData = string.Join(",", GetSettings().Select(x => x.ToString()).ToArray());
            Properties.Settings.Default.Save();
        }
        private int GetSetting(int Char)
        {
            try
            {
                return GetSettings()[Char];
            }

            catch (IndexOutOfRangeException)
            {
                return 0;
            }
        }

        private int[] GetSettings()
        {
            string prop = Properties.Settings.Default.SaveSlotData;
            if (prop.Split(',').Length != 5) { prop = "0,1,2,3,4"; }
            int[] set = prop.Split(',').Select(int.Parse).ToArray();
            return set;
        }

        public static string RBRDirPath
        {
            get
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Remnant\\Saved\\RBR");
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Remnant\\Saved\\RBR";
            }
        }


        public static string SaveDirPath
        {
            get
            {
                if (saveDirPath == null)
                {
                    saveDirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Remnant\\Saved\\SaveGames";
                }
                return saveDirPath;
            }
            set => saveDirPath = value;
        }

        public enum LogType
        {
            Normal,
            Success,
            Error
        }

        public MainWindow()
        {

            MW = this;
            InitializeComponent();
            //ProfileManager.IsEnabled = false;
            //ProfileManager.ToolTip = "Not Implemented Yet.";
            DownloadIMGFiles();
            File.Delete(RBRDirPath + "\\log.txt");
            if (File.Exists(SaveDirPath + "\\profile.sav"))
            {
                saveWatcher = new FileSystemWatcher();
                worldWatcher = new FileSystemWatcher();

                saveWatcher.Path = SaveDirPath;
                worldWatcher.Path = SaveDirPath;

                // Watch for changes in LastWrite times.
                saveWatcher.NotifyFilter = NotifyFilters.LastWrite;
                worldWatcher.NotifyFilter = NotifyFilters.LastWrite;

                // Only watch sav files.
                saveWatcher.Filter = "profile.sav";
                worldWatcher.Filter = "save_*.sav";

                // Add event handlers.
                saveWatcher.Changed += OnProfileSaveChanged;
                saveWatcher.Created += OnProfileSaveChanged;
                saveWatcher.Deleted += OnProfileSaveChanged;

                worldWatcher.Changed += OnWorldFileChanged;
                worldWatcher.Created += OnWorldFileChanged;
                worldWatcher.Deleted += OnWorldFileChanged;
            }

            ((MenuItem)BuildList.ContextMenu.Items[0]).Click += GrabBuild_Click;
            ((MenuItem)BuildList.ContextMenu.Items[1]).Click += CopyBuildObj_Click;
            ((MenuItem)BuildList.ContextMenu.Items[2]).Click += PasteBuildObj_Click;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(SaveDirPath + "\\profile.sav"))
            {
                Debug.WriteLine("Current save date: " + File.GetLastWriteTime(SaveDirPath + "\\profile.sav").ToString());

                saveWatcher.EnableRaisingEvents = true;
                worldWatcher.EnableRaisingEvents = true;

                cmbCharacter.ItemsSource = ActiveSave.Characters;
                cmbCharacter.SelectedIndex = 0;
                cmbSaveSlot.ItemsSource = GetSaveFiles;
                cmbSaveSlot.SelectedIndex = GetSetting(cmbCharacter.SelectedIndex);

                Debug.WriteLine(cmbSaveSlot.SelectedItem);

                ReadXML();
                GetData();
                SetupData();
                UpdateCharacterData();

            }
            else
            {
                ReadXML();
                SetupData();
                cmbCharacter.IsEnabled = false;
                cmbSaveSlot.IsEnabled = false;
                SaveManipulator.IsEnabled = false;
                KeepCheckpoint.IsEnabled = false;
            }
            checkForUpdate();
        }
        private void SetupData()
        {
            List<RemnantItem>
                empties = StrToRI.Values.Where(x => x.Itemname.Contains("_")).ToList(),
                molist = GetEquipment[SlotType.MO].Take(28).ToList(),
                hglist = GetEquipment[SlotType.HG].ToList(),
                lglist = GetEquipment[SlotType.LG].ToList(),
                mlist = GetEquipment[SlotType.M].ToList(),
                helist = GetEquipment[SlotType.HE].ToList(),
                chlist = GetEquipment[SlotType.CH].ToList(),
                lelist = GetEquipment[SlotType.LE].ToList(),
                amlist = GetEquipment[SlotType.AM].ToList(),
                rilist = GetEquipment[SlotType.RI].ToList();


            WeaponList.ItemsSource = Combine(hglist, lglist, mlist).Except(empties).ToList();
            ArmorList.ItemsSource = Combine(helist, chlist, lelist).Except(empties).ToList();
            AmuletList.ItemsSource = amlist.Except(empties).ToList();
            RingList.ItemsSource = rilist.Except(empties).ToList();
            ModList.ItemsSource = molist.Except(empties).ToList();

            if (File.Exists(SaveDirPath + "\\profile.sav"))
            {

                ProfileList.ItemsSource = Directory.GetDirectories(RBRDirPath + "\\Profiles").Select(x => new RemnantProfile(x)).ToList();
                if (ActiveSave.Characters.Count > 0)
                {
                    disablemissing();
                    BuildList.ItemsSource = Presets[ActiveCharacter.charNum].ToList();
                }
                else
                {
                    BuildList.ItemsSource = Presets[0].ToList();
                }


                long time = -1;
                if (!File.Exists(RBRDirPath + @"/WorldSaveData.txt"))
                {
                    SaveFileInit();
                }
                else
                {
                    try
                    {
                        time = ReadWorldSaveData(RBRDirPath + @"/WorldSaveData.txt");
                        bool boss = false, vend = false;
                        if (boss = CheckDownloadZip("Bosses2")) { BossList.ItemsSource = ParseSaveFiles(RBRDirPath + "\\" + "Bosses2" + ".zip"); }
                        if (vend = CheckDownloadZip("Vendors")) { VendList.ItemsSource = ParseSaveFiles(RBRDirPath + "\\" + "Vendors" + ".zip"); }
                        if (Directory.GetFiles(RBRDirPath + "\\" + "Misc Saves").Length > MiscList.Items.Count)
                        {
                            logMessage("More Misc Files Detected, Updating List");
                            MiscList.ItemsSource = ParseSaveFiles(RBRDirPath + "\\Misc Saves");
                        }
                        logMessage("Finished! " + time + "ms");
                    }
                    catch (Exception)
                    {
                        logMessage("Had A Problem Reading WorldSaveData.txt", LogType.Error);
                        SaveFileInit();
                    }
                }
                BossList.Items.Refresh();
                VendList.Items.Refresh();
                MiscList.Items.Refresh();
            }
            else
            {
                BuildList.ItemsSource = Presets[0].ToList();
            }

            EmptySlots.ItemsSource = empties;

            cmbHG.ItemsSource = hglist;
            cmbHGM.ItemsSource = molist;
            cmbLG.ItemsSource = lglist;
            cmbLGM.ItemsSource = molist;
            cmbM.ItemsSource = mlist; ;
            cmbHE.ItemsSource = helist;
            cmbCH.ItemsSource = chlist;
            cmbLE.ItemsSource = lelist;
            cmbAM.ItemsSource = amlist;
            cmbR1.ItemsSource = rilist;
            cmbR2.ItemsSource = rilist;

            setSelectedIndex(0, cmbHG, cmbHGM, cmbLG, cmbLGM, cmbM, cmbHE, cmbCH, cmbLE, cmbAM, cmbR1, cmbR2);
        }

        private void SaveFileInit()
        {
            SaveManipulator.IsEnabled = false;
            logMessage("Parsing Save Files...");
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            var task = Task.Run(() =>
            {

                List<WorldSave>
                boss = ParseSaveFiles(DownloadZip("Bosses2")),
                vend = ParseSaveFiles(DownloadZip("Vendors")),
                misc = ParseSaveFiles(RBRDirPath + "\\Misc Saves");

                this.Dispatcher.Invoke(() =>
                {
                    BossList.ItemsSource = from b in boss orderby b.Name select b;
                    VendList.ItemsSource = vend;
                    MiscList.ItemsSource = misc;
                });

            });


            task.ContinueWith((t) =>
          {
              Dispatcher.Invoke(() =>
              {
                  SaveManipulator.IsEnabled = true;
                  logMessage("Finished! " + watch.ElapsedMilliseconds + "ms");
                  watch.Stop();
                  Debug.WriteLine("SaveCount: " + WorldSave.GetSave.Count);
                  Debug.WriteLine("Time:" + watch.ElapsedMilliseconds);
                  return watch.ElapsedMilliseconds;
              });
          });



        }

        private void setSelectedIndex(int val, params ComboBox[] cmbs)
        {
            foreach (ComboBox cmb in cmbs) { cmb.SelectedIndex = val; }
        }

        private void DownloadIMGFiles()
        {
            DownloadZip("IMG");
        }
        private bool CheckDownloadZip(string zipName)
        {
            bool update = false;
            string bossURL = "https://raw.githubusercontent.com/Auricrystal/RemnantBuildRandomizer/master/Resources/" + zipName + ".zip";
            if (Directory.Exists(RBRDirPath + "\\" + zipName)) { Directory.Delete(RBRDirPath + "\\" + zipName, true); }
            using (WebClient client = new WebClient())
            {
                if (!File.Exists(RBRDirPath + "\\" + zipName + ".zip"))
                {
                    client.DownloadFile(bossURL, RBRDirPath + "\\" + zipName + ".zip");
                }
                else
                {
                    using (ZipArchive Old = ZipFile.Open(RBRDirPath + "\\" + zipName + ".zip", ZipArchiveMode.Update), New = new ZipArchive(client.OpenRead(bossURL)))
                    {
                        foreach (ZipArchiveEntry entry in New.Entries)
                        {
                            //If new file does not exist or the new file is smaller in size than the old
                            if (entry.Name != "")
                            {
                                if (Old.GetEntry(entry.FullName) == null || entry.Length < Old.GetEntry(entry.FullName).Length)
                                {
                                    logMessage("New " + zipName + " Package Update!", LogType.Success);
                                    update = true;
                                    Old.Dispose(); New.Dispose();
                                    File.Delete(RBRDirPath + "\\" + zipName + ".zip"); client.DownloadFile(bossURL, RBRDirPath + "\\" + zipName + ".zip");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return update;
        }
        private string DownloadZip(string zipName)
        {
            string bossURL = "https://raw.githubusercontent.com/Auricrystal/RemnantBuildRandomizer/master/Resources/" + zipName + ".zip";
            if (Directory.Exists(RBRDirPath + "\\" + zipName)) { Directory.Delete(RBRDirPath + "\\" + zipName, true); }
            using (WebClient client = new WebClient())
            {
                if (!File.Exists(RBRDirPath + "\\" + zipName + ".zip"))
                {
                    client.DownloadFile(bossURL, RBRDirPath + "\\" + zipName + ".zip");
                }
                else
                {
                    using (ZipArchive Old = ZipFile.Open(RBRDirPath + "\\" + zipName + ".zip", ZipArchiveMode.Update), New = new ZipArchive(client.OpenRead(bossURL)))
                    {
                        foreach (ZipArchiveEntry entry in New.Entries)
                        {
                            //If new file does not exist or the new file is smaller in size than the old
                            if (entry.Name != "")
                            {
                                if (Old.GetEntry(entry.FullName) == null || entry.Length < Old.GetEntry(entry.FullName).Length)
                                {
                                    logMessage("New " + zipName + " Package Update!\nDownloading!");
                                    Old.Dispose(); New.Dispose();
                                    File.Delete(RBRDirPath + "\\" + zipName + ".zip"); client.DownloadFile(bossURL, RBRDirPath + "\\" + zipName + ".zip");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return RBRDirPath + "\\" + zipName + ".zip";
        }
        private void DownloadNewProfile(string path)
        {
            string profilesave = "https://raw.githubusercontent.com/Auricrystal/RemnantBuildRandomizer/master/Resources/NewProfile.sav";
            using (WebClient client = new WebClient())
            {
                if (!File.Exists(path))
                {
                    client.DownloadFile(profilesave, path + "\\profile.sav");
                }
                else
                {
                    throw new Exception("Not A New Profile Folder");
                }
            }
        }

        private void grabFileFromZip(string path, string filedest)
        {
            using (ZipArchive zip = ZipFile.Open(path.Split('|').First(), ZipArchiveMode.Update))
            {
                zip.GetEntry(path.Split('|').Last()).ExtractToFile(filedest, true);
            }
        }

        private void OnWorldFileChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (cmbSaveSlot.SelectedItem.ToString() != e.Name)
                    {
                        logMessage(e.Name + " was written to");
                        logMessage("Char/Save Mismatch", LogType.Error);
                        lblLastMessage.ToolTip = "Expected " + cmbSaveSlot.SelectedItem.ToString() + " but got " + e.Name;
                    }
                });
            }
            catch (Exception) { }
        }


        private List<WorldSave> ParseSaveFiles(string directory)
        {
            List<WorldSave> list = new List<WorldSave>();

            if (directory.Contains(".zip") && !directory.Contains(".sav"))
            {
                using (ZipArchive za = ZipFile.OpenRead(directory))
                {
                    list.AddRange(za.Entries.Where(x => x.FullName.Contains(".sav")).Select(x => WorldSave.Parse(directory + '|' + x.FullName)));
                    list.AddRange(za.Entries.Where(x => x.FullName.Contains(".zip")).SelectMany(x => ParseSaveFiles(x.FullName)));
                }
            }
            else if (directory.Contains(".zip") && directory.Contains(".sav"))
            {
                list.Add(WorldSave.Parse(directory));
            }
            else
            {
                if (Directory.Exists(directory))
                {
                    list.AddRange(Directory.GetFiles(directory).Where(x => x.Contains(".sav")).Select(x => WorldSave.Parse(x)));
                    list.AddRange(Directory.GetFiles(directory).Where(x => x.Contains(".zip") && !x.Contains(".sav")).SelectMany(x => ParseSaveFiles(x)));
                }
            }
            return list;
        }

        private void LoadSave()
        {
            BossList.SelectedIndex = rd.Next(BossList.Items.Count);
            LoadSave((WorldSave)BossList.SelectedItem);
        }
        private void LoadSave(WorldSave rb)
        {
            logMessage("Loading: " + rb.path.Split('\\', '/').Last().Replace("\n", ""));
            if (rb.path.Contains(".zip"))
            {
                grabFileFromZip(rb.path, SaveDirPath + "\\" + ActiveSaveSlot);
            }
            else
            {
                File.Copy(rb.path.Replace("\n", ""), SaveDirPath + "\\" + ActiveSaveSlot, true);
            }
            File.SetLastWriteTime(SaveDirPath + "\\" + ActiveSaveSlot, DateTime.Now);
            File.SetLastWriteTime(SaveDirPath + "\\" + "Profile.sav", DateTime.Now);
        }



        private static List<T> Combine<T>(params IEnumerable<T>[] rils)
        {
            List<T> li = new List<T>();
            foreach (IEnumerable<T> ril in rils) { li.AddRange(ril); }
            return li;
        }

        private int checkForUpdate()
        {
            int compare = 0;
            using (WebClient client = new WebClient())
            {
                try
                {
                    string source = client.DownloadString("https://github.com/Auricrystal/RemnantBuildRandomizer/releases/latest");
                    string title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                    string remoteVer = Regex.Match(source, @"Remnant Multipurpose Manager (?<Version>([\d.]+)?)", RegexOptions.IgnoreCase).Groups["Version"].Value + ".0";

                    Version remoteVersion = new Version(remoteVer);
                    Version localVersion = typeof(MainWindow).Assembly.GetName().Version;
                    Debug.WriteLine("remote:" + remoteVersion + '\n' + "local:" + localVersion);
                    this.Dispatcher.Invoke(() =>
                        {
                            compare = localVersion.CompareTo(remoteVersion);
                            if (compare == -1)
                            {
                                Debug.WriteLine("Inside Update value: " + compare);
                                Debug.WriteLine("New Version Available");
                                BitmapImage bmp = new BitmapImage(new Uri(@"pack://application:,,,/Resources/IMG/Menu/_OutOfDateVersion.png"));
                                Version.Header = new Image { Source = bmp };
                                Version.ToolTip = "Update Available!";
                            }
                            else if (compare == 1)
                            {

                                Debug.WriteLine("Beta Version Detected");
                                BitmapImage bmp = new BitmapImage(new Uri(@"pack://application:,,,/Resources/IMG/Menu/_BetaVersion.png"));
                                Version.Header = new Image { Source = bmp };
                                Version.ToolTip = "Beta Version";
                            }
                            else
                            {
                                logMessage("No new version found.");
                                BitmapImage bmp = new BitmapImage(new Uri(@"pack://application:,,,/Resources/IMG/Menu/_CurrentVersion.png"));
                                Version.Header = new Image { Source = bmp };
                                Version.ToolTip = "No Update Found";

                            }
                        }
                     );
                }
                catch (Exception)
                {
                }
            }
            lastUpdateCheck = DateTime.Now;
            Debug.WriteLine("Update value: " + compare);
            return compare;

        }

        public static void SlogMessage(string msg)
        {
            MW.logMessage(msg, Colors.White);
        }
        public void logMessage(string msg)
        {
            logMessage(msg, Colors.White);
        }


        public void logMessage(string msg, LogType lt)
        {
            Color color = Colors.White;
            if (lt == LogType.Success)
            {
                color = Color.FromRgb(0, 200, 0);
            }
            else if (lt == LogType.Error)
            {
                color = Color.FromRgb(200, 0, 0);
            }
            logMessage(msg, color);
        }


        public void logMessage(string msg, Color color)
        {

            if (!Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => { logMessage(msg, color); });
            }
            else
            {
                txtLog.Text = txtLog.Text + Environment.NewLine + DateTime.Now.ToString() + ": " + msg;
                lblLastMessage.Text = msg;
                lblLastMessage.ToolTip = null;
                lblLastMessage.Foreground = new SolidColorBrush(color);
                if (color.Equals(Colors.White))
                {
                    lblLastMessage.FontWeight = FontWeights.Normal;
                }
                else
                {
                    lblLastMessage.FontWeight = FontWeights.Bold;
                }
                StreamWriter writer = System.IO.File.AppendText(RBRDirPath + "/log.txt");
                writer.WriteLine(DateTime.Now.ToString() + ": " + msg);
                writer.Close();
            }




        }


        private void OnProfileSaveChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            this.Dispatcher.Invoke(() =>
            {
                int temp = cmbCharacter.SelectedIndex;
                ActiveSave = new RemnantSave(saveDirPath);
                cmbCharacter.ItemsSource = ActiveSave.Characters;
                if (temp > ActiveSave.Characters.Count - 1)
                {
                    cmbCharacter.SelectedIndex = 0;
                }
                else { cmbCharacter.SelectedIndex = temp; }
                cmbSaveSlot.ItemsSource = GetSaveFiles;
                cmbSaveSlot.SelectedIndex = GetSetting(cmbCharacter.SelectedIndex);
            });


            KeepCheck();
            UpdateCharacterData();
        }

        private void KeepCheck()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (KeepCheckpoint.IsChecked == true)
                {
                    LoadCheckpoint();
                }
            });
        }

        private void UpdateCharacterData()
        {
            ActiveSave.UpdateCharacters();

            this.Dispatcher.Invoke(() =>
            {
                SaveTime.Text = "Last Save: " + File.GetLastWriteTime(SaveDirPath + "\\profile.sav");
            });
        }



        private string Code(int[] arr)
        {
            var test = arr.Select(x => string.Format("{0, 0:D2}", x)).ToArray();
            return string.Join("-", test);

        }
        private void DisplayBuild(Build b)
        {
            setImage(HandGunImg, HandGunText, b.HandGun);
            setModImage(HandModImg, HandCoverModImg, HandGunModText, b.HandMod);
            setImage(LongGunImg, LongGunText, b.LongGun);
            setModImage(LongModImg, LongCoverModImg, LongGunModText, b.LongMod);
            setImage(MeleeImg, MeleeText, b.Melee);
            setImage(HeadImg, HeadText, b.Head);
            setImage(ChestImg, ChestText, b.Chest);
            setImage(LegImg, LegText, b.Legs);
            setImage(AmImg, AmuletText, b.Amulet);
            setImage(Ri1Img, Ring1Text, b.Ring1);
            setImage(Ri2Img, Ring2Text, b.Ring2);
        }
        private void setImage(Image i, RemnantItem ri)
        {
            i.Source = ri.Data.GetImage();
            Debug.WriteLine("Set image:" + ri.Data.GetImage().ToString());
            ToolTipService.SetShowDuration(i, 60000);
            i.ToolTip = ri.Description;
        }
        private void setImage(Image i, TextBlock tb, RemnantItem ri)
        {
            i.Source = ri.Data.GetImage();
            Debug.WriteLine("Set image:" + ri.Data.GetImage().ToString());
            ToolTipService.SetShowDuration(i, 60000);
            i.ToolTip = ri.Description;
            tb.Text = ri.Itemname;
        }
        private void setModImage(Image i, Image j, RemnantItem ri)
        {
            i.Source = ri.Data.GetImage();
            ToolTipService.SetShowDuration(j, 60000);
            j.ToolTip = ri.Description;
        }
        private void setModImage(Image i, Image j, TextBlock tb, RemnantItem ri)
        {
            i.Source = ri.Data.GetImage();
            ToolTipService.SetShowDuration(j, 60000);
            j.ToolTip = ri.Description;
            tb.Text = ri.Itemname;
        }


        private void Conditions(Build b)
        {
            if (b.Amulet.Itemname == "White Rose")
            {
                string text = "\n\nWHITE ROSE EFFECT\n";
                Debug.WriteLine(text);
                if (rd.Next(2) == 1) { setImage(HandGunImg, HandGunText, StrToRI["_No Hand Gun"]); setImage(HandModImg, HandGunModText, StrToRI["_No Mod"]); HandCoverModImg.ToolTip = null; text += "removed HG\n"; }
                if (rd.Next(2) == 1) { setImage(LongGunImg, LongGunText, StrToRI["_No Long Gun"]); setImage(LongModImg, LongGunModText, StrToRI["_No Mod"]); LongCoverModImg.ToolTip = null; text += "removed LG"; }
                AmImg.ToolTip += text;
            }
            else if (b.Amulet.Itemname == "Daredevil's Charm")
            {
                string text = "\n\nDDC EFFECT\n";
                Debug.WriteLine(text);
                if (rd.Next(2) == 1) { setImage(HeadImg, HeadText, StrToRI["_No Head"]); text += "removed Head\n"; }
                if (rd.Next(2) == 1) { setImage(ChestImg, ChestText, StrToRI["_No Chest"]); text += "removed Chest\n"; }
                if (rd.Next(2) == 1) { setImage(LegImg, LegText, StrToRI["_No Legs"]); text += "removed Legs"; }
                AmImg.ToolTip += text;
            }
            if (b.Ring1.Itemname.ToLower() == "Ring Of The Unclean".ToLower() || b.Ring2.Itemname.ToLower() == "Ring Of The Unclean".ToLower() ||
                b.Ring1.Itemname.ToLower() == "Five Fingered Ring".ToLower() || b.Ring2.Itemname.ToLower() == "Five Fingered Ring".ToLower())
            {
                Debug.WriteLine("ROTU or FFR Effect");
                if (rd.Next(2) == 1) { setImage(MeleeImg, MeleeText, StrToRI["_Fists"]); }

            }

        }

        public void SaveData()
        {
            string path = RBRDirPath + @"/Data.txt";
            File.Delete(path);
            GetData();
            CreateWorldSaveData(RBRDirPath + @"/WorldSaveData.txt");
        }


        private void RemnantBuildRandomizer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //MessageBox.Show("Closing called");
            if (AlterFile.IsChecked == true)
            {
                foreach (string s in Directory.GetFiles(RBRDirPath + "\\Backup\\", "save_*.sav"))
                {
                    File.Copy(s, saveDirPath + '\\' + s.Split('\\').Last(), true);
                    File.SetLastWriteTime(saveDirPath + '\\' + s.Split('\\').Last(), DateTime.Now);
                    logMessage("Reverting " + s.Split('\\').Last());
                }
                File.SetLastWriteTime(SaveDirPath + "\\" + "Profile.sav", DateTime.Now);
            }
            if (ActiveSave.Characters.Count > 0) { SaveData(); }

        }




        public static void CopyUIElementToClipboard(FrameworkElement element)
        {
            double width = element.ActualWidth;
            double height = element.ActualHeight;
            RenderTargetBitmap bmpCopied = new RenderTargetBitmap((int)Math.Round(width), (int)Math.Round(height), 96, 96, PixelFormats.Default);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(element);
                dc.DrawRectangle(vb, null, new Rect(new Point(), new Size(width, height)));
            }
            bmpCopied.Render(dv);
            Clipboard.SetImage(bmpCopied);
        }

        private void cmbSaveSlot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine(cmbSaveSlot.SelectedItem);
            try
            {
                SetSettings(cmbCharacter.SelectedIndex, cmbSaveSlot.SelectedIndex);
            }
            catch (IndexOutOfRangeException) { }



        }
        private void CmbCharacter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (cmbCharacter.SelectedIndex == -1 && ActiveSave.Characters.Count > 0) return;
            if (cmbCharacter.Items.Count > 0 && cmbCharacter.SelectedIndex > -1)
            {
                cmbSaveSlot.SelectedIndex = GetSetting(cmbCharacter.SelectedIndex);
                safeCommit(WeaponList, ArmorList, AmuletList, RingList, ModList, BuildList, EmptySlots);
                try
                {
                    BuildList.ItemsSource = Presets[ActiveCharacter.charNum];
                }
                catch (KeyNotFoundException) { Presets.Add(ActiveCharacter.charNum, new List<Build>()); }

                Debug.WriteLine("CHANGED TO " + ActiveSave.Characters[cmbCharacter.SelectedIndex].ToString());
                Debug.WriteLine(ActiveSave.Characters[cmbCharacter.SelectedIndex].GetMissingItems().Count);
                foreach (RemnantItem ri in StrToRI.Values)
                {
                    ri.Character = cmbCharacter.SelectedIndex;
                }
                disablemissing();
                safeRefresh(WeaponList, ArmorList, AmuletList, RingList, ModList, BuildList, EmptySlots);
            }
        }
        private void safeRefresh(params DataGrid[] dg)
        {
            safeCommit(dg);
            foreach (DataGrid d in dg) { d.Items.Refresh(); }
        }
        private void safeCommit(params DataGrid[] dg)
        {
            foreach (DataGrid d in dg)
            {
                d.CommitEdit();
                d.CommitEdit();
            }
        }


        private void disablemissing()
        {
            RemnantCharacter rc = ActiveSave.Characters[cmbCharacter.SelectedIndex];
            Debug.WriteLine(rc.ToString() + " Has " + rc.GetMissingItems().Count + " Missing items.");
            foreach (RemnantItem ri in StrToRI.Values)
            {
                ri.Missing = false;
            }
            foreach (Item ri in rc.GetMissingItems())
            {
                StrToRI[ri.ItemAltName].Missing = true;
            }

        }



        private void WeaponList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();
            //Cancel the column you don't want to generate

            if (headername == "Missing")
            {
                string path = SaveDirPath + "\\profile.sav ";
                Debug.WriteLine("header is " + headername + " " + path + " Exists " + File.Exists(SaveDirPath + "\\profile.sav"));
                if (File.Exists(SaveDirPath + "\\profile.sav"))
                {
                    Debug.WriteLine("Readonly " + headername);
                    e.Column.IsReadOnly = true;
                }
                else
                {
                    Debug.WriteLine("Cancelling " + headername);
                    e.Cancel = true;
                }
            }


            switch (headername)
            {
                case "Data":
                case "Description":
                case "Character": e.Cancel = true; break;



                case "Itemname":
                case "Slot":
                case "Mod": e.Column.IsReadOnly = true; break;
                default: break;
            }


        }
        private void ArmorList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();
            //Cancel the column you don't want to generate 
            switch (headername)
            {
                case "Data":
                case "Mod":
                case "Character": e.Cancel = true; break;

                case "Missing": if (!File.Exists(SaveDirPath + "\\profile.sav")) { e.Cancel = true; } else { e.Column.IsReadOnly = true; } break;
                case "Itemname":
                case "Slot":
                case "Description": e.Column.IsReadOnly = true; break;
                default: break;
            }
        }

        private void TrinketList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();
            //Cancel the column you don't want to generate 
            switch (headername)
            {
                case "Data":
                case "Mod":
                case "Slot":
                case "Character": e.Cancel = true; break;

                case "Missing": if (!File.Exists(SaveDirPath + "\\profile.sav")) { e.Cancel = true; } else { e.Column.IsReadOnly = true; } break;
                case "Itemname":
                case "Description": e.Column.IsReadOnly = true; break;
                default: break;
            }
        }
        private void BuildList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();
            //Cancel the column you don't want to generate 
            switch (headername)
            {
                case "BuildName":
                case "Disabled":
                case "Code": break;

                default: e.Column.IsReadOnly = true; break;
            }

        }

        private void BuildList_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            Build edit = ((Build)e.Row.Item);
            Debug.WriteLine("Editting: " + edit);
        }
        private bool isManualEditCommit;
        private void BuildList_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (!isManualEditCommit)
            {
                isManualEditCommit = true;
                DataGrid grid = (DataGrid)sender;
                grid.CommitEdit(DataGridEditingUnit.Row, true);
                grid.Items.Refresh();
                isManualEditCommit = false;
            }
        }

        private void RemItem_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            RemnantItem edit = ((RemnantItem)e.Row.Item);
            logMessage("Edited item: " + edit);
        }
        private void MiscList_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (!isManualEditCommit)
                {
                    isManualEditCommit = true;
                    DataGrid grid = (DataGrid)sender;
                    grid.CommitEdit(DataGridEditingUnit.Row, true);
                    grid.Items.Refresh();
                    isManualEditCommit = false;

                    Debug.WriteLine("EDIT ENDING");
                    WorldSave rb = (WorldSave)MiscList.SelectedItem;
                    if (rb.path.Contains(".zip"))
                    {
                        using (ZipArchive z = ZipFile.Open(rb.path.Split('|').First(), ZipArchiveMode.Update))
                        {
                            z.RenameEntry(rb.path.Split('|').Last().Replace("\n", "").Replace("__", "_"), rb.fullpath().Split('|').Last().Replace("__", "_"));
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Renaming:");
                        Debug.WriteLine(rb.path.Replace("__", "_"));
                        File.Move(rb.path.Replace("__", "_"), rb.fullpath().Replace("__", "_"));
                        Debug.WriteLine(rb.path.Replace("__", "_"));
                    }
                    grid.Items.Refresh();
                }
            }
            catch (Exception edit)
            {
                logMessage(edit.Message, LogType.Error);
                logMessage("Problem Naming File", LogType.Error);
            }
        }
        private void ProfileList_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (!isManualEditCommit)
                {
                    isManualEditCommit = true;
                    DataGrid grid = (DataGrid)sender;
                    grid.CommitEdit(DataGridEditingUnit.Row, true);
                    grid.Items.Refresh();
                    isManualEditCommit = false;
                    Debug.WriteLine("Renaming:");
                }
            }
            catch (Exception edit)
            {
                logMessage(edit.Message, LogType.Error);
                logMessage("Problem Naming Folder", LogType.Error);
            }
        }
        private void BuildList_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }
        private void RemItem_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            RemnantItem edit = ((RemnantItem)e.Row.Item);
            logMessage("Beginning edit: " + edit);

        }



        private void RemItem_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }



        private void cmbHG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemnantItem hg = (RemnantItem)cmbHG.SelectedItem;
            cmbHGM.SelectedIndex = 0;
            if (hg.Mod != "")
            {
                cmbHGM.ItemsSource = new List<RemnantItem>() { StrToRI[hg.Mod] };
            }
            else
            {
                cmbHGM.ItemsSource = GetEquipment[SlotType.MO].Take(28).ToList();
            }
            cmbHGM.SelectedIndex = 0;
            cmbHGM.Items.Refresh();
        }

        private void cmbLG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemnantItem lg = (RemnantItem)cmbLG.SelectedItem;
            cmbLGM.SelectedIndex = 0;
            if (lg.Mod != "")
            {
                cmbLGM.ItemsSource = new List<RemnantItem>() { StrToRI[lg.Mod] };
            }
            else
            {
                cmbLGM.ItemsSource = GetEquipment[SlotType.MO].Take(28).ToList();
            }
            cmbLGM.SelectedIndex = 0;
            cmbLGM.Items.Refresh();

        }

        private void cmbR1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbR2.ItemsSource = StrToRI.Values.Where(x => x != (RemnantItem)cmbR1.SelectedItem && x.Data.Slot == SlotType.RI || x.Itemname == "_No Ring");
        }

        private void cmbR2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbR1.ItemsSource = StrToRI.Values.Where(x => x != (RemnantItem)cmbR2.SelectedItem && x.Data.Slot == SlotType.RI || x.Itemname == "_No Ring");
        }



        private void cmbHGM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbLGM.Items.Count > 1)
            {

                if (cmbLGM.SelectedIndex > 27) { cmbHGM.SelectedIndex = 0; }
                cmbLGM.ItemsSource = StrToRI.Values.Where(x => x != (RemnantItem)cmbHGM.SelectedItem && x.Data.Slot == SlotType.MO || x.Itemname == "_No Mod").Take(27);
            }
            else { }
        }

        private void cmbLGM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbHGM.Items.Count > 1)
            {
                if (cmbHGM.SelectedIndex > 27) { cmbHGM.SelectedIndex = 0; }

                cmbHGM.ItemsSource = StrToRI.Values.Where(x => x != (RemnantItem)cmbLGM.SelectedItem && x.Data.Slot == SlotType.MO || x.Itemname == "_No Mod").Take(27);
            }
            else { }
        }


        #region Click Events
        private void Reroll_Click(object sender, RoutedEventArgs e)
        {
            int slot;
            if (File.Exists(SaveDirPath + "\\profile.sav") && ActiveSave.Characters.Count > 0)
            {
                slot = ActiveCharacter.charNum;
            }
            else { slot = 0; }
            List<Build> builds = Presets[slot];

            List<Build> list = builds.Where(x => x.Disabled == false).ToList();
            if (list.Count > 0 && rd.Next(100) <= badluck)
            {
                badluck = 0;
                Build b = list[rd.Next(list.Count)];
                DisplayBuild(b);

                BuildNum.Text = b.BuildName;
                BuildNum.ToolTip = b.toStringCode();
            }
            else
            {
                if (list.Count > 0)
                {
                    badluck++;
                    Debug.WriteLine(badluck);
                }
                Build rand = new Build("");
                DisplayBuild(rand);
                Conditions(rand);
                Debug.WriteLine(Code(rand.toCodeArray()));

                BuildNum.Text = Code(rand.toCodeArray());
                BuildNum.ToolTip = null;
            }
        }
        private void Add_Build_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BuildNameEnter.Text == "") { throw new Exception("No BuildName Assigned to Build!"); }
                Build b = new Build(BuildNameEnter.Text, BuildCodeEnter.Text);
                int slot;
                if (File.Exists(SaveDirPath + "\\profile.sav") && ActiveSave.Characters.Count > 0) { slot = ActiveCharacter.charNum; }
                else { slot = 0; }
                if (!Presets[slot].Contains(new Build(BuildNameEnter.Text, BuildCodeEnter.Text)))
                {
                    BuildNameEnter.Text = "";
                    BuildCodeEnter.Text = "";

                    Presets[slot].Add(b);
                    BuildList.ItemsSource = Presets[slot];
                    Debug.WriteLine("Preset count" + Presets[slot].Count);
                }
            }
            catch (Exception ce) { MessageBox.Show(ce.Message); }
            BuildList.Items.Refresh();
        }
        private void CopyBuild_Click(object sender, RoutedEventArgs e)
        {
            CopyUIElementToClipboard(BuildScreen);
            MessageBox.Show("Copied to Clipboard!");
        }
        private void ResetBlack_Click(object sender, RoutedEventArgs e)
        {
            StrToRI.Values.ToList().ForEach((x) => { x.No = false; });
            safeRefresh(WeaponList, ArmorList, AmuletList, RingList, ModList, EmptySlots);
        }
        private void UpdateCheck_Click(object sender, RoutedEventArgs e)
        {
            if (checkForUpdate() == -1)
            {
                var confirmResult = MessageBox.Show("There is a new version available. Would you like to open the download page?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (confirmResult == MessageBoxResult.Yes)
                {
                    Process.Start("https://github.com/Auricrystal/RemnantBuildRandomizer/releases/latest");
                    System.Environment.Exit(1);
                }
            }
        }
        private void GenBuildCode_Click(object sender, RoutedEventArgs e)
        {
            RemnantItem
                hg = (RemnantItem)cmbHG.SelectedItem, hgm = (RemnantItem)cmbHGM.SelectedItem,
                lg = (RemnantItem)cmbLG.SelectedItem, lgm = (RemnantItem)cmbLGM.SelectedItem,
                 m = (RemnantItem)cmbM.SelectedItem,
                he = (RemnantItem)cmbHE.SelectedItem, ch = (RemnantItem)cmbCH.SelectedItem, le = (RemnantItem)cmbLE.SelectedItem,
                am = (RemnantItem)cmbAM.SelectedItem, r1 = (RemnantItem)cmbR1.SelectedItem, r2 = (RemnantItem)cmbR2.SelectedItem;

            BuildCodeEnter.Text = new Build("", hg, hgm, lg, lgm, m, he, ch, le, am, r1, r2).toStringCode();

            cmbHG.SelectedIndex = 0; cmbHGM.SelectedIndex = 0;
            cmbLG.SelectedIndex = 0; cmbLGM.SelectedIndex = 0;
            cmbM.SelectedIndex = 0;
            cmbHE.SelectedIndex = 0; cmbCH.SelectedIndex = 0; cmbLE.SelectedIndex = 0;
            cmbAM.SelectedIndex = 0; cmbR1.SelectedIndex = 0; cmbR2.SelectedIndex = 0;
        }
        private void BuildCode_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(BuildNum.Text);
        }
        private void DataFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(RBRDirPath);
        }
        private void GrabBuild_Click(object sender, System.EventArgs e)
        {
            if (BuildList.SelectedItem != null)
            {
                Build selectedBuild = (Build)(BuildList.SelectedItem);
                logMessage("Grabbing screenshot of (" + selectedBuild.BuildName + ")");
                new BuildWindow(selectedBuild);
            }
        }
        private void CopyBuildObj_Click(object sender, System.EventArgs e)
        {
            if (BuildList.SelectedItem != null)
            {
                Build selectedBuild = (Build)(BuildList.SelectedItem);
                logMessage("Grabbing Build Object of (" + selectedBuild.BuildName + ")");
                Clipboard.SetText(selectedBuild.ToData().Trim());
                BuildList.SelectedItem = null;
            }
        }
        private void PasteBuildObj_Click(object sender, System.EventArgs e)
        {
            try
            {
                int slot;
                if (File.Exists(SaveDirPath + "\\profile.sav")) { slot = ActiveCharacter.charNum; } else { slot = 0; }
                Build b = Build.FromData(Clipboard.GetText().Trim());
                logMessage("Pasting Build (" + b + ")");
                if (!Presets[slot].Contains(b)) { Presets[slot].Add(b); safeRefresh(BuildList); }
                else { logMessage("Build (" + b + ") already exists!"); }
            }
            catch (Exception err) { logMessage(err.Message); }
        }
        private void DisableEmpties_Click(object sender, RoutedEventArgs e)
        {
            StrToRI.Values.Where(x => x.Itemname.Contains("_")).ToList().ForEach((x) => { x.No = true; });
            EmptySlots.Items.Refresh();
        }
        private void LoadBoss_Click(object sender, RoutedEventArgs e)
        {
            WorldSave ws;
            if ((ws = GetSelectedSave()) != null) { LoadSave(ws); }
        }
        private void FeelingLucky_Click(object sender, RoutedEventArgs e)
        {
            LoadSave();
        }
        private void GameRestart_Click(object sender, RoutedEventArgs e)
        {
            RestartGame();
        }
        private void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            File.Delete(GetSelectedSave().path);
            (MiscList.ItemsSource as List<WorldSave>).Remove(GetSelectedSave());
            MiscList.Items.Refresh();
        }
        private void AddCurrentCheckpoint_Click(object sender, RoutedEventArgs e)
        {
            int dupe = 0;
            while (File.Exists(RBRDirPath + "\\Misc Saves\\" + "Checkpoint_" + dupe + ".sav")) { dupe++; }
            File.Copy(SaveDirPath + "\\" + ActiveSaveSlot, RBRDirPath + "\\Misc Saves\\" + "Checkpoint_" + dupe + ".sav", true);
            File.SetLastWriteTime(RBRDirPath + "\\Misc Saves\\" + "Checkpoint_" + dupe + ".sav", DateTime.Now);
            MiscList.ItemsSource = ParseSaveFiles(RBRDirPath + "\\Misc Saves\\");
            MiscList.Items.Refresh();
        }
        private void CreateProfile_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(RBRDirPath + "\\Profiles");
            int num = 0;
            while (Directory.Exists(RBRDirPath + "\\Profiles\\" + "NewProfile" + num)) { num++; }
            Directory.CreateDirectory(RBRDirPath + "\\Profiles\\" + "NewProfile" + num);
            DownloadNewProfile(RBRDirPath + "\\Profiles\\" + "NewProfile" + num);
            ProfileList.ItemsSource = Directory.GetDirectories(RBRDirPath + "\\Profiles").Select(x => new RemnantProfile(x)).ToList();
            ProfileList.Items.Refresh();
        }
        private void LoadProfile_Click(object sender, RoutedEventArgs e)
        {
            string foldername = ((RemnantProfile)ProfileList.SelectedItem).Profile;
            var confirmResult = MessageBox.Show("Are you sure you want to Load: " + foldername + "?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (confirmResult == MessageBoxResult.Yes)
            {
                File.Copy(
                    saveDirPath + "\\profile.sav",
                    saveDirPath + "\\RBRprofile.bak",
                    true
                    );
                File.Copy(RBRDirPath + "\\Profiles\\" + foldername + "\\profile.sav", saveDirPath + "\\profile.sav", true);
                logMessage("Successfully Loaded " + foldername, LogType.Success);
                confirmResult = MessageBox.Show("Restart Game?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (confirmResult == MessageBoxResult.Yes) { RestartGame(); }
                ProfileList.ItemsSource = Directory.GetDirectories(RBRDirPath + "\\Profiles").Select(x => new RemnantProfile(x)).ToList();
                ProfileList.Items.Refresh();
            }
        }
        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            string foldername = ((RemnantProfile)ProfileList.SelectedItem).Profile;
            var confirmResult = MessageBox.Show("Are you sure you want to overwrite: " + foldername + "?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (confirmResult == MessageBoxResult.Yes)
            {
                File.Copy(RBRDirPath + "\\Profiles\\" + foldername + "\\profile.sav", RBRDirPath + "\\Profiles\\" + foldername + "\\profile.bak", true);
                File.Copy(saveDirPath + "\\profile.sav", RBRDirPath + "\\Profiles\\" + foldername + "\\profile.sav", true);
                logMessage("Successfully Overwrote " + foldername, LogType.Success);
                ProfileList.ItemsSource = Directory.GetDirectories(RBRDirPath + "\\Profiles").Select(x => new RemnantProfile(x)).ToList();
                ProfileList.Items.Refresh();
            }
        }
        #endregion

        private void BuildList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MenuItem CopyBuildImg = ((MenuItem)BuildList.ContextMenu.Items[0]);
            MenuItem CopyBuildCode = ((MenuItem)BuildList.ContextMenu.Items[1]);
            MenuItem PasteBuildCode = ((MenuItem)BuildList.ContextMenu.Items[2]);
            try
            {
                Build.FromData(Clipboard.GetText());
            }
            catch (Exception)
            {
            }

            if (BuildList.SelectedItem == null)
            {
                CopyBuildCode.IsEnabled = false;
                CopyBuildImg.IsEnabled = false;
            }
            else
            {
                CopyBuildCode.IsEnabled = true;
                CopyBuildImg.IsEnabled = true;
            }
        }

        private void EmptySlots_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();
            //Cancel the column you don't want to generate
            switch (headername)
            {
                case "Data":
                case "Description":
                case "Slot":
                case "Mod":
                case "Missing":
                case "Character": e.Cancel = true; break;
                case "Itemname": e.Column.IsReadOnly = true; break;
                default: break;
            }
        }
        private void BossList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.IsReadOnly = true;
            if (e.Column.Header.ToString() == "Description") { e.Cancel = true; }
        }
        private void VendorList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();
            switch (header)
            {
                case "Modifiers": e.Column.Header = "Inventory"; break;
                case "Diff": e.Cancel = true; break;
            }
            e.Column.IsReadOnly = true;
        }





        private void LoadCheckpoint()
        {
            LoadCheckpoint(RBRDirPath + "\\Backup\\Keep.sav");
        }
        private void LoadCheckpoint(string path)
        {
            try
            {
                worldWatcher.EnableRaisingEvents = false;
                saveWatcher.EnableRaisingEvents = false;
                Debug.WriteLine("Path: " + path);
                File.Copy(path, SaveDirPath + "\\" + ActiveSaveSlot, true);
                File.SetLastWriteTime(SaveDirPath + "\\" + ActiveSaveSlot, DateTime.Now);
                File.SetLastWriteTime(SaveDirPath + "\\" + "Profile.sav", DateTime.Now);
                logMessage("Loading: " + path.Split('/', '\\').Last() + " to " + ActiveSaveSlot);
                worldWatcher.EnableRaisingEvents = true;
                saveWatcher.EnableRaisingEvents = true;
            }
            catch (IOException ex)
            {
                if (ex.Message.Contains("being used by another process"))
                {
                    Console.WriteLine("Save file in use; waiting 0.5 seconds and retrying.");

                    Thread.Sleep(500);
                    LoadCheckpoint(path);
                }
            }
        }


        private void RandomBoss_Checked(object sender, RoutedEventArgs e)
        {


        }
        private void RandomBoss_Unchecked(object sender, RoutedEventArgs e)
        {
        }




        private void KeepCheckpoint_Checked(object sender, RoutedEventArgs e)
        {
            worldWatcher.EnableRaisingEvents = false;
            saveWatcher.EnableRaisingEvents = false;
            logMessage("KeepCheck Enabled");
            File.Copy(saveDirPath + "\\" + ActiveSaveSlot, RBRDirPath + "\\Backup\\Keep.sav", true);
            File.SetLastWriteTime(RBRDirPath + "\\Backup\\Keep.sav", DateTime.Now);
            File.SetLastWriteTime(SaveDirPath + "\\" + "Profile.sav", DateTime.Now);
            worldWatcher.EnableRaisingEvents = true;
            saveWatcher.EnableRaisingEvents = true;



            LoadButtonSettings();

        }
        private void KeepCheckpoint_Unchecked(object sender, RoutedEventArgs e)
        {
            logMessage("KeepCheck Disabled");
            LoadButtonSettings();
        }

        private void LoadButtonSettings()
        {

            LoadBoss.IsEnabled = AlterFile.IsChecked.Value && !KeepCheckpoint.IsChecked.Value;
            FeelingLucky.IsEnabled = AlterFile.IsChecked.Value && !KeepCheckpoint.IsChecked.Value;
        }

        private WorldSave GetSelectedSave()
        {
            TabItem t = ((TabItem)SaveManager.SelectedItem);
            DataGrid dg = (DataGrid)t.Content;
            if (dg.SelectedIndex > -1)
            {
                return ((WorldSave)dg.SelectedItem);
            }
            return null;

        }

        private void AlterFile_Checked(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(RBRDirPath + "\\Backup\\")) { Directory.CreateDirectory(RBRDirPath + "\\Backup\\"); }
            List<string> files = new List<string>();
            foreach (string s in Directory.GetFiles(saveDirPath, "save_*.sav"))
            {
                File.Copy(s, RBRDirPath + "\\Backup\\" + s.Split('\\').Last(), true);
                File.SetLastWriteTime(RBRDirPath + "\\Backup\\" + s.Split('\\').Last(), DateTime.Now);
                files.Add(s.Split('\\').Last());
            }
            File.SetLastWriteTime(SaveDirPath + "\\" + "Profile.sav", DateTime.Now);
            logMessage("Backing Up: " + string.Join(", ", files), LogType.Success);
            LoadButtonSettings();
        }

        private void AlterFile_Unchecked(object sender, RoutedEventArgs e)
        {
            worldWatcher.EnableRaisingEvents = false;
            KeepCheckpoint.IsChecked = false;
            List<string> files = new List<string>();
            foreach (string s in Directory.GetFiles(RBRDirPath + "\\Backup\\", "save_*.sav"))
            {
                File.Copy(s, saveDirPath + '\\' + s.Split('\\').Last(), true);
                File.SetLastWriteTime(saveDirPath + '\\' + s.Split('\\').Last(), DateTime.Now);
                files.Add(s.Split('\\').Last());
            }
            File.SetLastWriteTime(SaveDirPath + "\\" + "Profile.sav", DateTime.Now);
            logMessage("Restoring: " + string.Join(", ", files), LogType.Success);
            worldWatcher.EnableRaisingEvents = true;
            LoadButtonSettings();
        }


        private void RestartGame()
        {
            //Process p = Process.GetProcessById(7364);
            //Debug.WriteLine("Process: "+p.ProcessName);


            var process = Process.GetProcessesByName("Remnant-Win64-Shipping");
            if (process.Length > 0)
            {
                string path = process[0].MainModule.FileName;
                Debug.WriteLine(process.Length + " Remnant Is Running! " + path + " PID:" + process[0].Id);
                while (!process[0].WaitForExit(1000)) { process[0].Kill(); }
                if (path.Contains("steam")) { Process.Start("steam://rungameid/617290"); } else { Process.Start(path); }
            }
            else
            {
                Debug.WriteLine("Remnant Is NOT Running!");
            }
        }


        private void CheckSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBoxName = (TextBox)sender;
            string[] filterText = textBoxName.Text.Split(' ');
            ICollectionView[] arr =
                new ICollectionView[] {
                    CollectionViewSource.GetDefaultView(BossList.ItemsSource),
                    CollectionViewSource.GetDefaultView(VendList.ItemsSource),
                    CollectionViewSource.GetDefaultView(MiscList.ItemsSource)
                };

            if (!string.IsNullOrEmpty(filterText[0]))
            {
                foreach (ICollectionView icv in arr) { icv.Filter = o => { return ((o as WorldSave).Contains(filterText)); }; }
            }
            else
            {
                foreach (ICollectionView icv in arr) { icv.Filter = o => { return true; }; }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        public void CreateWorldSaveData(string path)
        {
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    try
                    {
                        foreach (WorldSave ws in BossList.ItemsSource) { sw.WriteLine("BossList;" + ws.ToData()); }
                        foreach (WorldSave ws in VendList.ItemsSource) { sw.WriteLine("VendorList;" + ws.ToData()); }
                        foreach (WorldSave ws in MiscList.ItemsSource) { sw.WriteLine("MiscList;" + ws.ToData()); }
                    }
                    catch (Exception) { }
                }
            }
            else { File.Delete(path); CreateWorldSaveData(path); }

        }
        public long ReadWorldSaveData(string path)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            logMessage("Reading From Index...");
            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                List<WorldSave>
                    Bosses = new List<WorldSave>(),
                    Vendors = new List<WorldSave>(),
                    Other = new List<WorldSave>();
                while ((s = sr.ReadLine()) != null)
                {
                    string[] args = s.Split(';');

                    switch (args[0])
                    {
                        case "BossList": Bosses.Add(WorldSave.FromData(s.Substring(s.IndexOf(';') + 1))); break;
                        case "VendorList": Vendors.Add(WorldSave.FromData(s.Substring(s.IndexOf(';') + 1))); break;
                        case "MiscList":
                            WorldSave ws = WorldSave.FromData(s.Substring(s.IndexOf(';') + 1));
                            if (File.Exists(ws.path)) { Other.Add(ws); }
                            break;
                    }
                }

                BossList.ItemsSource = from b in Bosses orderby b.Name select b;
                VendList.ItemsSource = Vendors;
                MiscList.ItemsSource = Other;

                watch.Stop();
                return watch.ElapsedMilliseconds;
            }

        }




        private void ProfileList_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {

        }

        private void ProfileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfileList.SelectedItem != null)
            {
                SaveProfile.IsEnabled = true;
                LoadProfile.IsEnabled = true;
            }
            else
            {
                SaveProfile.IsEnabled = false;
                LoadProfile.IsEnabled = false;
            }
        }
    }
}

