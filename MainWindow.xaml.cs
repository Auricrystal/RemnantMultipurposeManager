using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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

        private FileSystemWatcher saveWatcher, worldWatcher;
        private string[] saveFiles;
        public static List<InventoryItem> ItemList { get => ActiveCharacter.Inventory; }
        public static string ProfilesDirPath
        {
            get
            {
                if (Properties.Settings.Default.ProfileFolder == null || Properties.Settings.Default.ProfileFolder == "")
                {
                    Directory.CreateDirectory(RBRDirPath + "\\Profiles");
                    Properties.Settings.Default.ProfileFolder = (RBRDirPath + "\\Profiles");
                    Properties.Settings.Default.Save();
                    MW.ProfilePath.Text = Properties.Settings.Default.ProfileFolder;
                    return Properties.Settings.Default.ProfileFolder;
                }
                else
                {
                    MW.ProfilePath.Text = Properties.Settings.Default.ProfileFolder;
                    return Properties.Settings.Default.ProfileFolder;
                }
            }
            set
            {
                if (Directory.Exists(value))
                {
                    Properties.Settings.Default.ProfileFolder = value;
                    Properties.Settings.Default.Save();
                }
                else { MW.logMessage("Not a valid folder", LogType.Error); }
            }
        }
        public static RemnantSave ActiveSave
        {
            get
            {
                if (activeSave == null)
                {
                    activeSave = new RemnantSave(SaveDirPath);
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
                    string[] files = Directory.GetFiles(SaveDirPath, "save_?.sav");
                    for (int i = 0; i < files.Length; i++) { files[i] = files[i].Replace(SaveDirPath + @"\", ""); }
                    saveFiles = files;
                }

                return saveFiles;
            }
        }

        public static RemnantCharacter ActiveCharacter
        {
            get
            {
                try
                {
                    return ActiveSave.Characters[MW.cmbCharacter.SelectedIndex];
                }
                catch (ArgumentOutOfRangeException)
                {
                    Debug.WriteLine("MainWindow ActiveCharacter problem");
                    return new RemnantCharacter(0);
                }
            }
        }
        public string ActiveSaveSlot
        {
            get => (string)cmbSaveSlot.SelectedItem;
            set => cmbSaveSlot.SelectedItem = value;
        }
        private void SetSettings(int Char, int Save)
        {
            int[] set = GetSettings();
            set[Char] = Save;

            Debug.WriteLine(Char + "/" + Save + " | " + string.Join(",", set.Select(x => x.ToString()).ToArray()));
            Properties.Settings.Default.SaveSlotData = string.Join(",", set.Select(x => x.ToString()).ToArray());
            Properties.Settings.Default.Save();
        }
        private int GetSetting(int Char)
        {
            try
            {
                int test = GetSettings()[Char];
                Debug.WriteLine(test);
                return test;
            }

            catch (IndexOutOfRangeException)
            {
                return 0;
            }
        }

        private int[] GetSettings()
        {
            string prop = Properties.Settings.Default.SaveSlotData;
            //if (prop.Split(',').Length != ActiveSave.Characters.Count) { prop = "0,1,2,3,4"; }
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

        public enum LogType { Normal, Success, Error }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Unhandled exception occurred: \n" + e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public MainWindow() : base()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            MW = this;
            InitializeComponent();
            txtLog.IsReadOnly = true;
            DownloadZip("IMG");
            File.Delete(RBRDirPath + "\\log.txt");
            if (File.Exists(SaveDirPath + "\\profile.sav"))
            {
                saveWatcher = new FileSystemWatcher(SaveDirPath, "profile.sav");
                worldWatcher = new FileSystemWatcher(SaveDirPath, "save_*.sav");

                // Watch for changes in LastWrite times.
                saveWatcher.NotifyFilter = NotifyFilters.LastWrite;
                worldWatcher.NotifyFilter = NotifyFilters.LastWrite;

                // Add event handlers.
                saveWatcher.Changed += OnProfileSaveChanged;
                saveWatcher.Created += OnProfileSaveChanged;
                saveWatcher.Deleted += OnProfileSaveChanged;

                worldWatcher.Changed += OnWorldFileChanged;
                worldWatcher.Created += OnWorldFileChanged;
                worldWatcher.Deleted += OnWorldFileChanged;
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(SaveDirPath + "\\profile.sav"))
            {
                Debug.WriteLine("Current save date: " + File.GetLastWriteTime(SaveDirPath + "\\profile.sav").ToString());
                //throw new Exception("test hehehe");
                saveWatcher.EnableRaisingEvents = true;
                worldWatcher.EnableRaisingEvents = true;

                cmbCharacter.ItemsSource = ActiveSave.Characters;
                cmbCharacter.SelectedIndex = 0;
                cmbSaveSlot.ItemsSource = GetSaveFiles;
                cmbSaveSlot.SelectedIndex = GetSetting(cmbCharacter.SelectedIndex);

                Debug.WriteLine(cmbSaveSlot.SelectedItem);
                SetupData();
                UpdateCharacterData();
            }
            else
            {
                SetupData();
                cmbCharacter.IsEnabled = false;
                cmbSaveSlot.IsEnabled = false;
                SaveManipulator.IsEnabled = false;
                KeepCheckpoint.IsEnabled = false;
            }
            checkForUpdate();
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
                catch (Exception) { Debug.WriteLine("Check for update problem"); }
            }
            Debug.WriteLine("Update value: " + compare);
            return compare;
        }
        private void SetupData()
        {
            List<InventoryItem> BossMods = Items.Where(x => x.Mod != null).Select(x => x.Mod).ToList();
            List<InventoryItem>
                empties = Items.Where(x => x.Name.Contains("_")).ToList(),
                molist = Items.Where(x => x.Slot == InventoryItem.SlotType.MO).Except(BossMods).ToList(),
                hglist = Items.Where(x => x.Slot == InventoryItem.SlotType.HG).ToList(),
                lglist = Items.Where(x => x.Slot == InventoryItem.SlotType.LG).ToList(),
                mlist = Items.Where(x => x.Slot == InventoryItem.SlotType.M).ToList(),
                helist = Items.Where(x => x.Slot == InventoryItem.SlotType.HE).ToList(),
                chlist = Items.Where(x => x.Slot == InventoryItem.SlotType.CH).ToList(),
                lelist = Items.Where(x => x.Slot == InventoryItem.SlotType.LE).ToList(),
                amlist = Items.Where(x => x.Slot == InventoryItem.SlotType.AM).ToList(),
                rilist = Items.Where(x => x.Slot == InventoryItem.SlotType.RI).ToList();

            WeaponList.ItemsSource = Combine(hglist, lglist, mlist).Except(empties).ToList();
            ArmorList.ItemsSource = Combine(helist, chlist, lelist).Except(empties).ToList();
            AmuletList.ItemsSource = amlist.Except(empties).ToList();
            RingList.ItemsSource = rilist.Except(empties).ToList();
            ModList.ItemsSource = molist.Except(empties).ToList();

            if (File.Exists(SaveDirPath + "\\profile.sav"))
            {
                ProfileList.ItemsSource = Directory.GetDirectories(ProfilesDirPath).Select(x => new RemnantProfile(x)).ToList();
            }
            else
            {
            }

            EmptySlots.ItemsSource = empties;

            cmbHG.ItemsSource = hglist;
            cmbHGM.ItemsSource = molist;
            cmbLG.ItemsSource = lglist;
            cmbLGM.ItemsSource = molist;
            cmbM.ItemsSource = mlist;
            cmbHE.ItemsSource = helist;
            cmbCH.ItemsSource = chlist;
            cmbLE.ItemsSource = lelist;
            cmbAM.ItemsSource = amlist;
            cmbR1.ItemsSource = rilist;
            cmbR2.ItemsSource = rilist;

            setSelectedIndex(0, cmbHG, cmbHGM, cmbLG, cmbLGM, cmbM, cmbHE, cmbCH, cmbLE, cmbAM, cmbR1, cmbR2);
        }

        private void setSelectedIndex(int val, params ComboBox[] cmbs)
        {
            foreach (ComboBox cmb in cmbs) { cmb.SelectedIndex = val; }
        }

        private String DownloadZip(string zipName)
        {
            try
            {
                string zipURL = "https://raw.githubusercontent.com/Auricrystal/RemnantBuildRandomizer/master/Resources/" + zipName + ".zip";
                using (WebClient client = new WebClient())
                {
                    if (!File.Exists(RBRDirPath + "\\" + zipName + ".zip"))
                    {
                        client.DownloadFile(zipURL, RBRDirPath + "\\" + zipName + ".zip");
                    }
                    else
                    {
                        using (ZipArchive Old = ZipFile.Open(RBRDirPath + "\\" + zipName + ".zip", ZipArchiveMode.Update), New = new ZipArchive(client.OpenRead(zipURL)))
                        {
                            foreach (ZipArchiveEntry entry in New.Entries)
                            {
                                //If new file does not exist or the new file is smaller in size than the old
                                if (entry.Name != "")
                                {
                                    if (Old.GetEntry(entry.FullName) == null || entry.Length < Old.GetEntry(entry.FullName).Length || entry.LastWriteTime.Ticks > Old.GetEntry(entry.FullName).LastWriteTime.Ticks)
                                    {
                                        logMessage("New " + zipName + " Package Update!\nDownloading!");
                                        Old.Dispose(); New.Dispose();
                                        File.Delete(RBRDirPath + "\\" + zipName + ".zip"); client.DownloadFile(zipURL, RBRDirPath + "\\" + zipName + ".zip");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                return RBRDirPath + "\\" + zipName + ".zip";
            }
            catch (Exception) { logMessage("Problem accessing internet.", LogType.Error); return null; }
        }
        private void DownloadNewProfile(bool rewards, string path)
        {
            string profilesave = "https://raw.githubusercontent.com/Auricrystal/RemnantBuildRandomizer/master/Resources/NewProfile";
            using (WebClient client = new WebClient())
            {
                if (!File.Exists(path))
                {
                    client.DownloadFile(profilesave + (rewards ? "Rewards" : "") + ".sav", path + "\\profile.sav");
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
            catch (Exception) { Debug.WriteLine("MainWindow WorldFile problem"); }
        }

        private static List<T> Combine<T>(params IEnumerable<T>[] rils)
        {
            List<T> li = new List<T>();
            foreach (IEnumerable<T> ril in rils) { li.AddRange(ril); }
            return li;
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

            this.Dispatcher.Invoke(() =>
            {
                if (KeepCheckpoint.IsChecked == true)
                {
                    LoadCheckpoint(RBRDirPath + "\\Backup\\Keep.sav");
                }
            });
            UpdateCharacterData();
        }

        private void UpdateCharacterData()
        {
            ActiveSave.UpdateCharacters();
            this.Dispatcher.Invoke(() =>
            {
                SaveTime.Text = "Last Save: " + File.GetLastWriteTime(SaveDirPath + "\\profile.sav");
            });
        }

        private void DisplayBuild(Build b)
        {
            setImage(HandGunImg, HandGunText, b.HandGun);
            setImage(HandModImg, HandGunModText, b.HandGun.Mod);
            setImage(LongGunImg, LongGunText, b.LongGun);
            setImage(LongModImg, LongGunModText, b.LongGun.Mod);
            setImage(MeleeImg, MeleeText, b.Melee);
            setImage(HeadImg, HeadText, b.Head);
            setImage(ChestImg, ChestText, b.Chest);
            setImage(LegImg, LegText, b.Legs);
            setImage(AmImg, AmuletText, b.Amulet);
            setImage(Ri1Img, Ring1Text, b.Ring1);
            setImage(Ri2Img, Ring2Text, b.Ring2);
        }
        private void setImage(Image i, TextBlock tb, InventoryItem ri)
        {
            i.Source = GearInfo.GetImage(ZipFile.OpenRead(RBRDirPath + "\\IMG.zip"), ri.IMG);
            tb.Text = ri.Name;
        }

        private void Conditions(Build b)
        {
            if (b.Amulet.Name == "White Rose")
            {
                string text = "\n\nWHITE ROSE EFFECT\n";
                Debug.WriteLine(text);
                InventoryItem hg = Items.Find(x => x.Name == "_No Hand Gun");
                InventoryItem lg = Items.Find(x => x.Name == "_No Long Gun");
                if (rd.Next(2) == 1) { setImage(HandGunImg, HandGunText, hg); setImage(HandModImg, HandGunModText, hg.Mod); HandCoverModImg.ToolTip = null; text += "removed HG\n"; }
                if (rd.Next(2) == 1) { setImage(LongGunImg, LongGunText, lg); setImage(LongModImg, LongGunModText, lg.Mod); LongCoverModImg.ToolTip = null; text += "removed LG"; }
            }
            else if (b.Amulet.Name == "Daredevil's Charm")
            {
                string text = "\n\nDDC EFFECT\n";
                Debug.WriteLine(text);
                if (rd.Next(2) == 1) { setImage(HeadImg, HeadText, Items.Find(x => x.Name == "_No Head")); text += "removed Head\n"; }
                if (rd.Next(2) == 1) { setImage(ChestImg, ChestText, Items.Find(x => x.Name == "_No Chest")); text += "removed Chest\n"; }
                if (rd.Next(2) == 1) { setImage(LegImg, LegText, Items.Find(x => x.Name == "_No Legs")); text += "removed Legs"; }
            }
            if (b.Ring1.Name.ToLower() == "Ring Of The Unclean".ToLower() || b.Ring2.Name.ToLower() == "Ring Of The Unclean".ToLower() ||
                b.Ring1.Name.ToLower() == "Five Fingered Ring".ToLower() || b.Ring2.Name.ToLower() == "Five Fingered Ring".ToLower())
            {
                Debug.WriteLine("ROTU or FFR Effect");
                if (rd.Next(2) == 1) { setImage(MeleeImg, MeleeText, Items.Find(x => x.Name == "_Fists")); }
            }
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
            if (ActiveSave.Characters.Count > 0)
            {
                string path = RBRDirPath + @"/Data.txt";
                File.Delete(path);
            }
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
            catch (IndexOutOfRangeException) { Debug.WriteLine("MainWindow SaveSlot problem"); }
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
                    //BuildList.ItemsSource = Presets[ActiveCharacter.charNum];
                }
                catch (KeyNotFoundException) { 
                   // Presets.Add(ActiveCharacter.charNum, new List<Build>()); 
                }

                //Debug.WriteLine("CHANGED TO " + ActiveSave.Characters[cmbCharacter.SelectedIndex].ToString());
                var rc = ActiveSave.Characters[cmbCharacter.SelectedIndex];
                //Debug.WriteLine("Inv:" + rc.Progression + ", Miss:" + rc.GetMissingItems().Count);
                //foreach (RemnantItem ri in StrToRI.Values)
                //{
                //    ri.Character = cmbCharacter.SelectedIndex;
                //}
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
        
        private void WeaponList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();
            //Cancel the column you don't want to generate
            switch (headername)
            {
                case "Data":
                case "Character": e.Cancel = true; break;

                case "Name":
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

                case "Name":
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

                case "Name":
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
            InventoryItem edit = ((InventoryItem)e.Row.Item);
            logMessage("Edited item: " + edit);
        }
        private static void DeleteEmptyFolders(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                DeleteEmptyFolders(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
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

        private void cmbHG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InventoryItem hg = (InventoryItem)cmbHG.SelectedItem;
            cmbHGM.SelectedIndex = 0;
            if (hg.Mod != null)
            {
                cmbHGM.ItemsSource = new List<InventoryItem>() {hg.Mod};
            }
            else
            {
                cmbHGM.ItemsSource = 
                    ItemList.Where(x => x.Slot == InventoryItem.SlotType.MO)
                    .Except(ItemList.Where(x => x.Mod != null).Select(x => x.Mod)).ToList();
            }
            cmbHGM.SelectedIndex = 0;
            cmbHGM.Items.Refresh();
        }

        private void cmbLG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InventoryItem lg = (InventoryItem)cmbLG.SelectedItem;
            cmbLGM.SelectedIndex = 0;
            if (lg.Mod != null)
            {
                cmbLGM.ItemsSource = new List<InventoryItem>() { lg.Mod };
            }
            else
            {
                cmbLGM.ItemsSource =
                    ItemList.Where(x => x.Slot == InventoryItem.SlotType.MO)
                    .Except(ItemList.Where(x => x.Mod != null).Select(x => x.Mod)).ToList();
            }
            cmbLGM.SelectedIndex = 0;
            cmbLGM.Items.Refresh();
        }

        private void cmbR1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbR2.ItemsSource = ItemList.Where(x => (x != (InventoryItem)cmbR1.SelectedItem && x.Slot == InventoryItem.SlotType.RI) || x.Name == "_No Ring");
        }

        private void cmbR2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbR1.ItemsSource = ItemList.Where(x => (x != (InventoryItem)cmbR2.SelectedItem && x.Slot == InventoryItem.SlotType.RI) || x.Name == "_No Ring");
        }

        private void cmbHGM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbLGM.Items.Count > 1)
            {
                if (cmbLGM.SelectedIndex > 27) { cmbHGM.SelectedIndex = 0; }
                cmbLGM.ItemsSource = 
                    ItemList.Where(x => x != (InventoryItem)cmbHGM.SelectedItem && x.Slot == InventoryItem.SlotType.MO || x.Name == "_No Mod")
                    .Except(ItemList.Where(x => x.Mod != null).Select(x => x.Mod)).ToList();
            }
        }

        private void cmbLGM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbHGM.Items.Count > 1)
            {
                if (cmbHGM.SelectedIndex > 27) { cmbHGM.SelectedIndex = 0; }

                cmbHGM.ItemsSource = 
                    ItemList.Where(x => x != (InventoryItem)cmbLGM.SelectedItem && x.Slot == InventoryItem.SlotType.MO || x.Name == "_No Mod")
                    .Except(ItemList.Where(x => x.Mod != null).Select(x => x.Mod)).ToList();
            }
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
            //List<Build> builds = Presets[slot];

            //List<Build> list = builds.Where(x => x.Disabled == false).ToList();
           // if (list.Count > 0 && rd.Next(100) <= badluck)
            {
               // badluck = 0;
               // Build b = list[rd.Next(list.Count)];
               // DisplayBuild(b);

                //BuildNum.Text = b.BuildName;
                //BuildNum.ToolTip = b.toStringCode();
            }
            //else
            {
                //if (list.Count > 0)
                //{
                  //  badluck++;
                   // Debug.WriteLine(badluck);
               // }
                Build rand = Build.Random(ItemList);
                DisplayBuild(rand);
                Conditions(rand);
                //Debug.WriteLine(Code(rand.toCodeArray()));

                //BuildNum.Text = Code(rand.toCodeArray());
                BuildNum.ToolTip = null;
            }
        }
        private void Add_Build_Click(object sender, RoutedEventArgs e)
        {
            /*
            try
            {
                if (BuildNameEnter.Text == "") { throw new Exception("No BuildName Assigned to Build!"); }
                //Build b = new Build(BuildNameEnter.Text, BuildCodeEnter.Text);
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
            */
        }
        private void CopyBuild_Click(object sender, RoutedEventArgs e)
        {
            CopyUIElementToClipboard(BuildScreen);
            MessageBox.Show("Copied to Clipboard!");
        }
        private void ResetBlack_Click(object sender, RoutedEventArgs e)
        {
            //StrToRI.Values.ToList().ForEach((x) => { x.No = false; });
           //safeRefresh(WeaponList, ArmorList, AmuletList, RingList, ModList, EmptySlots);
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
            /*
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
            */
        }
        private void BuildCode_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(BuildNum.Text);
        }
        private void DataFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(RBRDirPath);
        }

        private void CopyBuildObj_Click(object sender, System.EventArgs e)
        {
            if (BuildList.SelectedItem != null)
            {
                Build selectedBuild = (Build)(BuildList.SelectedItem);
                //logMessage("Grabbing Build Object of (" + selectedBuild.BuildName + ")");
                //Clipboard.SetText(selectedBuild.ToData().Trim());
                BuildList.SelectedItem = null;
            }
        }
        private void PasteBuildObj_Click(object sender, System.EventArgs e)
        {
            try
            {
                int slot;
                if (File.Exists(SaveDirPath + "\\profile.sav")) { slot = ActiveCharacter.charNum; } else { slot = 0; }
                //Build b = Build.FromData(Clipboard.GetText().Trim());
                //logMessage("Pasting Build (" + b + ")");
                //if (!Presets[slot].Contains(b)) { Presets[slot].Add(b); safeRefresh(BuildList); }
                //else { logMessage("Build (" + b + ") already exists!"); }
            }
            catch (Exception err) { logMessage(err.Message); }
        }
        private void DisableEmpties_Click(object sender, RoutedEventArgs e)
        {
            //StrToRI.Values.Where(x => x.Itemname.Contains("_")).ToList().ForEach((x) => { x.No = true; });
            //EmptySlots.Items.Refresh();
        }

        private void GameRestart_Click(object sender, RoutedEventArgs e)
        {
            RestartGame();
        }

        private void CreateProfile_Click(object sender, RoutedEventArgs e)
        {
            int num = 0;
            Debug.WriteLine(ProfilesDirPath + "\\NewProfile" + num);
            while (Directory.Exists(ProfilesDirPath + "NewProfile" + num)) { num++; }
            Directory.CreateDirectory(ProfilesDirPath + "\\" + "NewProfile" + num);
            DownloadNewProfile(false, ProfilesDirPath + "\\" + "NewProfile" + num);
            ProfileList.ItemsSource = Directory.GetDirectories(ProfilesDirPath).Select(x => new RemnantProfile(x)).ToList();
            ProfileList.Items.Refresh();
        }
        private void CreateProfileRewards_Click(object sender, RoutedEventArgs e)
        {

            int num = 0;
            Debug.WriteLine(ProfilesDirPath + "\\NewProfile" + num);
            while (Directory.Exists(ProfilesDirPath + "\\NewProfile" + num)) { Debug.WriteLine(ProfilesDirPath + "\\NewProfile" + num); num++; }
            Directory.CreateDirectory(ProfilesDirPath + "\\" + "NewProfile" + num);
            DownloadNewProfile(true, ProfilesDirPath + "\\" + "NewProfile" + num);
            ProfileList.ItemsSource = Directory.GetDirectories(ProfilesDirPath).Select(x => new RemnantProfile(x)).ToList();
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
                File.Copy(ProfilesDirPath + "\\" + foldername + "\\profile.sav", saveDirPath + "\\profile.sav", true);
                logMessage("Successfully Loaded " + foldername, LogType.Success);
                RestartGame();
                ProfileList.ItemsSource = Directory.GetDirectories(ProfilesDirPath).Select(x => new RemnantProfile(x)).ToList();
                ProfileList.Items.Refresh();
            }
        }
        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            string foldername = ((RemnantProfile)ProfileList.SelectedItem).Profile;
            //ProfilesDirPath = ProfilePath.Text;
            Debug.WriteLine("Stored Profile: " + ProfilePath.Text);
            var confirmResult = MessageBox.Show("Are you sure you want to overwrite: " + foldername + "?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (confirmResult == MessageBoxResult.Yes)
            {
                File.Copy(ProfilesDirPath + "\\" + foldername + "\\profile.sav", ProfilesDirPath + "\\" + foldername + "\\profile.bak", true);
                File.Copy(saveDirPath + "\\profile.sav", ProfilesDirPath + "\\" + foldername + "\\profile.sav", true);
                logMessage("Successfully Overwrote " + foldername, LogType.Success);
                ProfileList.ItemsSource = Directory.GetDirectories(ProfilesDirPath).Select(x => new RemnantProfile(x)).ToList();
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
                //Build.FromData(Clipboard.GetText());
            }
            catch (Exception)
            {
                Debug.WriteLine("BuildList Problem");
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

            string header = e.Column.Header.ToString();
            switch (header)
            {
                case "Filepath":
                case "Data":
                case "Description": e.Cancel = true; break;
            }
            e.Column.IsReadOnly = true;
        }
        private void VendorList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();
            switch (header)
            {
                case "Modifiers":
                case "Filepath":
                case "Diff":
                case "Data":
                case "Description": e.Cancel = true; break;
            }
            e.Column.IsReadOnly = true;
        }
        private void EventList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();
            switch (header)
            {
                case "Filepath":
                case "Data":
                case "Description":
                case "Modifiers": e.Cancel = true; break;
            }
            e.Column.IsReadOnly = true;
        }
        private void MiscList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();
            switch (header)
            {
                case "Filepath":
                case "Data":
                case "Description": e.Cancel = true; break;
            }
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

            LoadSave.IsEnabled = AlterFile.IsChecked.Value && !KeepCheckpoint.IsChecked.Value;
            FeelingLucky.IsEnabled = AlterFile.IsChecked.Value && !KeepCheckpoint.IsChecked.Value;
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
            var process = Process.GetProcessesByName("Remnant-Win64-Shipping");
            if (process.Length > 0)
            {
                string path = process[0].MainModule.FileName;
                Debug.WriteLine(process.Length + " Remnant Is Running! " + path + " PID:" + process[0].Id);
                var confirmResult = MessageBox.Show("Restart Game?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (confirmResult == MessageBoxResult.Yes)
                {
                    while (!process[0].WaitForExit(1000)) { process[0].Kill(); }
                    Process.Start(path.Contains("steam") ? "steam://rungameid/617290" : path);
                }
            }
            else
            {
                Debug.WriteLine("Remnant Is NOT Running!");
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void ProfileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool b = ProfileList.SelectedItem != null;
            SaveProfile.IsEnabled = b;
            LoadProfile.IsEnabled = b;
            DeleteProfile.IsEnabled = b;
        }

        private void btnProfileChoose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            openFolderDialog.SelectedPath = ProfilesDirPath;
            openFolderDialog.Description = "Select the folder where you want your profiles kept.";
            System.Windows.Forms.DialogResult result = openFolderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string folderName = openFolderDialog.SelectedPath;
                if (folderName.Equals(saveDirPath))
                {
                    MessageBox.Show("Please select a folder other than the game's save folder.",
                                     "Invalid Folder", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                    return;
                }
                if (folderName.Equals(ProfilesDirPath))
                {
                    return;
                }
                if (Directory.GetDirectories(ProfilesDirPath).ToList().Count > 0)
                {
                    var confirmResult = MessageBox.Show("Do you want to move your Profiles to this new folder?",
                                     "Move Profiles", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    if (confirmResult == MessageBoxResult.Yes)
                    {
                        List<String> backupFiles = Directory.GetDirectories(ProfilesDirPath).ToList();
                        foreach (string file in backupFiles)
                        {
                            if (File.Exists(file + @"\profile.sav"))
                            {
                                string subFolderName = file.Substring(file.LastIndexOf(@"\"));
                                Directory.CreateDirectory(folderName + subFolderName);
                                Directory.SetCreationTime(folderName + subFolderName, Directory.GetCreationTime(file));
                                Directory.SetLastWriteTime(folderName + subFolderName, Directory.GetCreationTime(file));
                                foreach (string filename in Directory.GetFiles(file))
                                {
                                    File.Copy(filename, filename.Replace(ProfilesDirPath, folderName));
                                }
                                Directory.Delete(file, true);
                            }
                        }
                    }
                }
                ProfilePath.Text = folderName;
                ProfilesDirPath = folderName;
                ProfileList.ItemsSource = Directory.GetDirectories(ProfilesDirPath).Select(x => new RemnantProfile(x)).ToList();
                ProfileList.Items.Refresh();
            }
        }

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            string foldername = ((RemnantProfile)ProfileList.SelectedItem).Profile;
            //ProfilesDirPath = ProfilePath.Text;
            Debug.WriteLine("Stored Profile: " + ProfilePath.Text);
            var confirmResult = MessageBox.Show("Are you sure you want to delete: " + foldername + "?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (confirmResult == MessageBoxResult.Yes)
            {
                Directory.Delete(ProfilesDirPath + "\\" + foldername, true);
                logMessage("Successfully Deleted " + foldername, LogType.Success);
                ProfileList.ItemsSource = Directory.GetDirectories(ProfilesDirPath).Select(x => new RemnantProfile(x)).ToList();
                ProfileList.Items.Refresh();
            }
        }

        private void lblLastMessage_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Debug.WriteLine("Log Clicked!");
            MainTab.SelectedIndex = 4;
        }

        private void CreateRBB_Click(object sender, RoutedEventArgs e)
        {

            File.WriteAllText(@"C:\Users\AuriCrystal\Desktop\Inventory.txt", JsonConvert.SerializeObject(ActiveCharacter.Collected));
        }

        private void LoadSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FeelingLucky_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

