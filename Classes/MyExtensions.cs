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
using static RemnantMultipurposeManager.Equipment.SlotType;


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
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(new Random());
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            return source.ShuffleIterator(rng);
        }
        private static IEnumerable<T> ShuffleIterator<T>(
        this IEnumerable<T> source, Random rng)
        {
            var buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }

        public static IEnumerable<T> RandomElement<T>(this IEnumerable<T> list, int count = 1)
        {
            if (list.Count() < count)
                return list;
            IEnumerable<T> items = list;
            if (count == 0 || list.Count() == 0)
                return list;
            return items.Shuffle().Take(count);
        }

        public static IEnumerable<HandGun> HandGuns(this IEnumerable<Equipment> list)
        {
            return (IEnumerable<HandGun>)list.Where(x => x.Slot == HG);
        }
        public static IEnumerable<LongGun> LongGuns(this IEnumerable<Equipment> list)
        {
            return (IEnumerable<LongGun>)list.Where(x => x.Slot == LG);
        }
        public static IEnumerable<Equipment> BySlot(this IEnumerable<Equipment> list, Equipment.SlotType slot)
        {
            var temp = list.Where(x => x is not null && x.Slot == slot).ToList();
            if (temp.Count() == 0)
                temp.Add(EquipmentDirectory.DefaultEquipment(slot));
            return temp;
        }

        public static IEnumerable<Mod> Mods(this IEnumerable<Equipment> list)
        {
            return (IEnumerable<Mod>)list.Where(x => x.Slot == MO);
        }

        public static Build RandomBuild(this IEnumerable<Equipment> inventory, Build except = null, List<Equipment> blacklist = null)
        {
            blacklist = blacklist ?? new List<Equipment>();
            except = except ?? new Build();
            blacklist.AddRange(except.Data);
            List<Equipment> list = inventory.ToList().AsReadOnly().Shuffle().Except(blacklist).ToList();
            HandGun hg = (HandGun)(list.BySlot(HG).RandomElement().First() ?? EquipmentDirectory.DefaultEquipment(HG));
            LongGun lg = (LongGun)(list.BySlot(LG).RandomElement().First() ?? EquipmentDirectory.DefaultEquipment(LG));
            var rings = list.BySlot(RI).RandomElement(2).ToList();
            while (rings.Count() < 2) { rings.Add(EquipmentDirectory.DefaultEquipment(RI)); }
            var mods = list.BySlot(MO).RandomElement(2).ToList();
            while (mods.Count() < 2) { mods.Add((Mod)EquipmentDirectory.DefaultEquipment(MO)); }

            mods = mods.Shuffle().ToList();
            hg.EquipMod((Mod)mods.First());
            lg.EquipMod((Mod)mods.Last());

            Build b = new("",
                hg, lg,
                list.BySlot(M).RandomElement().First(),
                list.BySlot(HE).RandomElement().First(),
                list.BySlot(CH).RandomElement().First(),
                list.BySlot(LE).RandomElement().First(),
                list.BySlot(AM).RandomElement().First(),
                rings.First(), rings.Last()
                );
            return b;

        }
    }

}
