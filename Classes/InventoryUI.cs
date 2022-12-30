using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static RemnantMultipurposeManager.InventoryItem.SlotType;

namespace RemnantMultipurposeManager
{
    public class InventoryUI : Canvas
    {

        //256x1024 
        //512x512
        private StackPanel Main, Offense, Defense, Trinkets;
        private double Size;
        private const int _DefSize = 1024;
        public InventorySlot HandGun, HandGunMod, LongGun, LongGunMod, Melee, Head, Chest, Legs, Amulet, Ring1, Ring2;
        public Build Shown { get => CaptureSlots(); }
        public InventorySlot[] Array { get; private set; }
        public InventoryUI(double size = 1)
        {
            Size = size;
            Init();

        }
        private void Init()
        {
            Children.Add(Main = new StackPanel() { Orientation = Orientation.Vertical });
            Main.Children.Add(Offense = new StackPanel() { Orientation = Orientation.Vertical, Margin = new Thickness(0) });
            Main.Children.Add(Defense = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0) });
            Main.Children.Add(Trinkets = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0) });
            var bmp = new BitmapImage(new Uri(@"pack://application:,,,/Resources/IMG/Mods/Mod Overlay.png"));


            double
                rectH = (_DefSize * Size) / 4.5,
                rectW = _DefSize * Size,
                square = rectW / 3;
            this.Height = (rectH * 3 + square * 2);
            this.Width = rectW + 4;
            SolidColorBrush bg = null;//new SolidColorBrush(new Color() { R = 21, G = 21, B = 21, A = 255 });
            Offense.Children.Add(HandGun = new InventorySlot(HG, rectH, rectW) { Background = bg, Margin = new Thickness(0, 1, 0, 1) }); ;
            HandGun.Children.Add(HandGunMod = new InventorySlot(MO, rectH * 0.80, rectH * 1.60, false) { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 32, 0) });
            //Panel.SetZIndex(HandGunMod, 0);
            HandGunMod.Children.Add(new Image() { Name = "HGM", Source = bmp, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, -12, 0) });
            Offense.Children.Add(LongGun = new InventorySlot(LG, rectH, rectW) { Background = bg, Margin = new Thickness(0, 1, 0, 1) });
            LongGun.Children.Add(LongGunMod = new InventorySlot(MO, rectH * 0.80, rectH * 1.60, false) { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 32, 0) });
            //Panel.SetZIndex(LongGunMod, 0);
            LongGunMod.Children.Add(new Image() { Name = "LGM", Source = bmp, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, -12, 0) });
            Offense.Children.Add(Melee = new InventorySlot(M, rectH, rectW) { Background = bg, Margin = new Thickness(0, 1, 0, 1) });
            Defense.Children.Add(Head = new InventorySlot(HE, square, square) { Background = bg, Margin = new Thickness(1, 0, 1, 0) });
            Defense.Children.Add(Chest = new InventorySlot(CH, square, square) { Background = bg, Margin = new Thickness(1, 0, 1, 0) });
            Defense.Children.Add(Legs = new InventorySlot(LE, square, square) { Background = bg, Margin = new Thickness(1, 0, 1, 0) });
            Trinkets.Children.Add(Amulet = new InventorySlot(AM, square, square) { Background = bg, Margin = new Thickness(1, 0, 1, 0) });
            Trinkets.Children.Add(Ring1 = new InventorySlot(RI, square, square) { Background = bg, Margin = new Thickness(1, 0, 1, 0) });
            Trinkets.Children.Add(Ring2 = new InventorySlot(RI, square, square) { Background = bg, Margin = new Thickness(1, 0, 1, 0) });

            Array = new InventorySlot[] { HandGun, HandGunMod, LongGun, LongGunMod, Melee, Head, Chest, Legs, Amulet, Ring1, Ring2 };


        }
        public Build CaptureSlots()
        {

            return new Build("Shown", new Dictionary<InventoryItem.SlotType, object>()
            {
                {HG,new List<int>(){HandGun.Item.Index,HandGunMod.Item.Index}},
                {LG,new List<int>(){LongGun.Item.Index,LongGunMod.Item.Index}},
                {M, Melee.Item.Index},
                {HE, Head.Item.Index},
                {CH, Chest.Item.Index},
                {LE, Legs.Item.Index},
                {AM, Amulet.Item.Index},
                {RI,new List<int>(){Ring1.Item.Index,Ring2.Item.Index}},
            });


        }

        public void EquipBuild(Build b)
        {

            if (b.Data.Count == 0)
                foreach (var slot in Array)
                    slot.UnEquip();
            foreach (InventoryItem.SlotType st in b.Data.Keys)
            {

                InventorySlot slot = Array.ToList().Find(x => x.SlotType == st);
                if (st == RI)
                {
                    var rings = b.Data[st] as List<int>;

                    Ring2.Equip(rings[1]);
                    Ring1.Equip(rings[0]);
                    continue;
                }

                if (st == HG)
                {
                    slot.Equip(((List<int>)b.Data[st])[0]);
                    HandGunMod.Equip(((List<int>)b.Data[st])[1]);
                    continue;
                }
                if (st == LG)
                {
                    slot.Equip(((List<int>)b.Data[st])[0]);
                    LongGunMod.Equip(((List<int>)b.Data[st])[1]);
                    continue;
                }
                slot.Equip((int)b.Data[st]);
            }
        }
    }
    public class InventorySlot : Grid
    {
        private Image DisplayImage { get; set; }
        private TextBlock DisplayName { get; set; }
        public InventoryItem.SlotType SlotType { get; set; }
        private InventoryItem item;
        public InventoryItem Item
        {
            get
            {
                if (item is null)
                    item = GearInfo.GetEmpty(SlotType);
                return item;
            }
            set { Equip(value); item = value; }
        }
        public InventorySlot(InventoryItem.SlotType sl, double height, double width, bool border = true)
        {

            MouseDown += MainWindow.MW.InventorySlot_MouseDown;
            Border b = null;
            this.Height = height;
            this.Width = width;
            item = GearInfo.GetEmpty(sl);
            DisplayImage = new Image() { Source = (sl != MO) ? item.GetImage : null, HorizontalAlignment = (sl != MO) ? HorizontalAlignment.Center : HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Width = width * 0.85, Height = height * 0.85 };
            if (border)
            {
                b = new Border() { BorderBrush = new SolidColorBrush(new Color() { R = 30, G = 30, B = 30, A = 255 }), Child = DisplayImage, BorderThickness = new Thickness(6), CornerRadius = new CornerRadius(24), Background = new SolidColorBrush(new Color() { R = 15, G = 15, B = 15, A = 255 }) };
            }
            this.Children.Add(b ?? (UIElement)DisplayImage);

            this.Children.Add(new Border()
            {
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(),
                Child = DisplayName = new TextBlock()
                {

                    HorizontalAlignment = (sl != MO) ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                    VerticalAlignment = (sl != MO) ? VerticalAlignment.Top : VerticalAlignment.Bottom,
                    Padding = new Thickness(25, 25, 0, 0),
                    Margin = (sl != MO) ? new Thickness(0) : new Thickness(0, 0, 86, -20),
                    TextWrapping = (sl != MO) ? TextWrapping.Wrap : TextWrapping.NoWrap,
                    FontSize = 24,
                    FontFamily = new FontFamily(new Uri(@"pack://application:,,,/"), "./Resources/Fonts/#Montserrat Light")
                }
            });

            SlotType = sl;
            item = null;
        }



        public void Equip(InventoryItem ii)
        {
            if (ii == null || ii.Name.Contains("_")) { UnEquip(); return; }
            if (ii?.Slot != SlotType) { Debug.WriteLine("Invalid Equip! " + ii.Slot + "!=" + SlotType); return; }
            item = ii;
            DisplayImage.Source = Item.GetImage;
            DisplayName.Text = Item.Name;

            if (this.Children.OfType<InventorySlot>().Count() == 0)
                return;
            var modslot = Children.OfType<InventorySlot>().First();
            if (ii.Boss)
                modslot.Equip(ii?.ModIndex);
            else
                modslot.UnEquip();
        }
        public void Equip(int? ii)
        {
            Debug.WriteLine("Equipping: " + GearInfo.GetItem(ii)?.Name);
            Equip(GearInfo.GetItem(ii));
        }


        public void UnEquip()
        {
            item = GearInfo.GetEmpty(SlotType);

            DisplayImage.Source = (SlotType != MO) ? item.GetImage : null;
            DisplayName.Text = null;
            var img = this?.Children?.OfType<Image>().Where(x => x.Name == "LGM" || x.Name == "HGM");
            if (img.Count() > 0) { img.First().ToolTip = null; }

            if (this.Children.OfType<InventorySlot>().Count() == 0)
                return;
            Children.OfType<InventorySlot>().First().UnEquip();
        }
    }
}
