using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RemnantBuildRandomizer
{
    public class InventoryItem : IEquatable<InventoryItem>, IComparable, ICloneable
    {
        public enum SlotType { HG, LG, M, HE, CH, LE, AM, RI, MO };
        public SlotType Slot { get; set; }
        public string Name { get; set; }
        public string IMG { get; set; }
        public string File { get; set; }
        public InventoryItem Mod { get; set; }

        public bool Equals(InventoryItem b)
        {
            if (this.Name.Equals(b.Name)) { return true; }
            return false;
        }


        public int CompareTo(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null))
            {
                return 1;
            }
            else if (!this.GetType().Equals(obj.GetType()))
            {
                if (obj.GetType() == typeof(string))
                {
                    return Name.CompareTo(obj);
                }
                return ToString().CompareTo(obj.ToString());
            }
            else
            {
                InventoryItem rItem = (InventoryItem)obj;
                return this.Name.CompareTo(rItem.Name);
            }
        }

        public object Clone()
        {
            return new InventoryItem { File = this.File, IMG = this.IMG, Name = this.Name, Mod = this.Mod, Slot = this.Slot };
        }
    }


    public class Build
    {

        public InventoryItem HandGun, LongGun, Melee, Head, Chest, Legs, Amulet, Ring1, Ring2;

        public Build(InventoryItem handGun, InventoryItem longGun, InventoryItem melee, InventoryItem head, InventoryItem chest, InventoryItem legs, InventoryItem amulet, InventoryItem ring1, InventoryItem ring2)
        {
            HandGun = handGun;
            LongGun = longGun;
            Melee = melee;
            Head = head;
            Chest = chest;
            Legs = legs;
            Amulet = amulet;
            Ring1 = ring1;
            Ring2 = ring2;
        }

        public static Build Random(List<InventoryItem> inventory)
        {
            List<InventoryItem> local = inventory.ConvertAll(x => (InventoryItem)x.Clone());

            InventoryItem hg = local.Where(x => x.Slot == InventoryItem.SlotType.HG).ToList().RandomElement();
            InventoryItem lg = local.Where(x => x.Slot == InventoryItem.SlotType.LG).ToList().RandomElement();
            List<InventoryItem> mods = local.Where(x => x.Slot == InventoryItem.SlotType.MO)
                .Except(local.Where(x => x.Mod != null).Select(x => x.Mod)).ToList();
            if (hg.Mod == null) { hg.Mod = mods.RandomElement(); }
            if (lg.Mod == null) { lg.Mod = mods.Where(x => x.Mod != hg.Mod).ToList().RandomElement(); }
            List<InventoryItem> rings = local.Where(x => x.Slot == InventoryItem.SlotType.RI).ToList();
            InventoryItem ring1 = rings.RandomElement();
            Build b = new Build(
                hg,
                lg,
                local.Where(x => x.Slot == InventoryItem.SlotType.M).ToList().RandomElement(),
                local.Where(x => x.Slot == InventoryItem.SlotType.HE).ToList().RandomElement(),
                local.Where(x => x.Slot == InventoryItem.SlotType.CH).ToList().RandomElement(),
                local.Where(x => x.Slot == InventoryItem.SlotType.LE).ToList().RandomElement(),
                local.Where(x => x.Slot == InventoryItem.SlotType.AM).ToList().RandomElement(),
                ring1,
                rings.Where(x => x != ring1).ToList().RandomElement()
                );

            return b;

        }
    }
}
