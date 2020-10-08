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
        RemnantItem hg;
        RemnantItem lg;
        private static RemnantSave activeSave;
        private static int badluck = 0;
        //public static List<RemnantCharacter> listCharacters;
        // private Boolean suppressLog=false;

        private FileSystemWatcher saveWatcher;
        private DateTime lastUpdateCheck;
        //private Process gameProcess;
        Build b;

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

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Current save date: " + File.GetLastWriteTime(saveDirPath + "\\profile.sav").ToString());

            saveWatcher.EnableRaisingEvents = true;

            ActiveSave = new RemnantSave(saveDirPath);

            cmbCharacter.ItemsSource = ActiveSave.Characters;
            cmbCharacter.SelectedIndex = 0;

            

            ReadXML();
            getBlacklist();

            checkForUpdate();

            SetupData();

            UpdateCharacterData();
            disablemissing();
        }
        private void SetupData()
        {


            WeaponList.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.HG || x.Data.Slot == SlotType.LG || x.Data.Slot == SlotType.M).OrderBy(x => x.Itemname));
            ArmorList.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.HE || x.Data.Slot == SlotType.CH || x.Data.Slot == SlotType.LE).OrderBy(x => x.Data.ID).ThenBy(x => (int)x.Data.Slot));
            AmuletList.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.AM));
            RingList.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.RI));
            ModList.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.MO).Take(28));
            BuildList.ItemsSource = Presets[ActiveCharacter.charNum];

            cmbHG.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.HG));
            cmbHGM.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.MO).Take(28));
            cmbLG.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.LG));
            cmbLGM.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.MO).Take(28));
            cmbM.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.M));
            cmbHE.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.HE));
            cmbCH.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.CH));
            cmbLE.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.LE));
            cmbAM.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.AM));
            cmbR1.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.RI));
            cmbR2.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.RI));
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


        private static List<T> getList<T>(IEnumerable<T> ril)
        {
            List<T> li = new List<T>();
            foreach (T ri in ril)
            {
                li.Add(ri);
            }
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

            txtLog.Text = txtLog.Text + Environment.NewLine + DateTime.Now.ToString() + ": " + msg;
            StreamWriter writer = System.IO.File.AppendText(BackupDirPath + "/log.txt");
            writer.WriteLine(DateTime.Now.ToString() + ": " + msg);
            writer.Close();
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
            logMessage("Caracters: " + ActiveSave.Characters.Count);
            foreach (RemnantCharacter rc in ActiveSave.Characters)
            {
                logMessage(rc + " Has " + rc.GetMissingItems().Count + " Missing Items");
            }
        }


        private void ReRollImg(object sender, RoutedEventArgs e)
        {
            List<Build> builds = Presets[ActiveCharacter.charNum];
            try
            {
                List<Build> list = getList(builds.Where(x => x.Disabled == false));

                if (list.Count > 0 && rd.Next(100) <= badluck)
                {
                    badluck = 0;
                    Debug.WriteLine("PRESET CHOSEN!");
                    Build b = list[rd.Next(list.Count)];
                    //Build b = new Build(p.BuildName, p.Code);

                    setSlot(HandGunImg, b.hg);
                    setModSlot(HandModImg, HandCoverModImg, b.hgm);

                    setSlot(LongGunImg, b.lg);
                    setModSlot(LongModImg, LongCoverModImg, b.lgm);

                    setSlot(MeleeImg, b.m);

                    setSlot(HeadImg, b.he);
                    setSlot(ChestImg, b.ch);
                    setSlot(LegImg, b.le);

                    setSlot(AmImg, b.am);

                    setSlot(Ri1Img, b.r1);
                    setSlot(Ri2Img, b.r2);

                    //Conditions();
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
                    int[] ids = new int[11];
                    //RedCrystal.ToolTip = null;

                    ids[0] = RerollSlot(HandGunImg, SlotType.HG);
                    RemnantItem ri = RerollMod(HandCoverModImg, HandModImg, hg);
                    ids[1] = ri.Data.ID;

                    ids[2] = RerollSlot(LongGunImg, SlotType.LG);
                    RemnantItem ri2 = RerollMod(LongCoverModImg, LongModImg, lg);
                    while (ri.Itemname == ri2.Itemname && ri.Itemname != "_No Mod") { ri2 = RerollMod(LongCoverModImg, LongModImg, lg); }
                    ids[3] = ri2.Data.ID;

                    ids[4] = RerollSlot(MeleeImg, SlotType.M);

                    ids[5] = RerollSlot(HeadImg, SlotType.HE);
                    ids[6] = RerollSlot(ChestImg, SlotType.CH);
                    ids[7] = RerollSlot(LegImg, SlotType.LE);

                    ids[8] = RerollSlot(AmImg, SlotType.AM);
                    int num = RerollSlot(Ri1Img, SlotType.RI);
                    ids[9] = num;
                    int num2 = RerollSlot(Ri2Img, SlotType.RI);
                    while (num2 == num) { num2 = RerollSlot(Ri2Img, SlotType.RI); }
                    ids[10] = num2;
                    Conditions();


                    b = new Build("", ids);
                    BuildNum.Text = b.toCode();
                    BuildNum.ToolTip = null;
                }
            }
            catch (Exception ce) { MessageBox.Show(ce.Message); }

        }
        private void Conditions()
        {
            if (getItem(AmImg).Itemname == "White Rose")
            {
                string text = "\n\nWHITE ROSE EFFECT\n";
                Debug.WriteLine(text);
                if (rd.Next(2) == 1) { setSlot(HandGunImg, reflist["_No Hand Gun"]); setSlot(HandModImg, reflist["_No Mod"]); HandCoverModImg.ToolTip = null; text += "removed HG\n"; }
                if (rd.Next(2) == 1) { setSlot(LongGunImg, reflist["_No Long Gun"]); setSlot(LongModImg, reflist["_No Mod"]); LongCoverModImg.ToolTip = null; text += "removed LG"; }
                AmImg.ToolTip += text;
            }
            else if (getItem(AmImg).Itemname == "Daredevil's Charm")
            {
                string text = "\n\nDDC EFFECT\n";
                Debug.WriteLine(text);
                if (rd.Next(2) == 1) { setSlot(HeadImg, reflist["_No Head"]); text += "removed Head\n"; }
                if (rd.Next(2) == 1) { setSlot(ChestImg, reflist["_No Chest"]); text += "removed Chest\n"; }
                if (rd.Next(2) == 1) { setSlot(LegImg, reflist["_No Legs"]); text += "removed Legs"; }
                AmImg.ToolTip += text;
            }
            if (getItem(Ri1Img).Itemname.ToLower() == "Ring Of The Unclean".ToLower() || getItem(Ri2Img).Itemname.ToLower() == "Ring Of The Unclean".ToLower() ||
                getItem(Ri1Img).Itemname.ToLower() == "Five Fingered Ring".ToLower() || getItem(Ri2Img).Itemname.ToLower() == "Five Fingered Ring".ToLower())
            {
                Debug.WriteLine("ROTU or FFR Effect");
                if (rd.Next(2) == 1) { setSlot(MeleeImg, reflist["_Fists"]); }

            }

        }
        private RemnantItem getItem(Image item)
        {
            Debug.WriteLine("getItem:" + Path.GetFileName(item.Source.ToString()));
            return reflist[Path.GetFileName(item.Source.ToString()).Replace(".png", "")];
        }
        public int RerollSlot(Image i, SlotType st)
        {
            List<RemnantItem> riList = GearList[st].Where(x => x.Disabled == false).ToList();
            if (riList.Count == 0 && st != SlotType.RI)
            {
                throw new Exception("Too many items disabled in " + st + " Need atleast 1!");
            }
            else if (riList.Count < 2 && st == SlotType.RI)
            {
                throw new Exception("Too many items disabled in " + st + " Need atleast 2!");
            }

            RemnantItem ri = riList[rd.Next(0, riList.Count)];
            if (st == SlotType.HG) { hg = ri; }
            else
            if (st == SlotType.LG) { lg = ri; }
            setSlot(i, ri);
            return ri.Data.ID;

        }
        private void setSlot(Image i, RemnantItem ri)
        {
            i.Source = ri.Data.GetImage();
            ToolTipService.SetShowDuration(i, 60000);
            i.ToolTip = ri.Itemname + "\n" + ri.Description;
        }
        private void setModSlot(Image i, Image s, RemnantItem ri)
        {
            i.Source = ri.Data.GetImage();
            ToolTipService.SetShowDuration(i, 60000);
            s.ToolTip = ri.Itemname + "\n" + ri.Description;
        }
        public RemnantItem RerollMod(Image c, Image i, RemnantItem ri)
        {
            Debug.WriteLine(ri.Itemname + "==" + ri.Mod);
            ToolTipService.SetShowDuration(i, 60000);
            ToolTipService.SetShowDuration(c, 60000);
            if (reflist.ContainsKey(ri.Mod))
            {
                i.Source = reflist[ri.Mod].Data.GetImage();
                c.ToolTip = reflist[ri.Mod].Itemname + "\n" + reflist[ri.Mod].Description;
                Debug.WriteLine("Boss Mod: " + reflist[ri.Mod].Itemname);
                return reflist[ri.Mod];
            }
            else
            {
                List<RemnantItem> riList = GearList[SlotType.MO].Take(28).Where(x => x.Disabled == false).ToList();
                if (riList.Count < 2)
                {
                    throw new Exception("Too many items disabled in " + SlotType.MO + " Need atleast 2!");
                }
                RemnantItem mo = riList[rd.Next(0, riList.Count)];
                i.Source = mo.Data.GetImage();
                c.ToolTip = mo.Itemname + "\n" + mo.Description;
                Debug.WriteLine("Regular Mod: " + reflist[mo.Itemname].Itemname);
                return mo;
            }
        }

        public void SaveData()
        {

            string path = BackupDirPath + @"/Data.txt";
            File.Delete(path);
            getBlacklist();
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

            Debug.WriteLine(bmpCopied.Metadata.GetValue(NameProperty));




            Clipboard.SetImage(bmpCopied);
        }


        private void CmbCharacter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (cmbCharacter.SelectedIndex == -1 && ActiveSave.Characters.Count > 0) return;
            if (cmbCharacter.Items.Count > 0 && cmbCharacter.SelectedIndex > -1)
            {
                safeCommit(WeaponList, ArmorList, AmuletList, RingList, ModList, BuildList);
                BuildList.ItemsSource = Presets[ActiveCharacter.charNum];
                Debug.WriteLine("CHANGED TO " + ActiveSave.Characters[cmbCharacter.SelectedIndex].ToString());
                Debug.WriteLine(ActiveSave.Characters[cmbCharacter.SelectedIndex].GetMissingItems().Count);
                foreach (RemnantItem ri in reflist.Values)
                {
                    ri.Character = cmbCharacter.SelectedIndex;
                }
                disablemissing();
                safeRefresh(WeaponList, ArmorList, AmuletList, RingList, ModList, BuildList);
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
            foreach (RemnantItem ri in reflist.Values)
            {
                ri.Missing = false;
            }
            foreach (Item ri in rc.GetMissingItems())
            {
                reflist[ri.ItemAltName].Missing = true;
            }

            //BuildList.Items.Refresh();

        }

        private void ResetBlack_Click(object sender, RoutedEventArgs e)
        {
            foreach (RemnantItem ri in reflist.Values)
            {
                ri.Disabled = false;
            }
            safeRefresh(WeaponList);
            safeRefresh(ArmorList);
            safeRefresh(AmuletList);
            safeRefresh(RingList);
            safeRefresh(ModList);
            //BuildList.Items.Refresh();
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

        private void BuildList_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            Build edit = ((Build)e.Row.Item);
            Debug.WriteLine("Editting: " + edit);
        }

        private void BuildList_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

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
                cmbHGM.ItemsSource = new List<RemnantItem>() { reflist[hg.Mod] };
            }
            else
            {
                cmbHGM.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.MO).Take(28));
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
                cmbLGM.ItemsSource = new List<RemnantItem>() { reflist[lg.Mod] };
            }
            else
            {
                cmbLGM.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.MO).Take(28));
            }
            cmbLGM.SelectedIndex = 0;
            cmbLGM.Items.Refresh();

        }

        private void cmbR1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbR2.ItemsSource = getList(reflist.Values.Where(x => x != (RemnantItem)cmbR1.SelectedItem && x.Data.Slot == SlotType.RI || x.Itemname == "_No Ring"));
        }

        private void cmbR2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbR1.ItemsSource = getList(reflist.Values.Where(x => x != (RemnantItem)cmbR2.SelectedItem && x.Data.Slot == SlotType.RI || x.Itemname == "_No Ring"));
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
                cmbLGM.ItemsSource = getList(reflist.Values.Where(x => x != (RemnantItem)cmbHGM.SelectedItem && x.Data.Slot == SlotType.MO || x.Itemname == "_No Mod")).Take(27);
            }
            else {  }
        }

        private void cmbLGM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbHGM.Items.Count > 1)
            {
                if (cmbHGM.SelectedIndex > 27) {cmbHGM.SelectedIndex = 0;}
                
                cmbHGM.ItemsSource = getList(reflist.Values.Where(x => x != (RemnantItem)cmbLGM.SelectedItem && x.Data.Slot == SlotType.MO || x.Itemname == "_No Mod")).Take(27);
            }
            else {  }
        }

        private void DataFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(BackupDirPath);
        }
    }
}

