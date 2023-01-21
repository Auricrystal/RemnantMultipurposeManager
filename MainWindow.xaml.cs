using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using static RemnantMultipurposeManager.JWorldSave;

namespace RemnantMultipurposeManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string GameSavePath { get => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Remnant\\Saved\\SaveGames"; }
        public static MainWindow Instance = null;
        readonly public static Random rd = new Random();
        public enum LogType { Normal, Success, Error }
        private FileSystemWatcher ProfileWatcher, SaveWatcher;
        private readonly InventoryUI UI;
        private RemnantProfile profile;
        private List<JWorldSave> checkpoints;
        private byte[] lockedSave;
        private int MAX_LOG_STACK = 10;
        //test comment
        public RemnantProfile Profile
        {
            get
            {
                if (profile != null) { return profile; }
                string s;
                if (File.Exists(s = Properties.Settings.Default.CurrentProfile))
                {
                    Debug.WriteLine("Profile exists! " + s);
                    profile = JsonConvert.DeserializeObject<RemnantProfile>(File.ReadAllText(s), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                    return profile;
                }
                else
                {
                    Properties.Settings.Default.CurrentProfile = null;
                    Properties.Settings.Default.Save();
                    return null;
                }

            }
        }
        public List<JWorldSave> Checkpoints
        {
            get
            {
                DownloadDirectory();
                checkpoints = JWorldSave.LoadList(RmmInstallPath + @"\CheckpointDirectory.rmm");
                return checkpoints;
            }
        }

        public static string RmmInstallPath
        {
            get
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\RemnantMultipurposeManager");
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\RemnantMultipurposeManager";
            }
        }

        public MainWindow() : base()
        {

            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            Instance = this;
            InitializeComponent();
            txtLog.IsReadOnly = true;


            if (!File.Exists(RmmInstallPath + "\\" + "IMG.zip"))
                DownloadZip("IMG");
            checkForUpdate();

            BuildUI.Child = (UI = new InventoryUI());
            var list = Directory.GetFiles(RmmInstallPath + "\\Logs");

            for (int i = 0; i < list.Length - MAX_LOG_STACK; i++)
                File.Delete(list[i]);
            



        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            if (Debugger.IsAttached)
            {
                LogMessage("Local Creator Detected");
                ReadRMM.IsEnabled = CreateRMM.IsEnabled = true;
                ReadRMM.Visibility = CreateRMM.Visibility = Visibility.Visible;
            }

            if (Profile is null)
            {
                SetSaveManagerActive(false);
                return;
            }
            //Debug.WriteLine("checkbox " + Properties.Settings.Default.OfflineAccess);
            OfflineFileAccess.IsChecked = Properties.Settings.Default.OfflineAccess;
            SetSaveManagerActive(true);
            BuildList.ItemsSource = Profile.Builds[cmbCharacter.SelectedIndex];
            BuildList.Items.Refresh();

        }
        private void OnProfileSaveChanged(object source, FileSystemEventArgs e)
        {
            Debug.WriteLine("Profile Updated");
            Profile.PackProfile(GameSavePath + @"\profile.sav");

            Profile.Save(Properties.Settings.Default.CurrentProfile);
        }
        private void RerollClick(object sender, RoutedEventArgs e)
        {
            Build b = null;
            if (Profile != null)
            {
                Debug.WriteLine("Profile Inventory: " + Profile.Characters[cmbCharacter.SelectedIndex].Inventory.Count);
                var NoEmpties = EquipmentDirectory.Items.Where(x => x.Name.Contains("_")).ToList();
                b = Profile.Characters[cmbCharacter.SelectedIndex].Inventory.Select(x => EquipmentDirectory.FindEquipmentByName(x)).RandomBuild(UI.Shown, false ? NoEmpties : null);
            }
            else
            {
                b = EquipmentDirectory.Items.ToList().RandomBuild(UI.Shown, EquipmentDirectory.Items.Where(x => x.Name.Contains("_")).ToList());
            }
            UI.EquipBuild(b);
            //Profile?.Builds[0]?.Add(UI.Shown);
        }
        private void DownloadDirectory()
        {
            string Directory = "https://raw.githubusercontent.com/Auricrystal/RemnantMultipurposeManager/Experimental-FileFormat/Resources/Data/CheckpointDirectory.rmm";

            using (WebClient client = new WebClient())
            {


                if (!File.Exists(RmmInstallPath + @"\CheckpointDirectory.rmm"))
                {
                    client.DownloadFile(Directory, RmmInstallPath + @"\CheckpointDirectory.rmm");
                    return;
                }
                if (OfflineFileAccess.IsChecked.Value)
                    return;

                try
                {
                    using (StreamReader nw = new StreamReader(client.OpenRead(Directory)), od = new StreamReader(File.OpenRead(RmmInstallPath + @"\CheckpointDirectory.rmm")))
                    {

                        string a;
                        while ((a = nw.ReadLine()) != null)
                        {
                            if (a != od.ReadLine())
                            {
                                nw.Close();
                                od.Close();
                                client.DownloadFile(Directory, RmmInstallPath + @"\CheckpointDirectory.rmm");
                                return;
                            }
                        }
                    }
                }
                catch (WebException) { Debug.WriteLine("Offline Issues"); }
            }
        }

        public void UnbindProfile(object sender, RoutedEventArgs e)
        {

            Properties.Settings.Default.CurrentProfile = null;
            profile = null;
            Properties.Settings.Default.Save();
            SetSaveManagerActive(false);
        }
        public void SetSaveManagerActive(bool b)
        {
            if (Profile == null && b)
            {
                LogMessage("Profile Not Set", LogType.Error);
                return;
            }



            if (ProfileWatcher is null && b)
            {

                ProfileWatcher = new FileSystemWatcher()
                {
                    Path = GameSavePath,
                    Filter = "profile.sav",
                    NotifyFilter = NotifyFilters.LastWrite,
                };
                ProfileWatcher.Changed += OnProfileSaveChanged;
            }

            if (ProfileWatcher is not null)
                ProfileWatcher.EnableRaisingEvents = b;

            SaveManager.IsEnabled = b;
            if (!b && MainTab.SelectedIndex == 1)
                MainTab.SelectedIndex = 0;


            SaveManager.ToolTip = b ? null : "You need to create an (*.RProfile) to use this.";
            //SaveManager.Visibility = b ? Visibility.Visible : Visibility.Collapsed;

            LoadedProfile.Header = b ? profile.Name : "No Profile";
            LoadedProfile.Visibility = b ? Visibility.Visible : Visibility.Collapsed;

            cmbCharacter.IsEnabled = b;
            cmbSaveSlot.IsEnabled = b;

            cmbCharacter.ItemsSource = b ? profile.Characters : null;
            cmbCharacter.SelectedIndex = b ? 0 : -1;

            cmbSaveSlot.ItemsSource = b ? profile.SavePair.Keys.Select(x => "Slot " + x) : null;
            cmbSaveSlot.SelectedIndex = b ? profile.SavePair[0] : -1;

            DefaultCharacter.IsEnabled = b;

            cmbCharacter.Items.Refresh();
            cmbSaveSlot.Items.Refresh();

            InvList.ItemsSource = EquipmentDirectory.Items;
            SaveList.ItemsSource = b ? Checkpoints : null;
            cmbSaveType.SelectedIndex = b ? 0 : -1;
        }

        private enum BackupAction { Backup, Revert };
        private BackupAction lastBackupAction = BackupAction.Revert;
        private void BackupRevert(BackupAction b)
        {
            if (b == BackupAction.Backup)
            {
                AlterFile.Content = "Revert Saves";
                lastBackupAction = BackupAction.Backup;
                string[] saves = Directory.GetFiles(GameSavePath, "save_?.sav");
                Directory.CreateDirectory(RmmInstallPath + @"\Backup");
                foreach (string s in saves)
                {
                    File.Copy(s, RmmInstallPath + @"\Backup\" + s.Split('\\').Last(), true);
                }

            }
            else
            {
                if (lastBackupAction == BackupAction.Revert)
                    return;
                AlterFile.Content = "Backup Saves";
                lastBackupAction = BackupAction.Revert;
                string[] saves = Directory.GetFiles(RmmInstallPath + @"\Backup", "save_?.sav");
                foreach (string s in saves)
                {
                    File.Copy(s, GameSavePath + "\\" + s.Split('\\').Last(), true);
                }
                Directory.Delete(RmmInstallPath + @"\Backup", true);
            }

        }
        private void RemnantBuildRandomizer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BackupRevert(BackupAction.Revert);
            string s;
            if (File.Exists(s = RmmInstallPath + @"\Logs\log.txt"))
            {
                File.Move(s, RmmInstallPath + @"\Logs\" + File.GetLastWriteTime(s).Ticks + ".txt");
            }
        }


        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            BackupRevert(BackupAction.Revert);
            MessageBox.Show("Exception Message: " + e.Exception.Message + "\n\n" + "Exception StackTrace:" + e.Exception.StackTrace, "Unknown Crash Occurred", MessageBoxButton.OK, MessageBoxImage.Error);

            LogMessage(e.Exception.Message + "\n" + e.Exception.StackTrace);
            string s;
            if (File.Exists(s = RmmInstallPath + @"\Logs\log.txt"))
            {
                File.Move(s, RmmInstallPath + @"\Logs\" + File.GetLastWriteTime(s).Ticks + ".txt");
            }

        }

        private int checkForUpdate()
        {
            int compare = 0;
            using (WebClient client = new())
            {
                try
                {
                    string source = client.DownloadString("https://github.com/Auricrystal/RemnantMultipurposeManager/releases/latest");
                    string title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                    string remoteVer = Regex.Match(source, @"Remnant Multipurpose Manager (?<Version>([\d.]+)?)", RegexOptions.IgnoreCase).Groups["Version"].Value + ".0";

                    Version remoteVersion = new Version(remoteVer);
                    Version localVersion = typeof(MainWindow).Assembly.GetName().Version;
                    Debug.WriteLine("remote:" + remoteVersion + '\n' + "local:" + localVersion);
                    this.Dispatcher.Invoke(() =>
                    {
                        string tt = "No Update Found";
                        string bmptxt = @"/Resources/IMG/Menu/_CurrentVersion.png";
                        compare = localVersion.CompareTo(remoteVersion);
                        if (compare == -1)
                        {
                            bmptxt = "/Resources/IMG/Menu/_OutOfDateVersion.png";
                            tt = "Update Available!";
                        }
                        else if (compare == 1)
                        {
                            bmptxt = "/Resources/IMG/Menu/_BetaVersion.png";
                            tt = "Beta Version";
                        }
                        Version.Header = new Image { Source = new BitmapImage(new Uri(@"pack://application:,,," + bmptxt)) };
                        Version.ToolTip = tt;
                        LogMessage(tt);
                    }
                     );
                }
                catch (WebException)
                {
                    Version.Header = new Image { Source = new BitmapImage(new Uri(@"pack://application:,,," + @"/Resources/IMG/Menu/_CurrentVersion.png")) };
                    Version.ToolTip = "Online Access Restricted or Off";
                    Instance.LogMessage("Online Access Restricted or Off", LogType.Error);
                }
            }
            Debug.WriteLine("Update value: " + compare);
            return compare;
        }
        public static bool DownloadZip(string zipName)
        {
            try
            {
                string zipURL = "https://raw.githubusercontent.com/Auricrystal/RemnantMultipurposeManager/master/Resources/" + zipName + ".zip";
                using (WebClient client = new WebClient())
                {

                    if (!File.Exists(RmmInstallPath + "\\" + zipName + ".zip"))
                    {
                        client.DownloadFile(zipURL, RmmInstallPath + "\\" + zipName + ".zip");
                        Instance.LogMessage("Pulling " + zipName + " from Repo", LogType.Success);

                        return true;
                    }


                    using (ZipArchive Old = ZipFile.Open(RmmInstallPath + "\\" + zipName + ".zip", ZipArchiveMode.Update), New = new ZipArchive(client.OpenRead(zipURL)))
                    {
                        foreach (ZipArchiveEntry entry in New.Entries)
                        {
                            //If new file does not exist or the new file is smaller in size than the old
                            if (entry.Name == "" ||
                                Old.GetEntry(entry.FullName) != null ||
                                entry.Length >= Old.GetEntry(entry.FullName).Length ||
                                entry.LastWriteTime.Ticks <= Old.GetEntry(entry.FullName).LastWriteTime.Ticks)
                                continue;

                            Instance.LogMessage("Updating " + zipName + " from Repo", LogType.Success);
                            Old.Dispose(); New.Dispose();
                            File.Delete(RmmInstallPath + "\\" + zipName + ".zip"); client.DownloadFile(zipURL, RmmInstallPath + "\\" + zipName + ".zip");
                            break;
                        }
                    }
                }
                return true;
            }
            catch (Exception) { Instance.LogMessage("Problem accessing internet.", LogType.Error); return false; }
        }
        public void LogMessage(string msg, LogType lt = LogType.Normal, Color? color = null)
        {

            switch (lt)
            {
                case LogType.Success: color = Color.FromRgb(0, 200, 0); break;
                case LogType.Error: color = Color.FromRgb(200, 0, 0); break;
                default: break;
            }

            if (!Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => { LogMessage(msg, color: color); });
                return;
            }


            txtLog.Text = txtLog.Text + Environment.NewLine + DateTime.Now.ToString() + ": " + msg;
            lblLastMessage.Text = msg;
            lblLastMessage.ToolTip = null;
            lblLastMessage.Foreground = new SolidColorBrush(color ?? Colors.White);
            if (color.Equals(Colors.White))
            {
                lblLastMessage.FontWeight = FontWeights.Normal;
            }
            else
            {
                lblLastMessage.FontWeight = FontWeights.Bold;
            }
            Directory.CreateDirectory(RmmInstallPath + "\\Logs");
            StreamWriter writer = System.IO.File.AppendText(RmmInstallPath + "\\Logs\\log.txt");
            writer.WriteLine(DateTime.Now.ToString() + ": " + msg);
            writer.Close();

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

        private void CmbSaveSlot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCharacter.SelectedItem == null)
            {
                Debug.WriteLine("Char Null");
                return;
            }


            RemnantCharacter rc = (RemnantCharacter)cmbCharacter.SelectedItem;
            Profile.SavePair[rc.Slot] = cmbSaveSlot.SelectedIndex;
            Debug.WriteLine("Char:" + rc.Slot + " Set to:" + cmbSaveSlot.SelectedIndex);
            if (SaveWatcher is null || !SaveWatcher.EnableRaisingEvents)
                return;
            Debug.WriteLine("Switching Locked Save");
            SaveWatcher.Filter = "save_" + cmbSaveSlot.SelectedIndex + ".sav";
            lockedSave = File.ReadAllBytes(GameSavePath + @"\save_" + cmbSaveSlot.SelectedIndex + ".sav");
        }
        private void CmbCharacter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemnantCharacter rc = (RemnantCharacter)cmbCharacter.SelectedItem;
            cmbSaveSlot.SelectedIndex = (rc != null) ? Profile?.SavePair[rc.Slot] ?? -1 : -1;
            BuildList.ItemsSource = Profile.Builds[cmbCharacter.SelectedIndex];
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


        #region Click Events

        private void UpdateCheck_Click(object sender, RoutedEventArgs e)
        {
            if (checkForUpdate() != -1)
                return;
            var confirmResult = MessageBox.Show("There is a new version available. Would you like to open the download page?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (confirmResult == MessageBoxResult.No)
                return;
            Process.Start("https://github.com/Auricrystal/RemnantMultipurposeManager/releases/latest");
            System.Environment.Exit(1);
        }

        private void InstallFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(RmmInstallPath);
        }

        private void GameRestart_Click(object sender, RoutedEventArgs e)
        {
            RestartGame();
        }

        #endregion

        private void LockCheckpoint_Checked(object sender, RoutedEventArgs e)
        {
            if (SaveWatcher is null)
            {
                Debug.WriteLine("Making new save watcher");
                SaveWatcher = new FileSystemWatcher() { Path = GameSavePath, Filter = "save_" + cmbSaveSlot.SelectedIndex + ".sav", NotifyFilter = NotifyFilters.LastWrite };
                SaveWatcher.Changed += LockedSave_Changed;
            }
            SaveWatcher.EnableRaisingEvents = true;
            if (File.Exists(GameSavePath + @"\save_" + cmbSaveSlot.SelectedIndex + ".sav"))
                lockedSave = File.ReadAllBytes(GameSavePath + @"\save_" + cmbSaveSlot.SelectedIndex + ".sav");
            //Debug.WriteLine("Bytes: " + lockedSave.Length);
            LoadButtonSettings();
        }

        private void LockedSave_Changed(object sender, FileSystemEventArgs e)
        {
            SaveWatcher.EnableRaisingEvents = false;
            this.Dispatcher.Invoke(() =>
            {
                if (lockedSave is null)
                    lockedSave = File.ReadAllBytes(GameSavePath + @"\save_" + cmbSaveSlot.SelectedIndex + ".sav");

                try
                {
                    Debug.WriteLine("Overwriting!");
                    File.WriteAllBytes(GameSavePath + @"\save_" + cmbSaveSlot.SelectedIndex + ".sav", lockedSave);
                }
                catch (IOException ex)
                {
                    if (ex.Message.Contains("being used by another process"))
                    {
                        Console.WriteLine("Save file in use; waiting 0.5 seconds and retrying.");

                        Thread.Sleep(500);
                        LockedSave_Changed(sender, e);
                    }
                }
            });
            SaveWatcher.EnableRaisingEvents = true;
        }

        private void LockCheckpoint_Unchecked(object sender, RoutedEventArgs e)
        {
            SaveWatcher.EnableRaisingEvents = false;
            LoadButtonSettings();
        }

        private void LoadButtonSettings()
        {
            LoadSave.IsEnabled = !LockCheckpoint.IsChecked.Value && SaveList.SelectedIndex != -1;
            FeelingLucky.IsEnabled = !LockCheckpoint.IsChecked.Value;
        }
        private void AlterFile_Clicked(object sender, RoutedEventArgs e)
        {
            if (lastBackupAction == BackupAction.Backup)
            {
                BackupRevert(BackupAction.Revert);
            }
            else
            {
                BackupRevert(BackupAction.Backup);
            }
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

        private void lblLastMessage_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainTab.SelectedIndex = 3;
        }

        private void CreateMasterRMM()
        {
            //throw new Exception();

            string baseDir = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug", @"Resources\");



            string s = "";
            if (File.Exists(s = baseDir + "SaveLibrary.zip"))
                File.Delete(s);

            //var f = zipName == "Vendors2" ? "Vendors" : zipName;

            ZipFile.CreateFromDirectory(baseDir + "SaveLibrary", s, CompressionLevel.Optimal, false);

            using (ZipArchive zipFile = ZipFile.Open(s, ZipArchiveMode.Read))
            {
                string savetype = "";
                string diff = "";
                string planet = "";
                string bossname = "";
                string modifier = "";

                foreach (ZipArchiveEntry entry in zipFile.Entries)
                {
                    if (!entry.FullName.Contains("save.sav"))
                        continue;
                    Debug.WriteLine(entry.FullName);
                    string[] temp = entry.FullName.Split('/');
                   
                    if (savetype != temp[0])
                        savetype = temp[0];
                    if (diff != temp[1])
                        diff = temp[1];
                    if (planet != temp[2])
                        planet = temp[2];
                    if (bossname != temp[3])
                        bossname = temp[3];
                    if (modifier != temp[4])
                        modifier = temp[4];

                    

                    MergeRMM(new(
                            bossname,
                            TryParse<WorldZone>(string.Join("", planet.Split(' '))),
                            TryParse<SaveType>(savetype),
                            new Dictionary<JWorldSave.DifficultyType, List<string>>()
                            {
                              {
                                TryParse<DifficultyType>(diff=="No Difficulty"?"Vendor":diff) , new List<string>(){modifier}
                              }
                            }
                        )
                        ); 
                }
                zipFile.Dispose();

            }
        }

        private void MergeRMM(JWorldSave save)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug", @"Resources\");

            var savelist = JWorldSave.LoadList(baseDir + @"Data\CheckpointDirectory.rmm");
            if (!savelist.Select(x => (x.Name, x.World, x.Type)).Contains((save.Name, save.World, save.Type)))
            {
                Debug.WriteLine("Inserting: (" + String.Join(",", save.Type, save.World, save.Name) + ")");
                savelist.Add(save);
            }
            else
            {
                //Debug.WriteLine("Test Merge");
                JWorldSave match = savelist.Find(x => x.Name == save.Name);
                foreach (var diff in save.Checkpoints.Keys)
                {
                    if (!match.Checkpoints.ContainsKey(diff))
                        match.Checkpoints.Add(diff, new List<string>());
                    foreach (var line in save.Checkpoints[diff])
                    {
                        if (match.Checkpoints[diff].Contains(line))
                            continue;
                        match.Checkpoints[diff].Add(line);
                        Debug.WriteLine("Adding " + save.Name + "[" + diff + "," + line + "]");
                    }
                }
            }
            JWorldSave.Save(baseDir + @"Data\CheckpointDirectory.rmm", savelist.OrderBy(x => x.Type).ThenBy(x => x.World).ThenBy(x => x.Name).ToArray());
        }
        private void CreateRMM_Click(object sender, RoutedEventArgs e)
        {
            CreateMasterRMM();

        }
        private void ReadRMM_Click(object sender, RoutedEventArgs e)
        {
            string path;
            string baseDir = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug", @"Resources\");
            if (!File.Exists(path = baseDir + @"Data\CheckpointDirectory.rmm"))
                return;
            var save = JWorldSave.LoadList(path);

            Debug.WriteLine("RMM Count:" + save.Count);

            foreach (var item in save.OrderBy(x => x.World).ThenBy(x => x.Name))
                Debug.WriteLine(item.World + ": " + item.Name);
        }
        private void LoadSave_Click(object sender, RoutedEventArgs e)
        {
            if (lastBackupAction != BackupAction.Backup)
            {
                BackupRevert(BackupAction.Backup);
            }

            if (SaveList.SelectedIndex == -1)
                return;

            ProfileWatcher.EnableRaisingEvents = false;
            File.SetLastWriteTime(GameSavePath + "\\profile.sav", DateTime.Now);
            if (OfflineFileAccess.IsChecked.Value)
            {
                Save.grabFileFromZip(GameSavePath + "\\save_" + cmbSaveSlot.SelectedIndex + ".sav", Diff, Mod);
            }
            else
            {
                Save.DownloadFile(GameSavePath + "\\save_" + cmbSaveSlot.SelectedIndex + ".sav", Diff, Mod);
            }
            ProfileWatcher.EnableRaisingEvents = true;
        }

        private void FeelingLucky_Click(object sender, RoutedEventArgs e)
        {
            if (SaveList.ItemsSource is null)
                return;

            FeelingLucky.IsEnabled = LoadSave.IsEnabled = false;
            BackupRevert(BackupAction.Backup);
            ProfileWatcher.EnableRaisingEvents = false;
            var s = GameSavePath + "\\save_" + cmbSaveSlot.SelectedIndex + ".sav";
            Thread t = new Thread(() =>
            {
                //Thread.Sleep(2000);
                var save = ((List<JWorldSave>)SaveList.ItemsSource).Where(x => x.Type == JWorldSave.SaveType.Bosses).Shuffle().First();
                File.SetLastWriteTime(GameSavePath + "\\profile.sav", DateTime.Now);
                if (!Properties.Settings.Default.OfflineAccess)
                {
                    save.DownloadFile(s, JWorldSave.DifficultyType.Apocalypse, save.Checkpoints[JWorldSave.DifficultyType.Apocalypse].Shuffle().First());
                }
                else
                {
                    save.grabFileFromZip(s, JWorldSave.DifficultyType.Apocalypse, save.Checkpoints[JWorldSave.DifficultyType.Apocalypse].Shuffle().First());
                }
                Action action = () => FeelingLucky.IsEnabled = LoadSave.IsEnabled = ProfileWatcher.EnableRaisingEvents = true;
                this.Dispatcher.BeginInvoke(action);
            });
            t.Start();
        }

        private void CopyBuild_Click(object sender, RoutedEventArgs e)
        {
            CopyUIElementToClipboard(UI);
            MessageBox.Show("Copied to Clipboard!");
        }

        private void CreateCharacter_Click(object sender, RoutedEventArgs e)
        {
            using (var read = new System.Windows.Forms.OpenFileDialog())
            {

                read.Title = "Read Profile File";
                read.Filter = "|profile.sav";

                read.InitialDirectory = GameSavePath;
                var res = read.ShowDialog();

                if (read.FileName == "" || res == System.Windows.Forms.DialogResult.Cancel)
                    return;
                if (!File.Exists(read.FileName))
                    return;

                var profile = new RemnantProfile(read.FileName);



                //foreach (RemnantCharacter rc in profile.Characters)
                //{
                //    Debug.WriteLine(rc.ToString());
                //}
                using (var save = new System.Windows.Forms.SaveFileDialog())
                {
                    save.Title = "Save RProfile";
                    save.Filter = "(.RProfile)|*.RProfile";
                    save.FileName = "Default";
                    //save.DefaultExt = "RProfile";
                    res = save.ShowDialog();
                    if (save.FileName == "" || res == System.Windows.Forms.DialogResult.Cancel)
                        return;
                    Debug.WriteLine("FILENAME: " + save.FileName);
                    profile.Name = Path.GetFileNameWithoutExtension(save.FileName);
                    profile.Save(save.FileName);

                    Properties.Settings.Default.CurrentProfile = save.FileName;
                    Properties.Settings.Default.Save();
                }
                SetSaveManagerActive(true);
            }
        }
        private void LoadCharacter_Click(object sender, RoutedEventArgs e)
        {
            if (Process.GetProcessesByName("Remnant-Win64-Shipping").Length > 0 && Profile is not null)
            {
                //string path = process[0].MainModule.FileName;
                // Debug.WriteLine(process.Length + " Remnant Is Running! " + path + " PID:" + process[0].Id);
                MessageBox.Show("Loading an (*.RProfile) with the game still running will cause you to lose saved data!\nPlease close the game before trying again.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            using (var read = new System.Windows.Forms.OpenFileDialog())
            {
                read.Title = "Read RProfile File";
                read.Filter = "|*.RProfile";

                read.InitialDirectory = Path.GetDirectoryName(Properties.Settings.Default.CurrentProfile) ?? GameSavePath;
                read.ShowDialog();

                if (read.FileName == "")
                    return;

                var profile = RemnantProfile.Load(read.FileName);

                Properties.Settings.Default.CurrentProfile = read.FileName;
                Properties.Settings.Default.Save();

                profile.UnpackProfile(GameSavePath + @"\profile.sav");

                SetSaveManagerActive(true);

            }
        }

        JWorldSave Save = null;
        JWorldSave.DifficultyType Diff = JWorldSave.DifficultyType.Unknown;
        string Mod = "";
        private void SaveList_Modifier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListView)sender)?.SelectedItem == null)
                return;
            var s = ((ListView)sender)?.SelectedItem?.ToString();
            if (s == Mod)
                return;
            Mod = s;
            Debug.WriteLine("Modifier Changed! " + s);

        }

        private void SaveList_Difficulty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListView)sender)?.SelectedItem == null)
                return;
            var s = ((ListView)sender)?.SelectedItem.ToString();
            JWorldSave.DifficultyType st = (JWorldSave.DifficultyType)Enum.Parse(typeof(JWorldSave.DifficultyType), s);
            if (st == Diff)
                return;
            Diff = st;
            Debug.WriteLine("Difficulty Changed! " + s);

            if (SaveListDifficulty.ItemsSource == null)
                return;

            ListView cmb = (ListView)sender;
            if (cmb.SelectedItem == null)
                return;


            if (SaveList.SelectedItem == null)
                return;

            JWorldSave save = SaveList.SelectedItem as JWorldSave;

            SaveListModifier.ItemsSource = save.Checkpoints[st];
            SaveListModifier.SelectedIndex = 0;
        }

        private void SaveList_BossName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((DataGrid)sender)?.SelectedItem == null)
                return;
            var s = ((JWorldSave)((DataGrid)sender)?.SelectedItem);

            if (s.Equals(Save))
                return;
            Save = s;
            Debug.WriteLine("Boss Changed! " + s.Name);
            SaveListDifficulty.ItemsSource = s.Checkpoints.Keys;
            if (SaveListDifficulty.ItemsSource == null)
                return;


            if (cmbSaveType.SelectedIndex != 2)
            {
                ICollectionView icv2 = CollectionViewSource.GetDefaultView(SaveListDifficulty.ItemsSource);
                icv2.Filter = o =>
                {

                    var dt = (o as JWorldSave.DifficultyType?);
                    if (dt is null)
                        return false;
                    return !dt.Equals(JWorldSave.DifficultyType.Vendor) && s.Checkpoints[dt.Value].Count > 0;
                };
            }
            SaveListDifficulty.SelectedIndex = 0;
            JWorldSave save = SaveList.SelectedItem as JWorldSave;

            JWorldSave.DifficultyType st = (JWorldSave.DifficultyType)Enum.Parse(typeof(JWorldSave.DifficultyType), SaveListDifficulty.SelectedItem.ToString());
            SaveListModifier.ItemsSource = save.Checkpoints[st];

            if (SaveListModifier.ItemsSource == null)
                return;

            LoadButtonSettings();
            SaveListModifier.SelectedIndex = 0;
        }

        private void OfflineFileAccess_Toggle(object sender, RoutedEventArgs e)
        {
            if (OfflineFileAccess.IsChecked.Value && !Properties.Settings.Default.OfflineAccess)
                DownloadZip("SaveLibrary");
            Properties.Settings.Default.OfflineAccess = OfflineFileAccess.IsChecked.Value;
            Properties.Settings.Default.Save();
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            InvList.IsEnabled = !InvList.IsEnabled;
            InvList.Visibility = InvList.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
            AddBuild.Visibility = InvList.Visibility;
            BuildList.Visibility = InvList.IsEnabled ? Visibility.Collapsed : Visibility.Visible;
            CollectionViewSource.GetDefaultView(InvList.ItemsSource).Filter = o => false;
            UI.EquipBuild(new Build());

        }

        private void AddBuild_Click(object sender, RoutedEventArgs e)
        {
            if (Profile is null)
                return;

            Debug.WriteLine("Build Shown:\n" + UI.Shown.ToString());

            Profile.Builds[cmbCharacter.SelectedIndex].Add(UI.Shown);
            Debug.WriteLine("Build List: " + Profile.Builds[cmbCharacter.SelectedIndex].Count);
            BuildList.ItemsSource = Profile.Builds[cmbCharacter.SelectedIndex];
            BuildList.Items.Refresh();
            Profile.Save(Properties.Settings.Default.CurrentProfile);
        }
        private InventorySlot _SelectedInventorySlot = null;

        public void InventorySlot_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _SelectedInventorySlot = (e.OriginalSource as FrameworkElement).Parent as InventorySlot ?? (InventorySlot)sender;

            Debug.WriteLine(_SelectedInventorySlot.Item);

            ICollectionView icv = CollectionViewSource.GetDefaultView(InvList.ItemsSource);
            icv.Filter = o =>
            {
                var dt = (o as Equipment);
                if (dt is null)
                    return false;

                Mod mod = null;
                if (_SelectedInventorySlot.SlotType == Equipment.SlotType.MO && (mod = ((Mod)_SelectedInventorySlot.Item)) is not null && mod.Boss)
                    return false;

                return dt.Slot.Equals(_SelectedInventorySlot.SlotType);
            };

        }

        private void InvList_DoubleClick(object sender, SelectionChangedEventArgs e)
        {
            if (InvList.SelectedItem is null)
                return;
            if (_SelectedInventorySlot is null)
                return;


            Equipment copy = EquipmentDirectory.FindEquipmentByName(((Equipment)InvList.SelectedItem).Name);

            if (CanDoEquip(_SelectedInventorySlot, copy))
            {
                if (_SelectedInventorySlot is GunSlot)
                    ((GunSlot)_SelectedInventorySlot).Equip((Gun)(copy as Gun).Clone());
                else if (_SelectedInventorySlot is ModSlot)
                    ((ModSlot)_SelectedInventorySlot).Equip((Mod)copy);
                else
                    _SelectedInventorySlot.Equip(copy);
            }
            else
                Debug.WriteLine("Cant Equip Duplicate already Equipped");
        }

        private bool CanDoEquip(InventorySlot slot, Equipment item)
        {

            bool check1 = false, check2 = false;
            if (slot.SlotType is Equipment.SlotType.RI)
            {
                check1 = UI.Ring1.Item is not null && !UI.Ring1.Item.Equals(item);
                check2 = UI.Ring2.Item is not null && !UI.Ring2.Item.Equals(item);
                //Debug.WriteLine("Ring check: " + (check1 && check2));
                return check1 && check2;
            }
            if (slot.SlotType is Equipment.SlotType.MO)
            {
                check1 = UI.HandGunMod.Item is not null && !UI.HandGunMod.Item.Equals(item);
                check2 = UI.LongGunMod.Item is not null && !UI.LongGunMod.Item.Equals(item);
                //Debug.WriteLine("Mod check: " + (check1 && check2));
                if (item.Name.Contains("_") && !((Mod)item).Boss)
                    return true;
                return check1 && check2;
            }
            if (slot.Item.Name == item.Name)
                return false;
            return true;
        }

        private void BuildList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BuildList.SelectedItem is null)
                return;
            UI.EquipBuild((Build)BuildList.SelectedItem);
        }

        private void CmbSaveType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SaveList.ItemsSource == null)
                return;

            ComboBox cmb = (ComboBox)sender;
            if (cmb.SelectedItem == null)
                return;

            JWorldSave.SaveType st = (JWorldSave.SaveType)Enum.Parse(typeof(JWorldSave.SaveType), ((ComboBoxItem)cmb.SelectedItem).Content.ToString());

            ICollectionView icv = CollectionViewSource.GetDefaultView(SaveList.ItemsSource);

            icv.Filter = o => { return ((o as JWorldSave).Type.Equals(st)); };

            SaveListDifficulty.ItemsSource = null;
            SaveListModifier.ItemsSource = null;
            LoadButtonSettings();


        }

    }
}

