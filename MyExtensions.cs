using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RemnantBuildRandomizer
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
        public static Build RandomBuild(this List<InventoryItem> inventory)
        {
            return RandomBuild(inventory, new List<InventoryItem>());
        }
        public static Build RandomBuild(this List<InventoryItem> inventory, List<InventoryItem> blacklist)
        {
            Debug.WriteLine("Length:" + inventory.Count);
            List<InventoryItem> list = inventory.Except(MainWindow.curr.ToInventory(), new InventoryItemComparer()).ToList();
            Debug.WriteLine("Length:" + list.Count);

            var hg = list.HandGuns().RandomElement();
            Debug.WriteLine("Length:" + list.Count);
            var lg = list.LongGuns().RandomElement();
            Debug.WriteLine("Length:" + list.Count);
            var rings = list.Rings().RandomElement(2);
            Build b = new Build(
                hg,
                lg,
                list.Melees().RandomElement(),
                list.Heads().RandomElement(),
                list.Chests().RandomElement(),
                list.Legs().RandomElement(),
                list.Amulets().RandomElement(),
                rings[0],
               rings[1]
                );

            return b;
        }
    }
}
