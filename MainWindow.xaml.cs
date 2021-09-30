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
using static RemnantMultipurposeManager.GearInfo;


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
        private InventoryUI UI;
        public MainWindow() : base()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            MW = this;
            InitializeComponent();
            txtLog.IsReadOnly = true;
            DownloadZip("IMG");
            File.Delete(RBRDirPath + "\\log.txt");

            BuildUI.Child = new InventoryUI();

            
            
           
            //g.Children.Add(new Viewbox() {Child = (UI = new InventoryUI()), Stretch = Stretch.Uniform, StretchDirection = StretchDirection.Both, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(10, 0, 0, 20) });
            
        }

        private void RerollClick(object sender, RoutedEventArgs e)
        {
            UI.EquipBuild(GearInfo.Items.RandomBuild(UI.Shown,GearInfo.Items.Empties()));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RemnantProfile p = new RemnantProfile(GameSavePath + "\\profile.sav");
            File.WriteAllText(@"C:\Users\AuriCrystal\Documents\VisualProjects\RemnantMultipurposeManager\Resources\RemnantProfile.json", JsonConvert.SerializeObject(p, Formatting.None));
            cmbCharacter.IsEnabled = false;
            cmbSaveSlot.IsEnabled = false;
            SaveManipulator.IsEnabled = false;
            KeepCheckpoint.IsEnabled = false;
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
            using (WebClient client = new WebClient())
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
                            LogMessage("No new version found.");
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
                                        LogMessage("New " + zipName + " Package Update!\nDownloading!");
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
        public void LogMessage(string msg)
        {
            LogMessage(msg, Colors.White);
        }

        public void LogMessage(string msg, LogType lt)
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
            LogMessage(msg, color);
        }

        public void LogMessage(string msg, Color color)
        {

            if (!Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => { LogMessage(msg, color); });
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

        }
        private void CmbCharacter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
            if (checkForUpdate() == -1)
            {
                var confirmResult = MessageBox.Show("There is a new version available. Would you like to open the download page?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (confirmResult == MessageBoxResult.Yes)
                {
                    Process.Start("https://github.com/Auricrystal/RemnantMultipurposeManager/releases/latest");
                    System.Environment.Exit(1);
                }
            }
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

        private void CreateRBB_Click(object sender, RoutedEventArgs e)
        {
            var test = Items[300].IMG;
        }

        private void LoadSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FeelingLucky_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

