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

namespace RemnantBuildRandomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string saveDirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Remnant\\Saved\\SaveGames";
        #region ScaleValue Depdency Property
        Random rd = new Random();
        RemnantItem hg;
        RemnantItem lg;
        private RemnantSave activeSave;
        private List<RemnantCharacter> listCharacters;

        private FileSystemWatcher saveWatcher;
        //private Process gameProcess;
        Build b;

        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(MainWindow), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));

        public MainWindow()
        {
            InitializeComponent();
            ReadXML();
            getBlacklist();
            WeaponList.ItemsSource = reflist.Values.Where(x => x.Slot == SlotType.HG || x.Slot == SlotType.LG || x.Slot == SlotType.M).OrderBy(x => x.Itemname);
            ArmorList.ItemsSource = reflist.Values.Where(x => x.Slot == SlotType.HE || x.Slot == SlotType.CH || x.Slot == SlotType.LE).OrderBy(x => x.ID).ThenBy(x => (int)x.Slot);
            AmuletList.ItemsSource = reflist.Values.Where(x => x.Slot == SlotType.AM);
            RingList.ItemsSource = reflist.Values.Where(x => x.Slot == SlotType.RI);
            ModList.ItemsSource = reflist.Values.Where(x => x.Slot == SlotType.MO).Take(28);
            BuildList.ItemsSource = presets;

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
            listCharacters = new List<RemnantCharacter>();
            cmbCharacter.ItemsSource = listCharacters;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Current save date: " + File.GetLastWriteTime(saveDirPath + "\\profile.sav").ToString());
            saveWatcher.EnableRaisingEvents = true;
            //GearInfo.RefreshGameInfo();
            Debug.WriteLine("SAVE PATH: " + saveDirPath);
            activeSave = new RemnantSave(saveDirPath);
            updateCurrentWorldAnalyzer();
            LoadData(activeSave.Characters);
        }

        public void LoadData(List<RemnantCharacter> chars)
        {
            int selectedChar = cmbCharacter.SelectedIndex;
            listCharacters = chars;
            /*Console.WriteLine("Loading characters in analyzer: " + listCharacters.Count);
            foreach (CharacterData cd in listCharacters)
            {
                Console.WriteLine("\t" + cd);
            }*/
            cmbCharacter.ItemsSource = listCharacters;
            if (selectedChar == -1 && listCharacters.Count > 0) selectedChar = 0;
            if (selectedChar > -1 && listCharacters.Count > selectedChar) cmbCharacter.SelectedIndex = selectedChar;
            cmbCharacter.IsEnabled = (listCharacters.Count > 1);
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
                    Debug.WriteLine("TYPE " + t.ToString()+"//////////////////");
                    foreach (Item i in rc.GetMissingItems().Where(x => x.ItemType == t)) {
                        if (i.ItemAltName!=null&&i.ItemAltName.Length>0)
                        {
                            Debug.WriteLine("MISSING: " + i.ItemAltName);
                        }
                        else {
                            Debug.WriteLine("MISSING: " + i.ItemName);
                        }
                        
                    }
                }

            }

        }


        private void ResetDisabled()
        {

            //resetList(AmuletList);
            // resetList(RingList);
        }


        private void resetList(ListBox lb)
        {
            Debug.WriteLine("Items Disabled in " + lb.Name + ": " + lb.Items.OfType<RemnantItem>().Where(x => x.Disabled == true).ToList().Count);
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
                    Build b = presets[rd.Next(presets.Count)];

                    setSlot(HandGunSlot, b.hg);
                    setModSlot(Mod1Slot, Mod1Cover, b.hgm);

                    setSlot(LongGunSlot, b.lg);
                    setModSlot(Mod2Slot, Mod2Cover, b.lgm);

                    setSlot(MeleeSlot, b.m);

                    setSlot(HeadSlot, b.he);
                    setSlot(ChestSlot, b.ch);
                    setSlot(LegSlot, b.le);

                    setSlot(AmuletSlot, b.am);

                    setSlot(Ring1Slot, b.r1);
                    setSlot(Ring2Slot, b.r2);

                    //Conditions();
                    BuildNum.Text = b.Name;
                    BuildNum.ToolTip = b.toCode();

                }
                else
                {
                    int[] ids = new int[11];
                    RedCrystal.ToolTip = null;

                    ids[0] = RerollSlot(HandGunSlot, SlotType.HG);
                    RemnantItem ri = RerollMod(Mod1Cover, Mod1Slot, hg);
                    ids[1] = ri.ID;

                    ids[2] = RerollSlot(LongGunSlot, SlotType.LG);
                    RemnantItem ri2 = RerollMod(Mod2Cover, Mod2Slot, lg);
                    while (ri.Itemname == ri2.Itemname && ri.Itemname != "_No Mod") { ri2 = RerollMod(Mod2Cover, Mod2Slot, lg); }
                    ids[3] = ri2.ID;

                    ids[4] = RerollSlot(MeleeSlot, SlotType.M);

                    ids[5] = RerollSlot(HeadSlot, SlotType.HE);
                    ids[6] = RerollSlot(ChestSlot, SlotType.CH);
                    ids[7] = RerollSlot(LegSlot, SlotType.LE);

                    ids[8] = RerollSlot(AmuletSlot, SlotType.AM);
                    int num = RerollSlot(Ring1Slot, SlotType.RI);
                    ids[9] = num;
                    int num2 = RerollSlot(Ring2Slot, SlotType.RI);
                    while (num2 == num) { num2 = RerollSlot(Ring2Slot, SlotType.RI); }
                    ids[10] = num2;
                    Conditions();


                    b = new Build("", ids[0], ids[1], ids[2], ids[3], ids[4], ids[5], ids[6], ids[7], ids[8], ids[9], ids[10]);
                    BuildNum.Text = b.toCode();
                    BuildNum.ToolTip = null;
                }
            }
            catch (Exception ce) { MessageBox.Show(ce.Message); }

        }
        private void Conditions()
        {
            if (getItem(AmuletSlot).Itemname == "White Rose")
            {
                string text = "\n\nWHITE ROSE EFFECT\n";
                Debug.WriteLine(text);
                if (rd.Next(2) == 1) { setSlot(HandGunSlot, reflist["_No Hand Gun"]); setSlot(Mod1Slot, reflist["_No Mod"]); Mod1Cover.ToolTip = null; text += "removed HG\n"; }
                if (rd.Next(2) == 1) { setSlot(LongGunSlot, reflist["_No Long Gun"]); setSlot(Mod2Slot, reflist["_No Mod"]); Mod2Cover.ToolTip = null; text += "removed LG"; }
                AmuletSlot.ToolTip += text;
            }
            else if (getItem(AmuletSlot).Itemname == "Daredevil's Charm")
            {
                string text = "\n\nDDC EFFECT\n";
                Debug.WriteLine(text);
                if (rd.Next(2) == 1) { setSlot(HeadSlot, reflist["_No Head"]); text += "removed Head\n"; }
                if (rd.Next(2) == 1) { setSlot(ChestSlot, reflist["_No Chest"]); text += "removed Chest\n"; }
                if (rd.Next(2) == 1) { setSlot(LegSlot, reflist["_No Legs"]); text += "removed Legs"; }
                AmuletSlot.ToolTip += text;
            }
            if (getItem(Ring1Slot).Itemname.ToLower() == "Ring Of The Unclean".ToLower() || getItem(Ring2Slot).Itemname.ToLower() == "Ring Of The Unclean".ToLower() ||
                getItem(Ring1Slot).Itemname.ToLower() == "Five Fingered Ring".ToLower() || getItem(Ring2Slot).Itemname.ToLower() == "Five Fingered Ring".ToLower())
            {
                Debug.WriteLine("ROTU or FFR Effect");
                if (rd.Next(2) == 1) { setSlot(MeleeSlot, reflist["_Fists"]); }

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
            return ri.ID;

        }
        private void setSlot(Image i, RemnantItem ri)
        {
            i.Source = ri.Image;
            ToolTipService.SetShowDuration(i, 60000);
            i.ToolTip = ri.Itemname + "\n" + ri.Description;
        }
        private void setModSlot(Image i, Image s, RemnantItem ri)
        {
            i.Source = ri.Image;
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
                i.Source = reflist[ri.Mod].Image;
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
                i.Source = mo.Image;
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
                if (BuildNameEnter.Text == "") { throw new Exception("No Name Assigned to Build!"); }
                Build b = Build.fromCode(BuildNameEnter.Text, BuildCodeEnter.Text);
                if (!presets.Contains(Build.fromCode(BuildNameEnter.Text, BuildCodeEnter.Text)))
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

            if (cmbCharacter.SelectedIndex == -1 && listCharacters.Count > 0) return;
            if (cmbCharacter.Items.Count > 0 && cmbCharacter.SelectedIndex > -1)
            {
                Debug.WriteLine("CHANGED TO "+listCharacters[cmbCharacter.SelectedIndex].ToString());
                
            }
        }

        private void disablemissing_Click(object sender, RoutedEventArgs e)
        {
            RemnantCharacter rc = listCharacters[cmbCharacter.SelectedIndex];
            Debug.WriteLine(rc.ToString()+ " Has "+rc.GetMissingItems().Count+" Missing items.");
            foreach (Item ri in rc.GetMissingItems()) {
                reflist[ri.ItemAltName].Disabled = true;
            }
            WeaponList.Items.Refresh();
            ArmorList.Items.Refresh();
            AmuletList.Items.Refresh();
            RingList.Items.Refresh();
            ModList.Items.Refresh();
            //BuildList.Items.Refresh();

        }

        private void ResetBlack_Click(object sender, RoutedEventArgs e)
        {
            foreach (RemnantItem ri in reflist.Values) {
                ri.Disabled = false;
            }
            WeaponList.Items.Refresh();
            ArmorList.Items.Refresh();
            AmuletList.Items.Refresh();
            RingList.Items.Refresh();
            ModList.Items.Refresh();
            //BuildList.Items.Refresh();
        }
    }
}

