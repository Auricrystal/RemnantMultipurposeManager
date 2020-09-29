using System;
using System.Diagnostics;
using System.IO;
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
            //Debug.WriteLine("Mod total: "+GearList[SlotType.MO].Count);
            //Debug.WriteLine(reflist["Tentacle Shot"].Itemname);


        }



        private void ReRollImg(object sender, RoutedEventArgs e)
        {
            RerollSlot(HeadSlot, SlotType.HE);
            RerollSlot(ChestSlot, SlotType.CH);
            RerollSlot(LegSlot, SlotType.LE);
            RerollSlot(HandGunSlot, SlotType.HG);
            RerollSlot(LongGunSlot, SlotType.LG);
            RerollSlot(MeleeSlot, SlotType.M);
            RerollSlot(AmuletSlot, SlotType.AM);
            RerollSlot(Ring1Slot, SlotType.RI);
            RerollSlot(Ring2Slot, SlotType.RI);

            RerollMod(Mod1Cover,Mod1Slot, hg);
            RerollMod(Mod2Cover,Mod2Slot, lg);
        }
        public void RerollSlot(Image i, SlotType st)
        {
            int rand = rd.Next(0, GearList[st].Count);
            RemnantItem ri = GearList[st][rand];
            if (st == SlotType.HG) { hg = ri; }
            if (st == SlotType.LG) { lg = ri; }
            i.Source = ri.Image;
            ToolTipService.SetShowDuration(i, 60000);
            i.ToolTip =ri.Itemname+"\n"+ ri.Description;
        }
        public void RerollMod(Image c,Image i, RemnantItem ri)
        {
            Debug.WriteLine(ri.Itemname + "==" + ri.Mod);
            ToolTipService.SetShowDuration(i, 60000);
            ToolTipService.SetShowDuration(c, 60000);
            if (reflist.ContainsKey(ri.Mod))
            {
                i.Source = reflist[ri.Mod].Image;
                c.ToolTip = reflist[ri.Mod].Itemname+"\n"+ reflist[ri.Mod].Description;
                Debug.WriteLine("Boss Mod: "+ reflist[ri.Mod].Itemname);
            }
            else {
                int rand = rd.Next(0, 28);
                RemnantItem mo = GearList[SlotType.MO][rand];
                i.Source = mo.Image;
                c.ToolTip = mo.Itemname+"\n"+mo.Description;
                Debug.WriteLine("Regular Mod: " + reflist[mo.Itemname].Itemname);
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

    }
}
