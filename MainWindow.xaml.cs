using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;



namespace RemnantMultipurposeManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string GameSavePath { get => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Remnant\\Saved\\SaveGames"; }
        public static MainWindow MW = null;
        readonly public static Random rd = new Random();
        public enum LogType { Normal, Success, Error }
        private readonly InventoryUI UI;
        private RemnantProfile profile;
        private List<JWorldSave> checkpoints;
        //test comment
        public RemnantProfile Profile
        {
            get
            {
                if (profile != null) { return profile; }
                string s;
                if (File.Exists(s = Properties.Settings.Default.CurrentProfile))
                {
                    profile = JsonConvert.DeserializeObject<RemnantProfile>(File.ReadAllText(s));
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
                if (checkpoints == null)
                    checkpoints = new List<JWorldSave>();
                if (checkpoints.Count == 0)
                    checkpoints = JWorldSave.LoadList(@"C:\Users\AuriCrystal\Documents\VisualProjects\RemnantMultipurposeManager\Resources\Data\CheckpointDirectory.rmm");
                return checkpoints;
            }
        }

        public MainWindow() : base()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            MW = this;
            InitializeComponent();
            txtLog.IsReadOnly = true;
            DownloadZip("IMG");
            checkForUpdate();
            DownloadZip("Events");
            File.Delete(RBRDirPath + "\\log.txt");

            BuildUI.Child = (UI = new InventoryUI());

        }
        public void UnbindProfile(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CurrentProfile = null;
            profile = null;
            Properties.Settings.Default.Save();
            cmbCharacter.ItemsSource = null;
            cmbSaveSlot.ItemsSource = null;
            cmbCharacter.IsEnabled = false;
            cmbSaveSlot.IsEnabled = false;
            FileManager.IsEnabled = false;
            KeepCheckpoint.IsEnabled = false;
            LoadedProfile.Header = "No Profile";
            LoadedProfile.Visibility = Visibility.Collapsed;
            UpdateCharacter.IsEnabled = false;

        }
        public void BindProfile(RemnantProfile profile)
        {
            FileManager.IsEnabled = true;
            KeepCheckpoint.IsEnabled = true;
            LoadedProfile.Header = profile.Name;
            cmbCharacter.ItemsSource = profile.Characters;
            cmbCharacter.SelectedIndex = 0;
            cmbSaveSlot.ItemsSource = profile.SavePair.Keys.Select(x => "Slot " + x);
            cmbSaveSlot.SelectedIndex = profile.SavePair[0];
            cmbCharacter.IsEnabled = true;
            cmbSaveSlot.IsEnabled = true;
            cmbCharacter.Items.Refresh();
            cmbSaveSlot.Items.Refresh();
            UpdateCharacter.IsEnabled = true;
            SaveList.ItemsSource = Checkpoints;
            cmbSaveType.SelectedIndex = 0;



        }

        private void TreeViewCreator()
        {





        }



        private void RerollClick(object sender, RoutedEventArgs e)
        {
            Build b = null;
            if (Profile != null)
            {
                b = Profile.Characters[cmbCharacter.SelectedIndex].Inventory.Select(x => GearInfo.GetItem(x)).RandomBuild(UI.Shown, GearInfo.Items.Empties());
            }
            else
            {
                b = GearInfo.Items.ToList().RandomBuild(UI.Shown, GearInfo.Items.Empties());
            }

            Debug.WriteLine("Reroll: " + b.ToString());
            UI.EquipBuild(b);
            Profile?.Builds[0]?.Add(UI.Shown);
            File.WriteAllBytes(@"C:\Users\AuriCrystal\Documents\VisualProjects\RemnantMultipurposeManager\Resources\Data\test.txt", b.ToCode());
            Debug.WriteLine("BuildID");
            b.ToCode();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Profile != null)
            {
                BindProfile(Profile);
                return;
            }

            Properties.Settings.Default.CurrentProfile = null;
            profile = null;
            Properties.Settings.Default.Save();
            cmbCharacter.ItemsSource = null;
            cmbSaveSlot.ItemsSource = null;
            cmbCharacter.IsEnabled = false;
            cmbSaveSlot.IsEnabled = false;
            FileManager.IsEnabled = false;
            FileManager.Visibility = Visibility.Collapsed;
            KeepCheckpoint.IsEnabled = false;
            LoadedProfile.Header = "No Profile";
            LoadedProfile.Visibility = Visibility.Collapsed;
            UpdateCharacter.IsEnabled = false;
        }

        public static string RBRDirPath
        {
            get
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Remnant\\Saved\\RBR");
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Remnant\\Saved\\RBR";
            }
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogMessage(e.Exception.Message + "\n" + e.Exception.StackTrace);
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
                catch (Exception) { Debug.WriteLine("Check for update problem"); }
            }
            Debug.WriteLine("Update value: " + compare);
            return compare;
        }
        private string DownloadZip(string zipName)
        {
            try
            {
                string zipURL = "https://raw.githubusercontent.com/Auricrystal/RemnantMultipurposeManager/master/Resources/" + zipName + ".zip";
                using (WebClient client = new WebClient())
                {

                    if (!File.Exists(RBRDirPath + "\\" + zipName + ".zip"))
                    {
                        client.DownloadFile(zipURL, RBRDirPath + "\\" + zipName + ".zip");
                        LogMessage("Pulling " + zipName + " from Repo", LogType.Success);

                        return RBRDirPath + "\\" + zipName + ".zip";
                    }


                    using (ZipArchive Old = ZipFile.Open(RBRDirPath + "\\" + zipName + ".zip", ZipArchiveMode.Update), New = new ZipArchive(client.OpenRead(zipURL)))
                    {
                        foreach (ZipArchiveEntry entry in New.Entries)
                        {
                            //If new file does not exist or the new file is smaller in size than the old
                            if (entry.Name == "" ||
                                Old.GetEntry(entry.FullName) != null ||
                                entry.Length >= Old.GetEntry(entry.FullName).Length ||
                                entry.LastWriteTime.Ticks <= Old.GetEntry(entry.FullName).LastWriteTime.Ticks)
                                continue;

                            LogMessage("Updating " + zipName + " from Repo", LogType.Success);
                            Old.Dispose(); New.Dispose();
                            File.Delete(RBRDirPath + "\\" + zipName + ".zip"); client.DownloadFile(zipURL, RBRDirPath + "\\" + zipName + ".zip");
                            break;
                        }
                    }
                }
                return RBRDirPath + "\\" + zipName + ".zip";
            }
            catch (Exception) { LogMessage("Problem accessing internet.", LogType.Error); return null; }
        }
        private void DownloadNewProfile(bool rewards, string path)
        {
            string profilesave = "https://raw.githubusercontent.com/Auricrystal/RemnantMultipurposeManager/master/Resources/NewProfile";
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
            }
            else
            {
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
                StreamWriter writer = System.IO.File.AppendText(RBRDirPath + "/log.txt");
                writer.WriteLine(DateTime.Now.ToString() + ": " + msg);
                writer.Close();
            }
        }



        private void RemnantBuildRandomizer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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
            if (cmbCharacter.SelectedItem != null)
            {
                RemnantCharacter rc = (RemnantCharacter)cmbCharacter.SelectedItem;
                Profile.SavePair[rc.Slot] = cmbSaveSlot.SelectedIndex;
                Debug.WriteLine("Char:" + rc.Slot + " Set to:" + cmbSaveSlot.SelectedIndex);
            }
        }
        private void CmbCharacter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemnantCharacter rc = (RemnantCharacter)cmbCharacter.SelectedItem;


            cmbSaveSlot.SelectedIndex = (rc != null) ? Profile?.SavePair[rc.Slot] ?? -1 : -1;


        }
        private void safeRefresh(params DataGrid[] dg)
        {
            foreach (DataGrid d in dg)
            {
                d.CommitEdit();
                d.CommitEdit();
            }
            foreach (DataGrid d in dg) { d.Items.Refresh(); }
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
                LogMessage(edit.Message, LogType.Error);
                LogMessage("Problem Naming Folder", LogType.Error);
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

        private void DataFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(RBRDirPath);
        }

        private void GameRestart_Click(object sender, RoutedEventArgs e)
        {
            RestartGame();
        }

        private void CreateProfile_Click(object sender, RoutedEventArgs e)
        {

        }
        private void CreateProfileRewards_Click(object sender, RoutedEventArgs e)
        {

        }
        private void LoadProfile_Click(object sender, RoutedEventArgs e)
        {

        }
        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        private void KeepCheckpoint_Checked(object sender, RoutedEventArgs e)
        {
            LoadButtonSettings();
        }
        private void KeepCheckpoint_Unchecked(object sender, RoutedEventArgs e)
        {
            LoadButtonSettings();
        }

        private void LoadButtonSettings()
        {
            LoadSave.IsEnabled = AlterFile.IsChecked.Value && !KeepCheckpoint.IsChecked.Value;
            FeelingLucky.IsEnabled = AlterFile.IsChecked.Value && !KeepCheckpoint.IsChecked.Value;
        }
        private void AlterFile_Checked(object sender, RoutedEventArgs e)
        {
            LoadButtonSettings();
        }

        private void AlterFile_Unchecked(object sender, RoutedEventArgs e)
        {
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

        }

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void lblLastMessage_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainTab.SelectedIndex = 4;
        }

        private void CreateMasterRMM()
        {

            string zipName = "Vendors2";
            using (WebClient client = new WebClient())
            {
                using (ZipArchive Old = ZipFile.Open(RBRDirPath + "\\" + zipName + ".zip", ZipArchiveMode.Update))
                {
                    string savetype = "";
                    string diff = "";
                    string planet = "";
                    string bossname = "";
                    string modifier = "";

                    foreach (ZipArchiveEntry entry in Old.Entries)
                    {
                        if (!entry.FullName.Contains("save.sav"))
                            continue;
                        string[] temp = entry.FullName.Split('/');
                        // Debug.WriteLine(String.Join(",",temp));
                        if (savetype != temp[0])
                        {
                            savetype = temp[0];
                            Debug.WriteLine(savetype);
                        }

                        if (diff != temp[1])
                        {
                            diff = temp[1];
                            Debug.WriteLine("\t" + diff);
                        }
                        if (planet != temp[2])
                        {
                            planet = temp[2];
                            Debug.WriteLine("\t\t" + planet);
                        }

                        if (bossname != temp[3])
                        {
                            bossname = temp[3];
                            Debug.WriteLine("\t\t\t" + bossname);
                        }

                        if (modifier != temp[4])
                        {
                            modifier = temp[4];
                            Debug.WriteLine("\t\t\t\t" + modifier);
                        }

                        JWorldSave save = new JWorldSave(bossname,
                            (JWorldSave.WorldZone)Enum.Parse(typeof(JWorldSave.WorldZone), String.Join("", planet.Split(' '))),
                            (JWorldSave.SaveType)Enum.Parse(typeof(JWorldSave.SaveType), savetype),
                            new Dictionary<JWorldSave.DifficultyType, List<string>>() {{
                            //(JWorldSave.DifficultyType)Enum.Parse(typeof(JWorldSave.DifficultyType), diff)
                            JWorldSave.DifficultyType.Vendor
                            ,new List<string>(){modifier} }
                            }

                            );
                        MergeRMM(save);


                    }
                }
            }

        }

        private void MergeRMM(JWorldSave save)
        {
            var savelist = JWorldSave.LoadList(@"C:\Users\AuriCrystal\Documents\VisualProjects\RemnantMultipurposeManager\Resources\Data\CheckpointDirectory.rmm");



            if (!savelist.Select(x => (x.Name, x.World, x.Type)).Contains((save.Name, save.World, save.Type)))
            {
                Debug.WriteLine("New Data! Attempt to Insert!");
                savelist.Add(save);
            }
            else
            {
                Debug.WriteLine("Duplicate data! Attempt to Update!");
                JWorldSave match = savelist.Find(x => x.Name == save.Name);
                foreach (var diff in save.Checkpoints.Keys)
                {
                    if (!match.Checkpoints.ContainsKey(diff))
                        match.Checkpoints.Add(diff, new List<string>());
                    match.Checkpoints[diff].AddRange(save.Checkpoints[diff]);
                    match.Checkpoints[diff] = match.Checkpoints[diff].Distinct().ToList();
                }
            }
            JWorldSave.Save(@"C:\Users\AuriCrystal\Documents\VisualProjects\RemnantMultipurposeManager\Resources\Data\CheckpointDirectory.rmm", savelist.OrderBy(x => x.Type).ThenBy(x => x.World).ThenBy(x => x.Name).ToArray());
        }
        private void CreateRMM_Click(object sender, RoutedEventArgs e)
        {
            CreateMasterRMM();

        }
        private void ReadRMM_Click(object sender, RoutedEventArgs e)
        {
            var save = JWorldSave.LoadList(@"C:\Users\AuriCrystal\Documents\VisualProjects\RemnantMultipurposeManager\Resources\Data\CheckpointDirectory.rmm");

            Debug.WriteLine("RMM Count:" + save.Count);


            foreach (var item in save.OrderBy(x => x.World).ThenBy(x => x.Name))
            {

                Debug.WriteLine(item.World + ": " + item.Name);

            }



        }
        private void LoadSave_Click(object sender, RoutedEventArgs e)
        {
            File.SetLastWriteTime(@"C:\Users\AuriCrystal\AppData\Local\Remnant\Saved\SaveGames\profile.sav",DateTime.Now);
            Save.DownloadFile(GameSavePath + "\\save_" + cmbSaveSlot.SelectedIndex + ".sav", Diff, Mod);
        }

        private void FeelingLucky_Click(object sender, RoutedEventArgs e)
        {

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
                read.ShowDialog();

                if (read.FileName == "")
                    return;

                var profile = new RemnantProfile(read.FileName);



                foreach (RemnantCharacter rc in profile.Characters)
                {
                    Debug.WriteLine(rc.ToString());
                }
                using (var save = new System.Windows.Forms.SaveFileDialog())
                {
                    save.Title = "Save RProfile";
                    save.Filter = "(.RProfile)|*.RProfile";
                    save.DefaultExt = "RProfile";
                    save.ShowDialog();
                    if (save.FileName == "")
                        return;

                    profile.Name = Path.GetFileNameWithoutExtension(save.FileName);
                    profile.Save(save.FileName);

                    Properties.Settings.Default.CurrentProfile = save.FileName;
                    Properties.Settings.Default.Save();
                }
                BindProfile(profile);
            }
        }

        private void UpdateCharacter_Click(object sender, RoutedEventArgs e)
        {
            Profile.Save(Properties.Settings.Default.CurrentProfile);
        }

        private void LoadCharacter_Click(object sender, RoutedEventArgs e)
        {
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
                BindProfile(profile);
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
            if (SaveListDifficulty.SelectedIndex == -1)
                return;

            JWorldSave save = SaveList.SelectedItem as JWorldSave;

            JWorldSave.DifficultyType st = (JWorldSave.DifficultyType)Enum.Parse(typeof(JWorldSave.DifficultyType), SaveListDifficulty.SelectedItem.ToString());
            SaveListModifier.ItemsSource = save.Checkpoints[st];
            //SaveListDifficulty.SelectedIndex = 0;

        }

        private void cmbSaveType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SaveList.ItemsSource == null)
                return;
            ComboBox cmb = (ComboBox)sender;
            if (cmb.SelectedItem == null)
                return;
            JWorldSave.SaveType st = (JWorldSave.SaveType)Enum.Parse(typeof(JWorldSave.SaveType), ((ComboBoxItem)cmb.SelectedItem).Content.ToString());

            ICollectionView icv = CollectionViewSource.GetDefaultView(SaveList.ItemsSource);
            icv.Filter = o => { return ((o as JWorldSave).Type.Equals(st)); };


        }
    }
}

