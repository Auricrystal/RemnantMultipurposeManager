using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using static RemnantBuildRandomizer.GearInfo;
using static RemnantBuildRandomizer.RemnantItem;

namespace RemnantBuildRandomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region ScaleValue Depdency Property
        Random rd = new Random();
        RemnantItem hg;
        RemnantItem lg;

        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(MainWindow), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));

        public MainWindow()
        {
            InitializeComponent();
            ReadXML();
            getBlacklist();
            HeadList.ItemsSource = reflist.Values.Where(x => x.Slot == SlotType.HE);
            ChestList.ItemsSource = reflist.Values.Where(x => x.Slot == SlotType.CH);
            LegsList.ItemsSource = reflist.Values.Where(x => x.Slot == SlotType.LE);
            HandGList.ItemsSource = reflist.Values.Where(x => x.Slot == SlotType.HG);
            LongGList.ItemsSource = reflist.Values.Where(x => x.Slot == SlotType.LG);
            MeleeList.ItemsSource = reflist.Values.Where(x => x.Slot == SlotType.M);
            AmuletList.ItemsSource = reflist.Values.Where(x => x.Slot == SlotType.AM);
            RingList.ItemsSource = reflist.Values.Where(x => x.Slot == SlotType.RI);
        }
        private void ResetDisabled()
        {
            resetList(HeadList);
            resetList(ChestList);
            resetList(LegsList);
            resetList(HandGList);
            resetList(LongGList);
            resetList(MeleeList);
            resetList(AmuletList);
            resetList(RingList);
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
            RedCrystal.ToolTip = null;
            ResetDisabled();
            //getBlacklist();
            RerollSlot(HeadSlot, SlotType.HE);
            RerollSlot(ChestSlot, SlotType.CH);
            RerollSlot(LegSlot, SlotType.LE);
            RerollSlot(HandGunSlot, SlotType.HG);
            RerollSlot(LongGunSlot, SlotType.LG);
            RerollSlot(MeleeSlot, SlotType.M);
            RerollSlot(AmuletSlot, SlotType.AM);
            int num = RerollSlot(Ring1Slot, SlotType.RI);
            while (RerollSlot(Ring2Slot, SlotType.RI) == num) ;
            RemnantItem ri=RerollMod(Mod1Cover, Mod1Slot, hg);
            while (ri.Itemname == RerollMod(Mod2Cover, Mod2Slot, lg).Itemname) ;
            
            Conditions();

        }
        private void Conditions()
        {
            if (getItem(AmuletSlot).Itemname == "White Rose")
            {
                string text = "\n\nWHITE ROSE EFFECT\n";
                Debug.WriteLine(text);
                if (rd.Next(2) == 1) { setSlot(HandGunSlot, reflist["Hand Gun"]); setSlot(Mod1Slot, reflist["_Mod"]); Mod1Cover.ToolTip = null; text += "removed HG\n"; }
                if (rd.Next(2) == 1) { setSlot(LongGunSlot, reflist["Long Gun"]); setSlot(Mod2Slot, reflist["_Mod"]); Mod2Cover.ToolTip = null; text += "removed LG"; }
                AmuletSlot.ToolTip += text;
            }
            else if (getItem(AmuletSlot).Itemname == "Daredevil's Charm")
            {
                string text = "\n\nDDC EFFECT\n";
                Debug.WriteLine(text);
                if (rd.Next(2) == 1) { setSlot(HeadSlot, reflist["_Head"]); text += "removed Head\n"; }
                if (rd.Next(2) == 1) { setSlot(ChestSlot, reflist["_Chest"]); text += "removed Chest\n"; }
                if (rd.Next(2) == 1) { setSlot(LegSlot, reflist["_Legs"]); text += "removed Legs"; }
                AmuletSlot.ToolTip += text;
            }
            if (getItem(Ring1Slot).Itemname.ToLower() == "Ring Of The Unclean".ToLower() || getItem(Ring2Slot).Itemname.ToLower() == "Ring Of The Unclean".ToLower() ||
                getItem(Ring1Slot).Itemname.ToLower() == "Five Fingered Ring".ToLower() || getItem(Ring2Slot).Itemname.ToLower() == "Five Fingered Ring".ToLower()) {
                Debug.WriteLine("ROTU or FFR Effect");
                if (rd.Next(2) == 1) { setSlot(MeleeSlot, reflist["Fists"]); }
            
            }

        }
        private RemnantItem getItem(Image item)
        {
            Debug.WriteLine("getItem:"+ Path.GetFileName(item.Source.ToString()));
            return reflist[Path.GetFileName(item.Source.ToString()).Replace(".png","")];
        }
        public int RerollSlot(Image i, SlotType st)
        {
            int rand = rd.Next(0, GearList[st].Count);
            while (GearList[st][rand].Disabled)
            {
                Debug.WriteLine(GearList[st][rand].Itemname + "Cant USE!");
                rand = rd.Next(0, GearList[st].Count);
            }
            RemnantItem ri = GearList[st][rand];
            if (st == SlotType.HG) { hg = ri; }
            if (st == SlotType.LG) { lg = ri; }
            setSlot(i, ri);
            return rand;
        }
        private void setSlot(Image i, RemnantItem ri)
        {
            i.Source = ri.Image;
            ToolTipService.SetShowDuration(i, 60000);
            i.ToolTip = ri.Itemname + "\n" + ri.Description;
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
                int rand = rd.Next(0, 28);
                while (GearList[SlotType.MO][rand].Disabled) { rand = rd.Next(0, 28); }
                RemnantItem mo = GearList[SlotType.MO][rand];
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
    }
}
