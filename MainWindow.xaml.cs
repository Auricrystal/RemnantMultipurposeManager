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

namespace RemnantBuildRandomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string saveDirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Remnant\\Saved\\SaveGames";
        #region ScaleValue Depdency Property
        readonly public static Random rd = new Random();
        RemnantItem hg;
        RemnantItem lg;
        private RemnantSave activeSave;
        //public static List<RemnantCharacter> listCharacters;
        private Boolean suppressLog;

        private FileSystemWatcher saveWatcher;
        private DateTime lastUpdateCheck;
        //private Process gameProcess;
        Build b;

        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(MainWindow), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));

        public enum LogType
        {
            Normal,
            Success,
            Error
        }
        public MainWindow()
        {
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
            //listCharacters = new List<RemnantCharacter>();
            
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //VersionText.Text = "Version " + typeof(MainWindow).Assembly.GetName().Version;
            Debug.WriteLine("Current save date: " + File.GetLastWriteTime(saveDirPath + "\\profile.sav").ToString());



            saveWatcher.EnableRaisingEvents = true;
            
            activeSave = new RemnantSave(saveDirPath);
            cmbCharacter.ItemsSource = activeSave.Characters;
            GearInfo.characters = activeSave.Characters.Count;

            ReadXML();
            getBlacklist();
            checkForUpdate();

            Debug.WriteLine("reflength" + reflist.Count);
            Debug.WriteLine("refvalues" + reflist.Values.Count);
            Debug.WriteLine("refvaluesHG" + reflist.Values.Where(x => x.Data.Slot == SlotType.HG));
            Debug.WriteLine("weptest: " + reflist.Values.Where(x => x.Data.Slot == SlotType.HG || x.Data.Slot == SlotType.LG || x.Data.Slot == SlotType.M).OrderBy(x => x.Itemname).Count());

            
            WeaponList.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.HG || x.Data.Slot == SlotType.LG || x.Data.Slot == SlotType.M).OrderBy(x => x.Itemname));
            ArmorList.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.HE || x.Data.Slot == SlotType.CH || x.Data.Slot == SlotType.LE).OrderBy(x => x.Data.ID).ThenBy(x => (int)x.Data.Slot));
            AmuletList.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.AM));
            RingList.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.RI));
            ModList.ItemsSource = getList(reflist.Values.Where(x => x.Data.Slot == SlotType.MO).Take(28));
            BuildList.ItemsSource = presets;
            updateCurrentWorldAnalyzer();
        }

        private List<RemnantItem> getList(IEnumerable<RemnantItem> ril)
        {
            List<RemnantItem> li = new List<RemnantItem>();
            foreach (RemnantItem ri in ril)
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
                GearInfo.CheckForNewGameInfo();
            }).Start();
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
                catch (Exception ex)
                {
                }
            }).Start();
            lastUpdateCheck = DateTime.Now;
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
            if (!suppressLog)
            {
                txtLog.Text = txtLog.Text + Environment.NewLine + DateTime.Now.ToString() + ": " + msg;
                //lblLastMessage.Content = msg;
                //lblLastMessage.Foreground = new SolidColorBrush(color);
                if (color.Equals(Colors.White))
                {
                    //lblLastMessage.FontWeight = FontWeights.Normal;
                }
                else
                {
                    //lblLastMessage.FontWeight = FontWeights.Bold;
                }
            }
            StreamWriter writer = System.IO.File.AppendText("log.txt");
            writer.WriteLine(DateTime.Now.ToString() + ": " + msg);
            writer.Close();
        }


        private void OnSaveFileChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Debug.WriteLine("SAVE FILE CHANGED!!!!");
            updateCurrentWorldAnalyzer();
        }
        private void updateCurrentWorldAnalyzer()
        {
            Debug.WriteLine("UPDATING CHARACTERS!!!!!");
            activeSave.UpdateCharacters();
            Debug.WriteLine("CHAR COUNT==" + activeSave.Characters.Count);
            foreach (RemnantCharacter rc in activeSave.Characters)
            {
                Debug.WriteLine("MI COUNT=" + rc.GetMissingItems().Count);
                foreach (Item.Type t in Enum.GetValues(typeof(Item.Type)))
                {
                    Debug.WriteLine("TYPE " + t.ToString() + "//////////////////");
                    foreach (Item i in rc.GetMissingItems().Where(x => x.ItemType == t))
                    {
                        if (i.ItemAltName != null && i.ItemAltName.Length > 0)
                        {
                            Debug.WriteLine("MISSING: " + i.ItemAltName);
                        }
                        else
                        {
                            Debug.WriteLine("MISSING: " + i.ItemName);
                        }

                    }
                }

            }

        }



        private void Disable(string name)
        {
            if (!reflist[name].Disabled)
            {
                reflist[name].Disabled = true;
                Debug.WriteLine("Disabling: " + reflist[name].Itemname);

            }
            else
            {
                Debug.WriteLine(reflist[name].Itemname + " Is already Disabled!");
            }
        }

        private void Disable(params string[] names)
        {
            foreach (string n in names)
            {
                Disable(n);
            }
        }
        private void Enable(string name)
        {
            if (reflist[name].Disabled)
            {
                reflist[name].Disabled = false;
                Debug.WriteLine("Enabling: " + reflist[name].Itemname);

            }
            else
            {
                Debug.WriteLine(reflist[name].Itemname + " Is already Enabled!");
            }
        }
        private void Enable(params string[] names)
        {
            foreach (string n in names)
            {
                Enable(n);
            }
        }



        private void ReRollImg(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BuildList.Items.Count > 0 && rd.Next(20) == 0)
                {
                    Debug.WriteLine("PRESET CHOSEN!");
                    Build p = presets[rd.Next(presets.Count)];
                    Build b = new Build(p.BuildName, p.Code);

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

        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            MainWindow mainWindow = o as MainWindow;
            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double)value);
            else return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MainWindow mainWindow = o as MainWindow;
            if (mainWindow != null)
                mainWindow.OnScaleValueChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual double OnCoerceScaleValue(double value)
        {
            if (double.IsNaN(value))
                return 1.0f;

            value = Math.Max(0.1, value);
            return value;
        }

        protected virtual void OnScaleValueChanged(double oldValue, double newValue) { }

        public double ScaleValue
        {
            get => (double)GetValue(ScaleValueProperty);
            set => SetValue(ScaleValueProperty, value);
        }
        #endregion

        private void MainGrid_SizeChanged(object sender, EventArgs e) => CalculateScale();

        private void CalculateScale()
        {
            double yScale = ActualHeight / 500f;
            double xScale = ActualWidth / 840f;
            double value = Math.Min(xScale, yScale);

            ScaleValue = (double)OnCoerceScaleValue(RemnantBuildRandomizer, value);
        }
        public void WriteResourceToFile(string resourceName, string fileName)
        {
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            Debug.WriteLine("Checked! " + cb.Content.ToString());
            Disable(cb.Content.ToString());
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            Debug.WriteLine("UnChecked! " + cb.Content.ToString());
            Enable(cb.Content.ToString());
        }

        private void RemnantBuildRandomizer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //MessageBox.Show("Closing called");
            updateBlacklist();
        }

        private void CopyBuild_Click(object sender, RoutedEventArgs e)
        {
            CopyUIElementToClipboard(BuildScreen);
            MessageBox.Show("Copied to Clipboard!");
        }

        private void Add_Build_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BuildNameEnter.Text == "") { throw new Exception("No BuildName Assigned to Build!"); }
                Build b = new Build(BuildNameEnter.Text, BuildCodeEnter.Text);
                if (!presets.Contains(new Build(BuildNameEnter.Text, BuildCodeEnter.Text)))
                {
                    BuildNameEnter.Text = "";
                    BuildCodeEnter.Text = "";
                    presets.Add(b);
                    BuildList.Items.Refresh();
                }
            }
            catch (Exception ce)
            {
                MessageBox.Show(ce.Message);
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

        private void BuildBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            Debug.WriteLine("Checked! " + cb.Content.ToString());
            //((Build)cb.).Disabled = true;
        }

        private void BuildBox_UnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            Debug.WriteLine("Checked! " + cb.Content.ToString());

        }

        private void BuildList_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete && BuildList.SelectedIndex != -1)
            {
                presets.Remove(presets[BuildList.SelectedIndex]);
                BuildList.SelectedIndex = -1;
                BuildList.Items.Refresh();
            }
        }

        private void CmbCharacter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (cmbCharacter.SelectedIndex == -1 && activeSave.Characters.Count > 0) return;
            if (cmbCharacter.Items.Count > 0 && cmbCharacter.SelectedIndex > -1)
            {

                Debug.WriteLine("CHANGED TO " + activeSave.Characters[cmbCharacter.SelectedIndex].ToString());
                Debug.WriteLine(activeSave.Characters[cmbCharacter.SelectedIndex].GetMissingItems().Count);
                foreach (RemnantItem ri in reflist.Values)
                {
                    ri.Character = cmbCharacter.SelectedIndex;
                }
                disablemissing();
                WeaponList.Items.Refresh();
                ArmorList.Items.Refresh();
                AmuletList.Items.Refresh();
                RingList.Items.Refresh();
                ModList.Items.Refresh();
            }
        }

        private void disablemissing()
        {
            RemnantCharacter rc = activeSave.Characters[cmbCharacter.SelectedIndex];
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
            WeaponList.Items.Refresh();
            ArmorList.Items.Refresh();
            AmuletList.Items.Refresh();
            RingList.Items.Refresh();
            ModList.Items.Refresh();
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
                // case "Path":
                //case "Description":
                // case "Image":  e.Cancel=true; break;
                // case "Itemname":
                // case "Missing": break;
                default: break;
            }


        }
        private void ArmorList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string headername = e.Column.Header.ToString();
            //Cancel the column you don't want to generate
            switch (headername)
            {
                // case "Path":
                // case "Mod":
                // case "Image": e.Cancel = true; break;
                // case "Description":
                // case "Itemname":
                // case "Missing":  break;
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
            Debug.WriteLine("Editting: " + edit);

        }

        private void RemItem_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

        }

        private void RemItem_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }
    }
}

