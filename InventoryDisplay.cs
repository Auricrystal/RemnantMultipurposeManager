using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RemnantBuildRandomizer
{
    public class InventoryDisplay
    {
        public Slot HandGun;
        public Slot HandGunMod;

        public Slot LongGun;
        public Slot LongGunMod;

        public Slot Melee;

        public Slot Head;
        public Slot Chest;
        public Slot Legs;

        public Slot Amulet;
        public Slot[] Rings;

        public void EquipBuild(Build b)
        {
            HandGun.Equip(b.HandGun);
            HandGunMod.Equip(b.HandGun.Mod);
            LongGun.Equip(b.LongGun);
            LongGunMod.Equip(b.LongGun.Mod);
            Melee.Equip(b.Melee);
            Head.Equip(b.Head);
            Chest.Equip(b.Chest);
            Legs.Equip(b.Legs);
            Amulet.Equip(b.Amulet);
            Rings[0].Equip(b.Ring1);
            Rings[1].Equip(b.Ring2);
        }

    }
    public struct Slot
    {
        private Image Image { get; set; }
        private TextBlock TextBlock { get; set; }
        private InventoryItem.SlotType SlotType { get; set; }
        private InventoryItem Item { get; set; }
        public Slot(ref Image i, ref TextBlock tb, InventoryItem.SlotType sl)
        {
            Image = i;
            TextBlock = tb;
            SlotType = sl;
            Item = null;
        }

        public void Equip(InventoryItem ii)
        {
            if (ii == null || ii.Name.Contains("_")) { UnEquip(); return; }
            if (ii?.Slot != SlotType) { Debug.WriteLine("Invalid Equip! " + ii.Slot + "!=" + SlotType); return; }

            Item = ii;
            Debug.WriteLine("Equipping: " + Item.Name);
            Debug.WriteLine("Image: " + Item.GetImage()!=null);
            Image.Source = Item.GetImage();
            TextBlock.Text = Item.Name;
        }
        public void UnEquip()
        {
            Item = GearInfo.GetEmpty(SlotType);
            Image.Source = Item.GetImage();
            TextBlock.Text = null;
        }


    }
}
