using System;
using System.Collections.Generic;

using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RemnantMultipurposeManager.InventoryItem.SlotType;


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
            return list.Where(x => x.Slot == HG).ToList();
        }
        public static List<InventoryItem> LongGuns(this List<InventoryItem> list)
        {
            return list.Where(x => x.Slot == LG).ToList();
        }
        public static List<InventoryItem> Boss(this List<InventoryItem> list)
        {
            return list.Where(x => x.Boss).ToList();
        }
        public static List<InventoryItem> NonBoss(this List<InventoryItem> list)
        {
            return list.Where(x => !x.Boss).ToList();
        }
        public static List<InventoryItem> Melees(this List<InventoryItem> list)
        {
            return list.Where(x => x.Slot == M).ToList();
        }
        public static List<InventoryItem> Heads(this List<InventoryItem> list)
        {
            return list.Where(x => x.Slot == HE).ToList();
        }
        public static List<InventoryItem> Chests(this List<InventoryItem> list)
        {
            return list.Where(x => x.Slot == CH).ToList();
        }
        public static List<InventoryItem> Legs(this List<InventoryItem> list)
        {
            return list.Where(x => x.Slot == LE).ToList();
        }
        public static List<InventoryItem> Amulets(this List<InventoryItem> list)
        {
            return list.Where(x => x.Slot == AM).ToList();
        }
        public static List<InventoryItem> Rings(this List<InventoryItem> list)
        {
            return list.Where(x => x.Slot == RI).ToList();
        }
        public static List<InventoryItem> RegMods(this List<InventoryItem> list)
        {
            var test = list.Where(x => x.Slot == MO).Where(y => y.Boss == false).ToList();
            return test;
        }
        #endregion
        public static Build Conditions(this Build b)
        {
            Random rd = MainWindow.rd;
            if (GearInfo.GetItem(b.Amulet).Name == "White Rose")
            {
                if (rd.Next(2) == 1) { b.HandGun = GearInfo.GetEmpty(HG).Index; }
                if (rd.Next(2) == 1) { b.LongGun = GearInfo.GetEmpty(LG).Index; }
            }
            else if (GearInfo.GetItem(b.Amulet).Name == "Daredevil's Charm")
            {
                string text = "\n\nDDC EFFECT\n";
                Debug.WriteLine(text);
                if (rd.Next(2) == 1) { b.Head = GearInfo.GetEmpty(HE).Index; }
                if (rd.Next(2) == 1) { b.Chest = GearInfo.GetEmpty(CH).Index; }
                if (rd.Next(2) == 1) { b.Legs = GearInfo.GetEmpty(LE).Index; }
            }
            List<string> rings = GearInfo.GetItems(b.Ring1, b.Ring2).Select(x => x.Name.ToLower()).ToList();

            if (rings.Contains("Ring Of The Unclean".ToLower()) || rings.Contains("Five Fingered Ring".ToLower()))
            {
                Debug.WriteLine("ROTU or FFR Effect");
                if (rd.Next(2) == 1) { b.Melee = GearInfo.GetEmpty(M).Index; }
            }

            return b;
        }
        public static Build RandomBuild(this List<InventoryItem> inventory, Build except = null, List<InventoryItem> blacklist = null)
        {
            blacklist = blacklist ?? new List<InventoryItem>();
            except = except ?? new Build((InventoryItem)null);
            blacklist.AddRange(except.ToInventory().Select(x => GearInfo.GetItem(x)));
            List<InventoryItem> list = inventory.Select(x => (InventoryItem)x.Clone()).Except(blacklist, new InventoryItemComparer()).ToList();
            InventoryItem hg, lg;
            var rings = list.Rings().RandomElement(2);
            var mods = list.RegMods().RandomElement(2);

            Debug.WriteLine("Mods:\n" + mods[0].ToString() + "\n" + mods[1].ToString());

            Build b = new Build(
                (hg = list.HandGuns().RandomElement()).Index,
                hg.ModIndex > 0 ? hg.ModIndex : mods[0].Index,
                (lg = list.LongGuns().RandomElement()).Index,
                lg.ModIndex > 0 ? lg.ModIndex : mods[1].Index,
                list.Melees().RandomElement().Index,
                list.Heads().RandomElement().Index,
                list.Chests().RandomElement().Index,
                list.Legs().RandomElement().Index,
                list.Amulets().RandomElement().Index,
                rings?[0].Index,
               rings?[1].Index
                );
            return b.Conditions();
        }
    }
}
