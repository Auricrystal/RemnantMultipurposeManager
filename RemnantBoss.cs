using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents.DocumentStructures;

namespace RemnantBuildRandomizer
{
    class RemnantBoss
    {
        public enum Boss
        {
            //Earth
            Ent, Singe, Gorefist, Shroud, Riphide, Mangler,
            //Rhom
            Claviger, Harrow, UndyingKing, Scourge, ShatterShade, Raze,
            //Corsus
            Ixillis, UncleanOne, IskalQueen, Thrall, Dreameater, Canker, BarbedTerror,
            //Yaesha
            TotemFather, Raviger, Stormcaller, ScaldSear, TheWarden, Onslaught,
            //Reisum
            BrudvaakVargr, Harsgaard, Tian, Obryk, Ikro, Erfor
        }
        public enum Modifier { None, Vicious,Regenerator,Skullcracker,Hearty,Enchanter }
        public enum Difficulty { Normal, Hard, Nightmare, Apocalypse }
        private Boss name;
        private Modifier m;
        private Difficulty d;

        internal Boss Name { get => name; set => name = value; }

        public RemnantBoss(Boss name, Modifier m, Difficulty d)
        {

            this.Name = name;
            this.m = m;
            this.d = d;


        }

    }
}
