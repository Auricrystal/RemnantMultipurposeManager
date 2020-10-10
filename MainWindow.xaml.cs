using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static RemnantBuildRandomizer.GearInfo;
using static RemnantBuildRandomizer.RemnantItem;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading;
using System.Net;
using System.Windows.Markup;
using Path = System.IO.Path;
using static RemnantBuildRandomizer.DataObj;
using System.Dynamic;

namespace RemnantBuildRandomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static MainWindow MW = null;
        private static string saveDirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Remnant\\Saved\\SaveGames";

        readonly public static Random rd = new Random();
        private static RemnantSave activeSave;
        private static int badluck = 0;

        private FileSystemWatcher saveWatcher;
        private DateTime lastUpdateCheck;

        public static RemnantSave ActiveSave
        {
            get
            {
                if (activeSave == null) { ActiveSave = new RemnantSave(saveDirPath); }
                return activeSave;

            }
            set => activeSave = value;
        }
        public RemnantCharacter ActiveCharacter { get => ActiveSave.Characters[cmbCharacter.SelectedIndex]; }
        public static string BackupDirPath
        {
            get
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Remnant\\Saved\\RBR");
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Remnant\\Saved\\RBR";
            }
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

            saveWatcher = new FileSystemWatcher();
            saveWatcher.Path = saveDirPath;

            // Watch for changes in LastWrite times.
            saveWatcher.NotifyFilter = NotifyFilters.LastWrite;

            // Only watch sav files.
            saveWatcher.Filter = "profile.sav";

            // Add event handlers.
            saveWatcher.Changed += OnSaveFileChanged;
            saveWatcher.Created += OnSaveFileChanged;
            saveWatcher.Deleted += OnSaveFileChanged;
            //watcher.Renamed += OnRenamed;

            ((MenuItem)BuildList.ContextMenu.Items[0]).Click += GrabBuild_Click;
            ((MenuItem)BuildList.ContextMenu.Items[1]).Click += CopyBuildObj_Click;
            ((MenuItem)BuildList.ContextMenu.Items[2]).Click += PasteBuildObj_Click;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Current save date: " + File.GetLastWriteTime(saveDirPath + "\\profile.sav").ToString());

            saveWatcher.EnableRaisingEvents = true;

            ActiveSave = new RemnantSave(saveDirPath);

            cmbCharacter.ItemsSource = ActiveSave.Characters;
            cmbCharacter.SelectedIndex = 0;

            ReadXML();
            GetData();

            checkForUpdate();

            SetupData();

            UpdateCharacterData();
            disablemissing();
        }
        private void SetupData()
        {
            List<RemnantItem> empties = StrToRI.Values.Where(x => x.Itemname.Contains("_")).ToList();
            List<RemnantItem> molist = GetEquipment[SlotType.MO].Take(28).ToList();
            List<RemnantItem> hglist = GetEquipment[SlotType.HG].ToList();
            List<RemnantItem> lglist = GetEquipment[SlotType.LG].ToList();
            List<RemnantItem> mlist = GetEquipment[SlotType.M].ToList();
            List<RemnantItem> helist = GetEquipment[SlotType.HE].ToList();
            List<RemnantItem> chlist = GetEquipment[SlotType.CH].ToList();
            List<RemnantItem> lelist = GetEquipment[SlotType.LE].ToList();
            List<RemnantItem> amlist = GetEquipment[SlotType.AM].ToList();
            List<RemnantItem> rilist = GetEquipment[SlotType.RI].ToList();


            WeaponList.ItemsSource = Combine(hglist, lglist, mlist).Except(empties).ToList();
            ArmorList.ItemsSource = Combine(helist, chlist, lelist).Except(empties).ToList();
            AmuletList.ItemsSource = amlist.Except(empties);
            RingList.ItemsSource = rilist.Except(empties);
            ModList.ItemsSource = molist.Except(empties);
            BuildList.ItemsSource = Presets[ActiveCharacter.charNum].ToList();
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
        private static List<T> Combine<T>(params IEnumerable<T>[] rils)
        {
            List<T> li = new List<T>();
            foreach (IEnumerable<T> ril in rils) { li.AddRange(ril); }
            return li;
        }

        private void checkForUpdate()
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
                this.Dispatcher.Invoke(() =>
                {
                    txtLog.Text = txtLog.Text + Environment.NewLine + DateTime.Now.ToString() + ": " + msg;
                    StreamWriter writer = System.IO.File.AppendText(BackupDirPath + "/log.txt");
                    writer.WriteLine(DateTime.Now.ToString() + ": " + msg);
                    writer.Close();
                });
            }
            else
            {
                txtLog.Text = txtLog.Text + Environment.NewLine + DateTime.Now.ToString() + ": " + msg;
                StreamWriter writer = System.IO.File.AppendText(BackupDirPath + "/log.txt");
                writer.WriteLine(DateTime.Now.ToString() + ": " + msg);
                writer.Close();
            }

        }


        private void OnSaveFileChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            logMessage("Save File Changed...");
            UpdateCharacterData();
        }
        private void UpdateCharacterData()
        {
            logMessage("Updating Character Info:");
            ActiveSave.UpdateCharacters();
            if (!Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() =>
                {
                    cmbCharacter.ItemsSource = ActiveSave.Characters;
                    cmbCharacter.SelectedIndex = 0;
                    cmbCharacter.Items.Refresh();
                });
            }
            else
            {
                cmbCharacter.ItemsSource = ActiveSave.Characters;
                cmbCharacter.SelectedIndex = 0;
                cmbCharacter.Items.Refresh();
            }
            logMessage("Characters: " + ActiveSave.Characters.Count);
            foreach (RemnantCharacter rc in ActiveSave.Characters)
            {
                logMessage(rc + " Has " + rc.GetMissingItems().Count + " Missing Items");
            }
        }


        private void Reroll_Click(object sender, RoutedEventArgs e)
        {
            List<Build> builds = Presets[ActiveCharacter.charNum];

            List<Build> list = builds.Where(x => x.Disabled == false).ToList();
            if (list.Count > 0 && rd.Next(100) <= badluck)
            {
                badluck = 0;
                Build b = list[rd.Next(list.Count)];
                DisplayBuild(b);

                BuildNum.Text = b.BuildName;
                BuildNum.ToolTip = b.toCode();
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
                Conditions();
                BuildNum.Text = rand.toCode();
                BuildNum.ToolTip = null;
            }


        }
        private void DisplayBuild(Build b)
        {
            setImage(HandGunImg, b.HandGun);
            setModImage(HandModImg, HandCoverModImg, b.HandMod);
            setImage(LongGunImg, b.LongGun);
            setModImage(LongModImg, LongCoverModImg, b.LongMod);
            setImage(MeleeImg, b.Melee);
            setImage(HeadImg, b.Head);
            setImage(ChestImg, b.Chest);
            setImage(LegImg, b.Legs);
            setImage(AmImg, b.Amulet);
            setImage(Ri1Img, b.Ring1);
            setImage(Ri2Img, b.Ring2);
        }
        private void setImage(Image i, RemnantItem ri)
        {
            i.Source = ri.Data.GetImage();
            ToolTipService.SetShowDuration(i, 60000);
            i.ToolTip = ri.Description;
        }
        private void setModImage(Image i, Image j, RemnantItem ri)
        {
            i.Source = ri.Data.GetImage();
            ToolTipService.SetShowDuration(j, 60000);
            j.ToolTip = ri.Description;
        }


        private void Conditions()
        {
            if (getItem(AmImg).Itemname == "White Rose")
            {
                string text = "\n\nWHITE ROSE EFFECT\n";
                Debug.WriteLine(text);
                if (rd.Next(2) == 1) { setImage(HandGunImg, StrToRI["_No Hand Gun"]); setImage(HandModImg, StrToRI["_No Mod"]); HandCoverModImg.ToolTip = null; text += "removed HG\n"; }
                if (rd.Next(2) == 1) { setImage(LongGunImg, StrToRI["_No Long Gun"]); setImage(LongModImg, StrToRI["_No Mod"]); LongCoverModImg.ToolTip = null; text += "removed LG"; }
                AmImg.ToolTip += text;
            }
            else if (getItem(AmImg).Itemname == "Daredevil's Charm")
            {
                string text = "\n\nDDC EFFECT\n";
                Debug.WriteLine(text);
                if (rd.Next(2) == 1) { setImage(HeadImg, StrToRI["_No Head"]); text += "removed Head\n"; }
                if (rd.Next(2) == 1) { setImage(ChestImg, StrToRI["_No Chest"]); text += "removed Chest\n"; }
                if (rd.Next(2) == 1) { setImage(LegImg, StrToRI["_No Legs"]); text += "removed Legs"; }
                AmImg.ToolTip += text;
            }
            if (getItem(Ri1Img).Itemname.ToLower() == "Ring Of The Unclean".ToLower() || getItem(Ri2Img).Itemname.ToLower() == "Ring Of The Unclean".ToLower() ||
                getItem(Ri1Img).Itemname.ToLower() == "Five Fingered Ring".ToLower() || getItem(Ri2Img).Itemname.ToLower() == "Five Fingered Ring".ToLower())
            {
                Debug.WriteLine("ROTU or FFR Effect");
                if (rd.Next(2) == 1) { setImage(MeleeImg, StrToRI["_Fists"]); }

            }

        }
        private RemnantItem getItem(Image item)
        {
            Debug.WriteLine("getItem:" + Path.GetFileName(item.Source.ToString()));
            return StrToRI[Path.GetFileName(item.Source.ToString()).Replace(".png", "")];
        }




        public void SaveData()
        {
            string path = BackupDirPath + @"/Data.txt";
            File.Delete(path);
            GetData();
        }


        private void RemnantBuildRandomizer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //MessageBox.Show("Closing called");
            SaveData();
        }



        private void Add_Build_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BuildNameEnter.Text == "") { throw new Exception("No BuildName Assigned to Build!"); }
                Build b = new Build(BuildNameEnter.Text, BuildCodeEnter.Text);
                if (!Presets[ActiveCharacter.charNum].Contains(new Build(BuildNameEnter.Text, BuildCodeEnter.Text)))
                {
                    BuildNameEnter.Text = "";
                    BuildCodeEnter.Text = "";
                    Presets[ActiveCharacter.charNum].Add(b);
                    BuildList.Items.Refresh();
                }
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


        private void CmbCharacter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (cmbCharacter.SelectedIndex == -1 && ActiveSave.Characters.Count > 0) return;
            if (cmbCharacter.Items.Count > 0 && cmbCharacter.SelectedIndex > -1)
            {
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

            //BuildList.Items.Refresh();

        }

        private void ResetBlack_Click(object sender, RoutedEventArgs e)
        {
            foreach (RemnantItem ri in StrToRI.Values)
            {
                ri.Disabled = false;
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
            checkForUpdate();
        }

        private void WeaponList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();
            //Cancel the column you don't want to generate
            switch (headername)
            {
                case "Data":
                case "Description":
                case "Character": e.Cancel = true; break;

                case "Itemname":
                case "Slot":
                case "Mod":
                case "Missing": e.Column.IsReadOnly = true; break;
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

                case "Itemname":
                case "Slot":
                case "Description":
                case "Missing": e.Column.IsReadOnly = true; break;
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

                case "Itemname":
                case "Description":
                case "Missing": e.Column.IsReadOnly = true; break;
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


            BuildCodeEnter.Text = new Build("", hg, hgm, lg, lgm, m, he, ch, le, am, r1, r2).toCode();

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
            Process.Start(BackupDirPath);
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
                Clipboard.SetText(selectedBuild.ToData());
                BuildList.SelectedItem = null;
            }
        }
        private void PasteBuildObj_Click(object sender, System.EventArgs e)
        {
            try
            {
                Build b = Build.FromData(Clipboard.GetText());
                logMessage("Pasting Build (" + b + ")");
                if (!Presets[ActiveCharacter.charNum].Contains(b))
                {
                    Presets[ActiveCharacter.charNum].Add(b);
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

        private void DisableEmpties_Click(object sender, RoutedEventArgs e)
        {
            List<RemnantItem> empties = StrToRI.Values.Where(x => x.Itemname.Contains("_")).ToList();
            foreach (RemnantItem ri in empties)
            {
                ri.Disabled = true;
            }
            EmptySlots.Items.Refresh();
        }
    }
}

