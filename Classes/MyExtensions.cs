using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RemnantMultipurposeManager
{
    public static class MyExtensions
    {

        private static Random rng = MainWindow.rd;
        private static List<T> Combine<T>(params IEnumerable<T>[] rils)
        {
            List<T> li = new List<T>();
            foreach (IEnumerable<T> ril in rils) { li.AddRange(ril); }
            return li;
        }
        public static T RandomElement<T>(this List<T> list) where T : new()
        {
            T ii = list[rng.Next(list.Count)];
            list.Remove(ii);
            return ii;
        }
        public static List<InventoryItem> RandomElement(this List<InventoryItem> list, int count)
        {
            List<InventoryItem> items = new List<InventoryItem>();
            while (count > 0 && list.Count > 0)
            {
                count--;
                int r = MainWindow.rd.Next(list.Count);
                items.Add(list[r]);
                list.RemoveAt(r);
            }
            return items;
        }
        #region Inventory Stuff

        public static List<InventoryItem> Empties(this List<InventoryItem> list)
        {
            return list.Where(x => x.Name.Contains("_")).ToList();
        }
        public static List<InventoryItem> HandGuns(this List<InventoryItem> list)
        {
            return list.Where(x => x.Slot == InventoryItem.SlotType.HG).ToList();
        }
        public static List<InventoryItem> LongGuns(this List<InventoryItem> list)
        {
            return list.Where(x => x.Slot == InventoryItem.SlotType.LG).ToList();
        }
        public static List<InventoryItem> Boss(this List<InventoryItem> list)
        {
            return list.Where(x => x.Mod != null).ToList();
        }
        public static List<InventoryItem> NonBoss(this List<InventoryItem> list)
        {
            return list.Where(x => x.Mod == null).ToList();
        }
        public static List<InventoryItem> Melees(this List<InventoryItem> list)
        {
            return list.Where(x => x.Slot == InventoryItem.SlotType.M).ToList();
        }
        public static List<InventoryItem> Heads(this List<InventoryItem> list)
        {
            return list.Where(x => x.Slot == InventoryItem.SlotType.HE).ToList();
        }
        public static List<InventoryItem> Chests(this List<InventoryItem> list)
        {
            return list.Where(x => x.Slot == InventoryItem.SlotType.CH).ToList();
        }
        public static List<InventoryItem> Legs(this List<InventoryItem> list)
        {
            return list.Where(x => x.Slot == InventoryItem.SlotType.LE).ToList();
        }
        public static List<InventoryItem> Amulets(this List<InventoryItem> list)
        {
            return list.Where(x => x.Slot == InventoryItem.SlotType.AM).ToList();
        }
        public static List<InventoryItem> Rings(this List<InventoryItem> list)
        {
            return list.Where(x => x.Slot == InventoryItem.SlotType.RI).ToList();
        }
        public static List<InventoryItem> RegMods(this List<InventoryItem> list)
        {
            return list.Where(x => x.Slot == InventoryItem.SlotType.MO).Except(list.Where(x => x.Mod != null).Select(x => x.Mod)).ToList();
        }
        #endregion
        public static Build Conditions(this Build b)
        {
            Random rd = MainWindow.rd;
            if (b.Amulet.Name == "White Rose")
            {
                if (rd.Next(2) == 1) { b.HandGun = null; }
                if (rd.Next(2) == 1) { b.LongGun = null; }
            }
            else if (b.Amulet.Name == "Daredevil's Charm")
            {
                string text = "\n\nDDC EFFECT\n";
                Debug.WriteLine(text);
                if (rd.Next(2) == 1) { b.Head = null; }
                if (rd.Next(2) == 1) { b.Chest = null; }
                if (rd.Next(2) == 1) { b.Legs = null; }
            }
            if (b.Ring1.Name.ToLower() == "Ring Of The Unclean".ToLower() || b.Ring2.Name.ToLower() == "Ring Of The Unclean".ToLower() ||
                b.Ring1.Name.ToLower() == "Five Fingered Ring".ToLower() || b.Ring2.Name.ToLower() == "Five Fingered Ring".ToLower())
            {
                Debug.WriteLine("ROTU or FFR Effect");
                if (rd.Next(2) == 1) { b.Melee = null; }
            }

            return b;
        }
        public static Build RandomBuild(this List<InventoryItem> inventory, Build except = null, List<InventoryItem> blacklist = null)
        {
            blacklist = blacklist ?? new List<InventoryItem>();
            except = except ?? new Build();
            blacklist.AddRange(except.ToInventory());
            List<InventoryItem> list = inventory.Except(blacklist, new InventoryItemComparer()).ToList();
            InventoryItem hg, lg;
            var rings = list.Rings().RandomElement(2);
            var mods = list.RegMods().RandomElement(2);

            Build b = new Build(
                hg = list.HandGuns().RandomElement(),
                lg = list.LongGuns().RandomElement(),
                list.Melees().RandomElement(),
                list.Heads().RandomElement(),
                list.Chests().RandomElement(),
                list.Legs().RandomElement(),
                list.Amulets().RandomElement(),
                rings?[0],
               rings?[1]
                );
            hg.Mod = hg.Mod ?? mods?[0];
            lg.Mod = lg.Mod ?? mods?[1];

            return b.Conditions();
        }
    }
}
