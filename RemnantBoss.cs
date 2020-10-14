using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents.DocumentStructures;
using static RemnantBuildRandomizer.RemnantBoss.Boss;
namespace RemnantBuildRandomizer
{
    public class RemnantBoss
    {
        public enum Boss
        {
            //Earth
            Ent, Singe, Brabus, Gorefist, Shroud, Riphide, Mangler, Dreamer,
            //Rhom
            Claviger, Harrow, UndyingKing, Scourge, ShatterShade, Raze, AncientConstruct, Maul,
            //Corsus
            Ixillis, UncleanOne, IskalQueen, Thrall, Dreameater, Canker, BarbedTerror,
            //Yaesha
            TotemFather, Ravager, Stormcaller, ScaldSear, TheWarden, Onslaught,
            //Reisum
            BrudvaakVargr, Harsgaard, Tian, Obryk, Ikro, Erfor
        }
        public enum WorldZone { Earth, Rhom, Corsus, Yaesha, Reisum }
        public enum Mod { None, Vicious, Regenerator, Skullcracker, Hearty, Enchanter, World }
        public enum Diff { Normal, Hard, Nightmare, Apocalypse }
        private Boss name;
        private Mod m1;
        private Mod? m2;
        private Diff d;
        private WorldZone w;
        public Diff Difficulty { get => d; set => d = value; }
        public WorldZone World { get => w; set => w = value; }
        public Boss Name { get => name; set => name = value; }
        public Mod Modifier1 { get => m1; set => m1 = value; }
        public Mod? Modifier2 { get => m2; set => m2 = value; }

        private static int Eval<T>(T t)
        {
            return Convert.ToInt32(t);
        }
        private static WorldZone getWorld(Boss b)
        {
            if (Eval(b) >= Eval(BrudvaakVargr))
            {
                return WorldZone.Reisum;
            }
            else if (Eval(b) >= Eval(TotemFather))
            {
                return WorldZone.Yaesha;
            }
            else if (Eval(b) >= Eval(Ixillis))
            {
                return WorldZone.Corsus;
            }
            else if (Eval(b) >= Eval(Claviger))
            {
                return WorldZone.Rhom;
            }
            else { return WorldZone.Earth; }
        }

        public RemnantBoss(WorldZone w, Boss name, Diff d, Mod m1)
        {
            this.Name = name;
            this.Modifier1 = m1;
            this.Modifier2 = null;
            this.Difficulty = d;
            this.World = w;
        }
        public RemnantBoss(WorldZone w, Boss name, Diff d, Mod m1, Mod m2)
        {
            this.Name = name;
            this.Modifier1 = m1;
            this.Modifier2 = m2;
            this.Difficulty = d;
            this.World = w;
        }
        
        public static RemnantBoss FromFilename(string s)
        {
            s = s.Replace(".sav", "");
            string[] data = s.Split('_');

            
            Boss b = (Boss)Enum.Parse(typeof(Boss), data[0]);

            Diff d = (Diff)Enum.Parse(typeof(Diff), data[1]);

            Mod m= (Mod)Enum.Parse(typeof(Mod), data[2]);

            if (data.Length > 3)
            {
                Mod n = (Mod)Enum.Parse(typeof(Mod), data[3]);
                return new RemnantBoss(getWorld(b), b, d, m, n);

            }
            else {
                return new RemnantBoss(getWorld(b), b, d, m);
            }

        }

        public override string ToString()
        {
            string mods;
            if (Modifier2!=null) { mods = Modifier1 + "_" + Modifier2; } else { mods = Modifier1 + ""; }

            return Name + "_" + Difficulty + "_" + mods;
        }



    }
}
