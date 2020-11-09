using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static RemnantBuildRandomizer.GearInfo;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using static RemnantBuildRandomizer.DataObj;
using System.IO.Compression;
using System.ComponentModel;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Text;

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

        private Assembly assembly;
        //List<string> BossData;

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
        public RemnantCharacter ActiveCharacter { get => ActiveSave.Characters[cmbCharacter.SelectedIndex]; }
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
            return GetSettings()[Char];
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
            ProfileManager.IsEnabled = false;
            ProfileManager.ToolTip = "Not Implemented Yet.";
            DownloadIMGFiles();
            assembly = Assembly.GetExecutingAssembly();
            File.Delete(RBRDirPath + "\\log.txt");
            for (int i = 0; i < 5; i++)
            {
                logMessage("Char:" + i + " Save:" + GetSetting(i));
            }

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
                saveWatcher.Changed += OnSaveFileChanged;
                saveWatcher.Created += OnSaveFileChanged;
                saveWatcher.Deleted += OnSaveFileChanged;

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

                //ActiveSave = new RemnantSave(SaveDirPath);

                cmbCharacter.ItemsSource = ActiveSave.Characters;
                cmbCharacter.SelectedIndex = 0;
                cmbSaveSlot.ItemsSource = GetSaveFiles;
                cmbSaveSlot.SelectedIndex = GetSetting(cmbCharacter.SelectedIndex);

                Debug.WriteLine(cmbSaveSlot.SelectedItem);
            }
            else
            {
                cmbCharacter.IsEnabled = false;
                SaveManipulator.IsEnabled = false;
                KeepCheckpoint.IsEnabled = false;
            }


            ReadXML();
            GetData();

            checkForUpdate();

            SetupData();
            if (File.Exists(SaveDirPath + "\\profile.sav"))
            {
                UpdateCharacterData();
                disablemissing();
            }

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
                BuildList.ItemsSource = Presets[ActiveCharacter.charNum].ToList();

                var watch = System.Diagnostics.Stopwatch.StartNew();
                SaveFileInit();
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Debug.WriteLine("SETUP TIME: " + elapsedMs);
                BossList.Items.Refresh();
                VendList.Items.Refresh();
                MiscList.Items.Refresh();
            }
            else { BuildList.ItemsSource = Presets[0].ToList(); }
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
                boss = ParseSaveFilesAsync(DownloadZip("Bosses2")),
                vend = ParseSaveFilesAsync(DownloadZip("Vendors")),
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
                    Debug.WriteLine("SaveCount: " + WorldSave.getSave.Count);
                    Debug.WriteLine("Time:" + watch.ElapsedMilliseconds);
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
            //logMessage("World save " + e.Name + " was written to");

        }

        private List<WorldSave> ParseSaveFiles(string directory)
        {
            List<WorldSave> list = new List<WorldSave>();
            if (Directory.Exists(directory))
            {
                string[] files = Directory.GetFiles(directory);
                foreach (string s in files)
                {
                    if (s.Contains(".zip"))
                    {
                        list.AddRange(ParseSaveFilesAsync(s));
                    }
                    else if (s.Contains(".sav"))
                    {
                        list.Add(WorldSave.Parse(s));
                    }
                    else
                    {
                        Debug.WriteLine("Not Supported" + s);
                    }
                }
            }
            return list;
        }



        public List<WorldSave> ParseSaveFilesAsync(string directory)
        {
            List<Task<WorldSave>> tasks = new List<Task<WorldSave>>();
            List<string> names = new List<string>();
            using (ZipArchive za = ZipFile.OpenRead(directory))
            {
                var orderedEntries =
                from entry in za.Entries
                orderby entry.Length
                select entry;
                names = orderedEntries.Where(x => x.FullName.Contains(".sav")).Select(x => x.FullName).ToList();
            }

            foreach (string s in names)
            {
                try
                {
                    tasks.Add(Task.Run(() =>
                    {
                        using (ZipArchive za = ZipFile.OpenRead(directory))
                        {
                            using (StreamReader reader = new StreamReader(za.GetEntry(s).Open(), Encoding.UTF8))
                            {

                                string file = reader.ReadToEnd();
                                string[] name = za.GetEntry(s).Name.Replace(".sav", "").Split('_');

                                WorldSave ws = new WorldSave(directory + '|' + za.GetEntry(s).FullName, WorldSave.FindDiff(file), WorldSave.FindWorld(file), name[0], name.Skip(1).ToArray());
                                WorldSave.addWS(ws);
                                return ws;

                            }
                        }
                    }));
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }
            }
            List<WorldSave> data = new List<WorldSave>();
            foreach (Task<WorldSave> task in tasks)
            {
                // Debug.WriteLine("Adding: " + task.Result.Name);
                data.Add(task.Result);
            }
            return data;
        }
        private void LoadSave()
        {


            BossList.SelectedIndex = rd.Next(BossList.Items.Count);
            LoadSave((WorldSave)BossList.SelectedItem);
        }
        private void LoadSave(WorldSave rb)
        {
            logMessage("Loading:\n" + rb.path.Split('\\', '/').Last());
            if (rb.path.Contains(".zip"))
            {
                grabFileFromZip(rb.path, SaveDirPath + "\\" + ActiveSaveSlot);
            }
            else
            {
                File.Copy(rb.path, SaveDirPath + "\\" + ActiveSaveSlot, true);
            }

        }



        private static List<T> Combine<T>(params IEnumerable<T>[] rils)
        {
            List<T> li = new List<T>();
            foreach (IEnumerable<T> ril in rils) { li.AddRange(ril); }
            return li;
        }

        private void checkForUpdate()
        {
            if (!Properties.Settings.Default.ShowedUpdate)
            {
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;

                    try
                    {
                        WebClient client = new WebClient();
                        string source = client.DownloadString("https://github.com/Auricrystal/RemnantBuildRandomizer/releases/latest");
                        string title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                        string remoteVer = Regex.Match(source, @"Remnant Build Randomizer (?<Version>([\d.]+)?)", RegexOptions.IgnoreCase).Groups["Version"].Value;

                        Version remoteVersion = new Version(remoteVer);
                        Version localVersion = typeof(MainWindow).Assembly.GetName().Version;

                        this.Dispatcher.Invoke(() =>
                        {
                            //do stuff in here with the interface
                            if (localVersion.CompareTo(remoteVersion) == -1)
                            {
                                var confirmResult = MessageBox.Show("There is a new version available. Would you like to open the download page?",
                                         "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                                if (confirmResult == MessageBoxResult.Yes)
                                {
                                    Process.Start("https://github.com/Auricrystal/RemnantBuildRandomizer/releases/latest");
                                    System.Environment.Exit(1);
                                }
                                else { Properties.Settings.Default.ShowedUpdate = true; Properties.Settings.Default.Save(); }
                            }
                            else
                            {
                                logMessage("No new version found.");
                            }
                        });
                    }
                    catch (Exception)
                    {
                    }
                }).Start();

                lastUpdateCheck = DateTime.Now;
            }
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
                lblLastMessage.Content = msg;
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


        private void OnSaveFileChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.


            KeepCheck();
            UpdateCharacterData();

        }

        private void KeepCheck()
        {
            if (!Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (KeepCheckpoint.IsChecked == true)
                    {
                        LoadCheckpoint();
                    }
                });
            }
            else
            {
                if (KeepCheckpoint.IsChecked == true)
                {
                    LoadCheckpoint();
                }
            }
        }

        private void UpdateCharacterData()
        {
            ActiveSave.UpdateCharacters();
            if (!Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() =>
                {
                    SaveTime.Text = "Last Save: " + File.GetLastWriteTime(SaveDirPath + "\\profile.sav");
                });
            }
            else
            {
                SaveTime.Text = "Last Save: " + File.GetLastWriteTime(SaveDirPath + "\\profile.sav");
            }

            if (!Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() =>
                {
                    //cmbCharacter.ItemsSource = ActiveSave.Characters;
                    // cmbCharacter.SelectedIndex = 0;
                    //cmbCharacter.Items.Refresh();
                });
            }
            else
            {
                //cmbCharacter.ItemsSource = ActiveSave.Characters;
                //cmbCharacter.SelectedIndex = 0;
                //cmbCharacter.Items.Refresh();
            }


        }


        private void Reroll_Click(object sender, RoutedEventArgs e)
        {
            int slot;
            if (File.Exists(SaveDirPath + "\\profile.sav"))
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
        }


        private void RemnantBuildRandomizer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //MessageBox.Show("Closing called");
            if (AlterFile.IsChecked == true)
            {
                foreach (string s in Directory.GetFiles(RBRDirPath + "\\Backup\\", "save_*.sav"))
                {
                    File.Copy(s, saveDirPath + '\\' + s.Split('\\').Last(), true);
                    logMessage("Reverting " + s.Split('\\').Last());
                }
            }
            SaveData();
        }



        private void Add_Build_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BuildNameEnter.Text == "") { throw new Exception("No BuildName Assigned to Build!"); }
                Build b = new Build(BuildNameEnter.Text, BuildCodeEnter.Text);
                int slot;
                if (File.Exists(SaveDirPath + "\\profile.sav"))
                {
                    slot = ActiveCharacter.charNum;
                }
                else { slot = 0; }
                if (!Presets[slot].Contains(new Build(BuildNameEnter.Text, BuildCodeEnter.Text)))
                {
                    BuildNameEnter.Text = "";
                    BuildCodeEnter.Text = "";

                    Presets[slot].Add(b);
                    BuildList.ItemsSource = Presets[slot];
                    Debug.WriteLine("Preset count" + Presets[slot].Count);
                    BuildList.Items.Refresh();
                }
                BuildList.Items.Refresh();
            }
            catch (Exception ce)
            {
                MessageBox.Show(ce.Message);
            }
            BuildList.Items.Refresh();
        }
        private void CopyBuild_Click(object sender, RoutedEventArgs e)
        {
            CopyUIElementToClipboard(BuildScreen);
            MessageBox.Show("Copied to Clipboard!");
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
            SetSettings(cmbCharacter.SelectedIndex, cmbSaveSlot.SelectedIndex);

        }
        private void CmbCharacter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (cmbCharacter.SelectedIndex == -1 && ActiveSave.Characters.Count > 0) return;
            if (cmbCharacter.Items.Count > 0 && cmbCharacter.SelectedIndex > -1)
            {
                cmbSaveSlot.SelectedIndex = GetSetting(cmbCharacter.SelectedIndex);
                safeCommit(WeaponList, ArmorList, AmuletList, RingList, ModList, BuildList, EmptySlots);
                BuildList.ItemsSource = Presets[ActiveCharacter.charNum];
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

        private void ResetBlack_Click(object sender, RoutedEventArgs e)
        {
            foreach (RemnantItem ri in StrToRI.Values)
            {
                ri.No = false;
            }
            safeRefresh(WeaponList);
            safeRefresh(ArmorList);
            safeRefresh(AmuletList);
            safeRefresh(RingList);
            safeRefresh(ModList);
            safeRefresh(EmptySlots);

        }

        private void UpdateCheck_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ShowedUpdate = false;
            Properties.Settings.Default.Save();
            checkForUpdate();
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


        private void BuildList_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }
        private void RemItem_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            RemnantItem edit = ((RemnantItem)e.Row.Item);
            logMessage("Beginning edit: " + edit);

        }

        private void RemItem_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            RemnantItem edit = ((RemnantItem)e.Row.Item);
            logMessage("Edited item: " + edit);
        }

        private void RemItem_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }

        private void GenBuildCode_Click(object sender, RoutedEventArgs e)
        {
            RemnantItem hg = (RemnantItem)cmbHG.SelectedItem;
            RemnantItem hgm = (RemnantItem)cmbHGM.SelectedItem;
            RemnantItem lg = (RemnantItem)cmbLG.SelectedItem;
            RemnantItem lgm = (RemnantItem)cmbLGM.SelectedItem;
            RemnantItem m = (RemnantItem)cmbM.SelectedItem;
            RemnantItem he = (RemnantItem)cmbHE.SelectedItem;
            RemnantItem ch = (RemnantItem)cmbCH.SelectedItem;
            RemnantItem le = (RemnantItem)cmbLE.SelectedItem;
            RemnantItem am = (RemnantItem)cmbAM.SelectedItem;
            RemnantItem r1 = (RemnantItem)cmbR1.SelectedItem;
            RemnantItem r2 = (RemnantItem)cmbR2.SelectedItem;


            BuildCodeEnter.Text = new Build("", hg, hgm, lg, lgm, m, he, ch, le, am, r1, r2).toStringCode();

            cmbHG.SelectedIndex = 0;
            cmbHGM.SelectedIndex = 0;
            cmbLG.SelectedIndex = 0;
            cmbLGM.SelectedIndex = 0;
            cmbM.SelectedIndex = 0;
            cmbHE.SelectedIndex = 0;
            cmbCH.SelectedIndex = 0;
            cmbLE.SelectedIndex = 0;
            cmbAM.SelectedIndex = 0;
            cmbR1.SelectedIndex = 0;
            cmbR2.SelectedIndex = 0;

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

        private void BuildCode_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(BuildNum.Text);
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
                if (File.Exists(SaveDirPath + "\\profile.sav"))
                {
                    slot = ActiveCharacter.charNum;
                }
                else { slot = 0; }
                Build b = Build.FromData(Clipboard.GetText().Trim());
                logMessage("Pasting Build (" + b + ")");
                if (!Presets[slot].Contains(b))
                {
                    Presets[slot].Add(b);
                    safeRefresh(BuildList);
                }
                else
                {
                    logMessage("Build (" + b + ") already exists!");
                }
            }
            catch (Exception err)
            {
                logMessage(err.Message);
            }
        }


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
                case "Modifier1": e.Column.Header = "Consumable1"; break;
                case "Modifier2": e.Column.Header = "Consumable2"; break;
            }


            e.Column.IsReadOnly = true;
        }

        private void DisableEmpties_Click(object sender, RoutedEventArgs e)
        {
            List<RemnantItem> empties = StrToRI.Values.Where(x => x.Itemname.Contains("_")).ToList();
            foreach (RemnantItem ri in empties)
            {
                ri.No = true;
            }
            EmptySlots.Items.Refresh();
        }



        private void LoadCheckpoint()
        {
            LoadCheckpoint(RBRDirPath + "\\Backup\\KeepSave.sav");
        }
        private void LoadCheckpoint(string path)
        {
            try
            {
                Debug.WriteLine("Path: " + path);
                File.Copy(path, SaveDirPath + "\\" + ActiveSaveSlot, true);

                logMessage("Loading Checkpoint: " + path);
            }
            catch (IOException ex)
            {
                if (ex.Message.Contains("being used by another process"))
                {
                    Console.WriteLine("Save file in use; waiting 0.5 seconds and retrying.");

                    Thread.Sleep(5000);
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


        private void RerollBoss_Click(object sender, RoutedEventArgs e)
        {
            KeepCheck();
        }

        private void KeepCheckpoint_Checked(object sender, RoutedEventArgs e)
        {
            File.Copy(saveDirPath + "\\" + ActiveSaveSlot, RBRDirPath + "\\Backup\\Keep.sav", true);
        }

        private void LoadBoss_Click(object sender, RoutedEventArgs e)
        {
            TabItem t = ((TabItem)SaveManager.SelectedItem);
            DataGrid dg = (DataGrid)t.Content;
            if (dg.SelectedIndex > -1)
            {
                WorldSave rb = ((WorldSave)dg.SelectedItem);
                LoadSave(rb);
            }
        }

        private void AlterFile_Checked(object sender, RoutedEventArgs e)
        {
            foreach (string s in Directory.GetFiles(saveDirPath, "save_*.sav"))
            {
                File.Copy(s, RBRDirPath + "\\Backup\\" + s.Split('\\').Last(), true);
                logMessage("Backing Up " + s.Split('\\').Last());
            }

        }

        private void AlterFile_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (string s in Directory.GetFiles(RBRDirPath + "\\Backup\\", "save_*.sav"))
            {
                File.Copy(s, saveDirPath + '\\' + s.Split('\\').Last(), true);
                logMessage("Reverting "+ s.Split('\\').Last());
            }

        }

        private void FeelingLucky_Click(object sender, RoutedEventArgs e)
        {
            LoadSave();
        }

        private void BossListName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void FileName_Click(object sender, RoutedEventArgs e)
        {
            logMessage(BossList.SelectedItem.ToString());

        }
        private void GameRestart_Click(object sender, RoutedEventArgs e)
        {
            RestartGame();

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

        private void AddFiles_Click(object sender, RoutedEventArgs e)
        {

            if (!Directory.Exists(RBRDirPath + "\\Misc Saves")) { Directory.CreateDirectory(RBRDirPath + "\\Misc Saves"); }
            OpenFileDialog view = new OpenFileDialog();
            view.DefaultExt = ".sav";
            view.Filter = "World Saves (.sav)|*.sav";
            view.Multiselect = true;

            if (view.ShowDialog() == true)
            {
                var list = view.FileNames.Where(x => File.ReadAllText(x).Contains("RemnantSaveGame_C"));
                Debug.WriteLine(string.Join("\n", list));
                foreach (string s in list)
                {
                    string localfile = s.Split(new char[] { '\\', '|', '/' }).Last();
                    File.Copy(s, RBRDirPath + "\\Misc Saves\\" + localfile, true);
                }
                var parse = ParseSaveFiles(RBRDirPath + "\\Misc Saves\\");
                Debug.WriteLine("Get Check Name:" + parse[0].Name);
                MiscList.ItemsSource = parse;
                MiscList.Items.Refresh();
            }
        }


        private WorldSave temp;
        private void MiscList_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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
                if (temp != null)
                {
                    Debug.WriteLine(temp.path + "\n" + rb.fullpath());
                    if (rb.path.Contains(".zip"))
                    {
                        using (ZipArchive z = ZipFile.Open(rb.path.Split('|').First(), ZipArchiveMode.Update))
                        {
                            z.RenameEntry(temp.path.Split('|').Last(), rb.fullpath().Split('|').Last());
                        }
                    }
                    else
                    {

                        Debug.WriteLine(rb.fullpath());
                        File.Move(temp.path, rb.fullpath().Replace("\n", ""));
                    }

                    temp = null;
                }
            }
        }


        private void MiscList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string name = e.Column.Header.ToString();

            switch (name)
            {
                case "World":
                case "Diff": e.Column.IsReadOnly = true; break;
            }

        }

        private void MiscList_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            WorldSave rb = (WorldSave)MiscList.SelectedItem;
            temp = rb.Copy();
            Debug.WriteLine("EDIT STARTING");
        }

        private void OutputData_Click(object sender, RoutedEventArgs e)
        {
            using (StreamWriter sw = File.CreateText(RBRDirPath + '\\' + "TestData.txt"))
            {
                List<string> data = ((WorldSave)BossList.SelectedItem).ReadFile();
                foreach (string s in data)
                {
                    sw.WriteLine(s);
                }
            }

        }
    }
}

