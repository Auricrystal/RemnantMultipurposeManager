using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using static RemnantMultipurposeManager.Equipment;

namespace RemnantMultipurposeManager
{
    public class Build
    {
        public string Name { get; set; }
        public bool Active { get; set; }

        public string HandGun { get; set; }
        public string HandGunMod { get; set; }

        public string LongGun { get; set; }
        public string LongGunMod { get; set; }

        public string Melee { get; set; }

        public string Head { get; set; }
        public string Chest { get; set; }
        public string Legs { get; set; }

        public string Amulet { get; set; }
        public string RingLeft { get; set; }
        public string RingRight { get; set; }

        public Build(string name = "", string handGun = "", string handGunMod = "", string longGun = "", string longGunMod = "", string melee = "", string head = "", string chest = "", string legs = "", string amulet = "", string ringLeft = "", string ringRight = "", bool active = true)
        {
            Name = name;
            HandGun = handGun;
            HandGunMod = handGunMod;
            LongGun = longGun;
            LongGunMod = longGunMod;
            Melee = melee;
            Head = head;
            Chest = chest;
            Legs = legs;
            Amulet = amulet;
            RingLeft = ringLeft;
            RingRight = ringRight;
            Active = active;
        }

        public List<Equipment> GetItems()
        {
            return new List<string>() { HandGun, HandGunMod, LongGun, LongGunMod, Melee, Head, Chest, Legs, Amulet, RingLeft, RingRight }.Where(x => x != "").Select(x => EquipmentDirectory.FindEquipmentByName(x)).Where(x=>x is not null).ToList();
        }

    }
}
