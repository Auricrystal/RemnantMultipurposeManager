using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static RemnantMultipurposeManager.Equipment;

namespace RemnantMultipurposeManager
{
    public class Build
    {
        public string Name { get; set; }

        public List<Equipment> Data { get; set; }

        [JsonConstructor]
        public Build(string name = null, List<Equipment> items = null)
        {
            this.Name = name ?? "";
            this.Data = items ?? new List<Equipment>();
        }
        public Build(string name, params Equipment[] items)
        {
            this.Data = new List<Equipment>();
            this.Name = name;
            foreach (Equipment t in items)
            {
                if (t is Mod)
                {
                    Debug.WriteLine("Mods dont make sense by themselves!");
                    continue;
                }

                if (t.Slot == SlotType.RI && Data.Where(x => x.Slot == SlotType.RI).Count() > 2)
                {


                    Debug.WriteLine("Too Many Rings!");
                    continue;
                }
               

                if (Data.Where(x => x.Slot == t.Slot).Count() > 0 &&t.Slot!=SlotType.RI)
                {
                    Debug.WriteLine("Cant Equip more than one of "+t.Slot+"!");
                    continue;
                }
                Data.Add(t);
            }
        }
        public override string ToString()
        {
            return string.Join(", ",Data);
        }
    }
}
