﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static RemnantMultipurposeManager.Equipment;
using static RemnantMultipurposeManager.Equipment.SlotType;

namespace RemnantMultipurposeManager
{
    public class InventoryUI : Canvas
    {

        //256x1024 
        //512x512
        private StackPanel Main, Offense, Defense, Trinkets;
        private double Size;
        private const int _DefSize = 1024;
        public GunSlot HandGun, LongGun;
        public ModSlot HandGunMod, LongGunMod;
        public InventorySlot Melee, Head, Chest, Legs, Amulet, Ring1, Ring2;
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
            HandGunMod = new ModSlot(MO, rectH * 0.80, rectH * 1.60, false) { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 32, 0) };
            Offense.Children.Add(HandGun = new GunSlot(HG, rectH, rectW, HandGunMod) { Background = bg, Margin = new Thickness(0, 1, 0, 1) });
            HandGun.Children.Add(HandGunMod);
            HandGunMod.Children.Add(new Image() { Name = "HGM", Source = bmp, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, -12, 0) });

            LongGunMod = new ModSlot(MO, rectH * 0.80, rectH * 1.60, false) { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 32, 0) };
            Offense.Children.Add(LongGun = new GunSlot(LG, rectH, rectW, LongGunMod) { Background = bg, Margin = new Thickness(0, 1, 0, 1) });

            LongGun.Children.Add(LongGunMod);
            LongGunMod.Children.Add(new Image() { Name = "LGM", Source = bmp, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, -12, 0) });

            Offense.Children.Add(Melee = new InventorySlot(M, rectH, rectW) { Background = bg, Margin = new Thickness(0, 1, 0, 1) });
            Defense.Children.Add(Head = new InventorySlot(HE, square, square) { Background = bg, Margin = new Thickness(1, 0, 1, 0) });
            Defense.Children.Add(Chest = new InventorySlot(CH, square, square) { Background = bg, Margin = new Thickness(1, 0, 1, 0) });
            Defense.Children.Add(Legs = new InventorySlot(LE, square, square) { Background = bg, Margin = new Thickness(1, 0, 1, 0) });
            Trinkets.Children.Add(Amulet = new InventorySlot(AM, square, square) { Background = bg, Margin = new Thickness(1, 0, 1, 0) });
            Trinkets.Children.Add(Ring1 = new InventorySlot(RI, square, square) { Background = bg, Margin = new Thickness(1, 0, 1, 0) });
            Trinkets.Children.Add(Ring2 = new InventorySlot(RI, square, square) { Background = bg, Margin = new Thickness(1, 0, 1, 0) });

            Array = new InventorySlot[] { HandGun, LongGun, Melee, Head, Chest, Legs, Amulet, Ring1, Ring2 };
        }

        public void CheckBuild()
        {
            Build b = Shown;
            Random rd = MainWindow.rd;
            string s = b.GetItems().Find(x => x.Slot == AM).Name;
            if (s == "White Rose")
            {
                Debug.WriteLine(s + " Detected");
                if (rd.Next(2) == 1) { Debug.WriteLine("removed hg"); HandGun.UnEquip(); }
                if (rd.Next(2) == 1) { Debug.WriteLine("removed lg"); LongGun.UnEquip(); }
            }
            else if (s == "Daredevil's Charm")
            {
                Debug.WriteLine(s + " Detected");
                if (rd.Next(2) == 1) { Debug.WriteLine("removed he"); Head.UnEquip(); }
                if (rd.Next(2) == 1) { Debug.WriteLine("removed ch"); Chest.UnEquip(); }
                if (rd.Next(2) == 1) { Debug.WriteLine("removed le"); Legs.UnEquip(); }
            }
            var list = new List<string> { "Ring Of The Unclean", "Five Fingered Ring" };
            var rings = (b.GetItems().FindAll(x => x.Slot == RI)).Select(y => y.Name);
            if (rings.Contains(list[0]) || rings.Contains(list[1]))
            {
                Debug.WriteLine("Ring Removal Detected");
                if (rd.Next(2) == 1) { Debug.WriteLine("removed m"); Melee.UnEquip(); }
            }
        }
        public Build CaptureSlots()
        {
            var list = Array.Where(x => x.Item != null).Select(x => x.Item).ToList();
            Debug.WriteLine("CAPTURE BUILD");
            foreach (var item in list)
                Debug.WriteLine(item);
            HandGun hg = list[0] as HandGun;
            LongGun lg = list[1] as LongGun;
            return new Build("Shown", hg.Name, hg.Mod?.Name, lg.Name, lg.Mod?.Name, list[2].Name, list[3].Name, list[4].Name, list[5].Name, list[6].Name, list[7].Name, list[8].Name);
        }

        public void EquipBuild(Build b)
        {
            Debug.WriteLine("Build: " + b);
            if (b.GetItems().Count == 0)
                foreach (var slot in Array)
                    slot.UnEquip();
            HandGun.Equip(b.HandGun);
            
            HandGunMod.Equip(b.HandGunMod);

            LongGun.Equip(b.LongGun);
            LongGunMod.Equip(b.LongGunMod);

            Melee.Equip(b.Melee);

            Head.Equip(b.Head);
            Chest.Equip(b.Chest);
            Legs.Equip(b.Legs);

            Amulet.Equip(b.Amulet);
            Ring1.Equip(b.RingLeft);
            Ring2.Equip(b.RingRight);

            CheckBuild();
        }
    }
    public class ModSlot : InventorySlot
    {
        public ModSlot(SlotType sl, double height, double width, bool border = true) : base(sl, height, width, border)
        {
        }

        public GunSlot GetParentSlot()
        {
            if (Parent is GunSlot)
                Debug.WriteLine("Parent Gun Found!");
            return Parent as GunSlot;
        }

        public void Equip(Mod mod)
        {
           ( GetParentSlot().Item as Gun).EquipMod(mod);
            base.Equip(mod);
        }
    }
    public class GunSlot : InventorySlot
    {
        private InventorySlot modslot;
        public GunSlot(SlotType sl, double height, double width, InventorySlot mod, bool border = true) : base(sl, height, width, border)
        {
            modslot = mod;
        }

        public override void Equip(Equipment gun)
        {
            if (gun is not Gun)
            {
                Debug.WriteLine($"Invalid Equip!{gun.Name} is not a Gun!");
                return;
            }
                
            Debug.WriteLine("Equip Gun");
            if (gun is null) { UnEquip(); return; }
            if (gun.Slot != SlotType) { Debug.WriteLine("Invalid Equip! " + gun.Slot + "!=" + SlotType); return; }
            item = gun;
            DisplayImage.Source = Item.GetImage;
            DisplayName.Text = (!Item.Name.Contains("_")) ? Item.Name : "";

            modslot.Equip((gun as Gun).Mod);
        }
        public override void Equip(string ii)
        {
            var item = EquipmentDirectory.FindEquipmentByName(ii);
            if (item is null)
                return;
            if (item.Slot == SlotType)
                Equip(item);
            else
                Debug.WriteLine($"Invalid Equip of {item.Name} to slot {SlotType}.");
        }

        public override void UnEquip()
        {
            base.UnEquip();
            modslot.UnEquip();
        }

    }
    public class InventorySlot : Grid
    {
        protected Image DisplayImage { get; set; }
        protected TextBlock DisplayName { get; set; }
        public SlotType SlotType { get; set; }
        protected Equipment item;
        public Equipment Item
        {
            get
            {
                if (item is null)
                    item = EquipmentDirectory.DefaultEquipment(SlotType);
                return item;
            }
            set { Equip(value); item = value; }
        }
        public InventorySlot(SlotType sl, double height, double width, bool border = true)
        {
            MouseDown += MainWindow.Instance.InventorySlot_MouseDown;
            Border b = null;
            this.Height = height;
            this.Width = width;
            item = EquipmentDirectory.DefaultEquipment(sl);
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

        public virtual void Equip(Equipment ii)
        {

            if (ii is null) { UnEquip(); return; }
            if (ii?.Slot != SlotType) { Debug.WriteLine("Invalid Equip! " + ii.Slot + "!=" + SlotType); return; }
            item = ii;
            DisplayImage.Source = (SlotType is SlotType.MO && ii.Name.Contains("_")) ? null : Item.GetImage;
            DisplayName.Text = (!ii.Name.Contains("_")) ? Item.Name : "";
        }
        public virtual void Equip(string ii)
        {
            var item = EquipmentDirectory.FindEquipmentByName(ii);
            if (item is null)
                return;
            if (item.Slot == SlotType)
                Equip(item);
            else
                Debug.WriteLine($"Invalid Equip of {item.Name} to slot {SlotType}.");
        }

        public virtual void UnEquip()
        {
           
            Debug.WriteLine($"Unequipping {item?.Name}");
            Debug.WriteLine($"From Slot {SlotType}");
            Equip(EquipmentDirectory.DefaultEquipment(SlotType));

        }
    }
}