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
        private const int _DefSize = 256;
        public InventorySlot HandGun, HandGunMod, LongGun, LongGunMod, Melee, Head, Chest, Legs, Amulet, Ring1, Ring2;
        public Build Shown { get; private set; }

        public InventoryUI(double size = 1)
        {
            Size = size;
            Init();

        }
        private void Init()
        {
            Children.Add(Main = new StackPanel() { Orientation = Orientation.Vertical });
            Main.Children.Add(Offense = new StackPanel() { Orientation = Orientation.Vertical, Margin = new Thickness(2, 2, 0, 0) });
            Main.Children.Add(Defense = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(2, 2, 0, 0) });
            Main.Children.Add(Trinkets = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(2, 2, 0, 0) });
            var bmp = new BitmapImage(new Uri(@"pack://application:,,,/Resources/IMG/Mods/Mod Overlay.png"));


            double
                rectH = (_DefSize * Size) / 4.5,
                rectW = _DefSize * Size,
                square = rectW / 3;
            this.Height = (rectH * 3 + square * 2);
            this.Width = rectW;
            SolidColorBrush bg = new SolidColorBrush(new Color() { R = 21, G = 21, B = 21, A = 255 });
            Offense.Children.Add(HandGun = new InventorySlot(HG, rectH, rectW) { Background = bg }); ;
            HandGun.Children.Add(HandGunMod = new InventorySlot(MO, rectH * 0.80, rectH * 0.80, false) { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 5, 0) });
            HandGunMod.Children.Add(new Image() {Name="HGM", Source = bmp });
            Offense.Children.Add(LongGun = new InventorySlot(LG, rectH, rectW) { Background = bg });
            LongGun.Children.Add(LongGunMod = new InventorySlot(MO, rectH * 0.80, rectH * 0.80, false) { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 5, 0) });
            LongGunMod.Children.Add(new Image() { Name = "LGM", Source = bmp });
            Offense.Children.Add(Melee = new InventorySlot(M, rectH, rectW) { Background = bg });
            Defense.Children.Add(Head = new InventorySlot(HE, square, square) { Background = bg });
            Defense.Children.Add(Chest = new InventorySlot(CH, square, square) { Background = bg });
            Defense.Children.Add(Legs = new InventorySlot(LE, square, square) { Background = bg });
            Trinkets.Children.Add(Amulet = new InventorySlot(AM, square, square) { Background = bg });
            Trinkets.Children.Add(Ring1 = new InventorySlot(RI, square, square) { Background = bg });
            Trinkets.Children.Add(Ring2 = new InventorySlot(RI, square, square) { Background = bg });
        }
        public void EquipBuild(Build b)
        {
            Shown = b;
            HandGun.Equip(b?.HandGun);
            HandGunMod.Equip(b?.HandGun?.Mod);
            LongGun.Equip(b?.LongGun);
            LongGunMod.Equip(b?.LongGun?.Mod);
            Melee.Equip(b?.Melee);
            Head.Equip(b?.Head);
            Chest.Equip(b?.Chest);
            Legs.Equip(b?.Legs);
            Amulet.Equip(b?.Amulet);
            Ring1.Equip(b?.Ring1);
            Ring2.Equip(b?.Ring2);
        }
    }
    public class InventorySlot : Grid
    {
        private Image DisplayImage { get; set; }
        private TextBlock DisplayName { get; set; }
        public InventoryItem.SlotType SlotType { get; set; }
        private InventoryItem Item { get; set; }
        public InventorySlot(InventoryItem.SlotType sl, double height, double width, bool border = true)
        {
            Border b = null;
            this.Height = height;
            this.Width = width;
            Item = GearInfo.GetEmpty(sl);

            DisplayImage = new Image() { Source = (sl != MO) ? Item.GetImage() : null, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Width = width * 0.85, Height = height * 0.85 };
            if (border)
            {
                Debug.WriteLine("Border True");
                b = new Border() { BorderBrush = new SolidColorBrush(new Color() { R = 30, G = 30, B = 30, A = 255 }), Child = DisplayImage, BorderThickness = new Thickness(2), Background = new SolidColorBrush(new Color() { R = 15, G = 15, B = 15, A = 255 }) };
            }
            this.Children.Add(b ?? (UIElement)DisplayImage);
            this.Children.Add(DisplayName = new TextBlock() { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Padding = new Thickness(5, 5, 0, 0), TextWrapping = TextWrapping.Wrap, FontSize = 8 }); ;
            Debug.WriteLine("Font Size:" + DisplayName.FontSize);
            SlotType = sl;
            Item = null;
        }

        public void Equip(InventoryItem ii)
        {
            if (ii == null || ii.Name.Contains("_")) { UnEquip(); return; }
            if (ii?.Slot != SlotType) { Debug.WriteLine("Invalid Equip! " + ii.Slot + "!=" + SlotType); return; }

            Item = ii;

            Debug.WriteLine("Equipping: " + Item.Name + " Index: " + Item.Index());
            DisplayImage.Source = Item.GetImage();
            DisplayName.Text = (SlotType != MO) ? Item.Name : null;
            var img = this?.Children?.OfType<Image>().Where(x=>x.Name=="LGM"||x.Name=="HGM");
            if (img.Count() > 0) { img.First().ToolTip = Item.Name; }
                
        }


        public void UnEquip()
        {
            Item = GearInfo.GetEmpty(SlotType);

            DisplayImage.Source = (SlotType != MO) ? Item.GetImage() : null;
            DisplayName.Text = null;
            var img = this?.Children?.OfType<Image>().Where(x => x.Name == "LGM" || x.Name == "HGM");
            if (img.Count() > 0) { img.First().ToolTip = null; }
        }



    }

}
