using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
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
            if (list.Count == 0) { return default; }
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
            while (items.Count < count) { items.Add(null); }
            return items;
        }
        public static List<int?> ToInt(this InventoryItem ii)
        {
            return new List<int?>() { ii.Index, ii.ModIndex };
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
            string s = GearInfo.GetItem(b.Data[AM]?[0])?.Name;
            if (s == "White Rose")
            {
                Debug.WriteLine(s);
                if (rd.Next(2) == 1) { Debug.WriteLine("removed hg"); b.TryRemove(HG); }
                if (rd.Next(2) == 1) { Debug.WriteLine("removed lg"); b.TryRemove(LG); }
            }
            else if (s == "Daredevil's Charm")
            {
                Debug.WriteLine(s);
                if (rd.Next(2) == 1) { Debug.WriteLine("removed he"); b.TryRemove(HE); }
                if (rd.Next(2) == 1) { Debug.WriteLine("removed ch"); b.TryRemove(CH); }
                if (rd.Next(2) == 1) { Debug.WriteLine("removed le"); b.TryRemove(LE); }
            }
            var list = new List<string> { "Ring Of The Unclean", "Five Fingered Ring" };
            var rings = b.Data[RI].ToList().Where(x => x != null).Select(x => GearInfo.GetItem(x).Name);
            if (rings.Contains(list[0]) || rings.Contains(list[1]))
            {
                Debug.WriteLine(s);
                if (rd.Next(2) == 1) { Debug.WriteLine("removed m"); b.TryRemove(M); }
            }

            return b;
        }
        public static List<T> ToList<T>(this Tuple<T, T> tup)
        {
            return new List<T>() { tup.Item1, tup.Item2 };
        }
        public static Build RandomBuild(this IEnumerable<InventoryItem> inventory, Build except = null, List<InventoryItem> blacklist = null)
        {
            blacklist = blacklist ?? new List<InventoryItem>();
            except = except ?? new Build();
            blacklist.AddRange(except.getItems().Select(x => GearInfo.GetItem(x)));
            List<InventoryItem> list = inventory.Select(x => (InventoryItem)x.Clone()).Except(blacklist, new InventoryItemComparer()).ToList();
            InventoryItem hg = list.HandGuns().RandomElement() ?? GearInfo.GetEmpty(HG), lg = list.LongGuns().RandomElement() ?? GearInfo.GetEmpty(LG);
            var rings = list.Rings().RandomElement(2);
            var mods = list.RegMods().RandomElement(2).Select(x => x?.Index ?? null).ToList();
            if (mods.Count < 2) { mods.Add(null); }
            Debug.WriteLine("Mods: " + mods.Count());
            var mod1 = hg.ModIndex ?? mods.ToArray()[0];
            var mod2 = lg.ModIndex ?? mods.ToArray()[1];
            Debug.WriteLine("Mods: " + mod1 + " : " + mod2);
            Build b = new("",
                new List<int?>() { hg.Index, mod1 },
                new List<int?>() { lg.Index, mod2 },
                new List<int?>() { list.Melees().RandomElement().Index },
                new List<int?>() { list.Heads().RandomElement().Index },
                new List<int?>() { list.Chests().RandomElement().Index },
                new List<int?>() { list.Legs().RandomElement().Index },
                new List<int?>() { list.Amulets().RandomElement().Index },
                new List<int?>() { rings?[0].Index, rings?[1].Index }
                );
            return b.Conditions();
        }
    }
}
