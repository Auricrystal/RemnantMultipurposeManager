using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static RemnantBuildRandomizer.RemnantItem;
using static RemnantBuildRandomizer.GearInfo;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace RemnantBuildRandomizer
{
    public class RemnantItem
    {
        public enum SlotType { HG, LG, M, HE, CH, LE, AM, RI, MO };
        private SlotType slot;
        private string mod;
        private string dlc;
        private string path;
        private string itemname;
        private string description;
        private BitmapImage image;
        private bool disabled;
        static private int Index = 0;
        private int id;

        public RemnantItem(string path, string description, SlotType slot)
        {
            this.Path = path;
            this.Slot = slot;
            this.Mod = "";
            this.Itemname = path.Substring(path.LastIndexOf("/") + 1).Replace(".png", "");
            this.Image = createImage(path);
            this.Description = description;
            this.Disabled = false;
            switch (Itemname)
            {
                case "_No Hand Gun":
                case "_No Long Gun":
                case "_Fists":
                case "_No Head":
                case "_No Chest":
                case "_No Legs":
                case "_No Amulet":
                case "_No Mod":
                case "_No Ring": this.ID = 0; Index = 1; break;
                default: this.ID = Index++; break;
            }
            Debug.WriteLine(Itemname + ":" + this.ID);
        }

        public string Itemname { get => itemname; set => itemname = value; }
        public string Description { get => description; set => description = value; }
        public string Path { get => path; set => path = value; }
        public BitmapImage Image { get => image; set => image = value; }
        public SlotType Slot { get => slot; set => slot = value; }
        public string Dlc { get => dlc; set => dlc = value; }
        public string Mod { get => mod; set => mod = value; }
        public void setDis(bool b)
        {
            this.disabled = b;
            //Debug.WriteLine("CHANGING " + Itemname + " TO " + b);
        }
        public bool Disabled { get => disabled; set => setDis(value); }
        public int ID { get => id; set => id = value; }

        private BitmapImage createImage(string path)
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/IMG" + path, UriKind.RelativeOrAbsolute));
        }
    }
    public class Build: IEquatable<Build>
    {
        public RemnantItem hg;
        public RemnantItem hgm;
        public RemnantItem lg;
        public RemnantItem lgm;
        public RemnantItem m;
        public RemnantItem he;
        public RemnantItem ch;
        public RemnantItem le;
        public RemnantItem am;
        public RemnantItem r1;
        public RemnantItem r2;

        private string name;
        private string code;
        private bool disabled;

        public bool Disabled { get => disabled; set => disabled = value; }
        public string Code { get => code; set => code = value; }
        public string Name { get => name; set => name = value; }

        public Build(string name,RemnantItem hg, RemnantItem hgm, RemnantItem lg, RemnantItem lgm, RemnantItem m, RemnantItem he, RemnantItem ch, RemnantItem le, RemnantItem am, RemnantItem r1, RemnantItem r2)
        {
            this.hg = hg;
            this.hgm = hgm;
            this.lg = lg;
            this.lgm = lgm;
            this.m = m;
            this.he = he;
            this.ch = ch;
            this.le = le;
            this.am = am;
            this.r1 = r1;
            this.r2 = r2;
            this.Name = name;
            this.Disabled = false;
            Code = this.toCode();
        }

        public Build(string name,int hg, int hgm, int lg, int lgm, int m, int he, int ch, int le, int am, int r1, int r2)
        {
            this.hg =  GearList[SlotType.HG][hg];
            this.hgm = GearList[SlotType.MO][hgm];
            this.lg =  GearList[SlotType.LG][lg ];
            this.lgm = GearList[SlotType.MO][lgm];
            this.m =   GearList[SlotType.M][m ];
            this.he =  GearList[SlotType.HE][he ];
            this.ch =  GearList[SlotType.CH][ch ];
            this.le =  GearList[SlotType.LE][le ];
            this.am =  GearList[SlotType.AM][am ];
            this.r1 =  GearList[SlotType.RI][r1 ];
            this.r2 =  GearList[SlotType.RI][r2 ];
            this.Disabled = false;
            this.Name = name;
            Code = this.toCode();

        }



        public string toCode()
        {
            string code = "";
            //code += Name+":";
            code += hg.ID + "-";
            code += hgm.ID + "-";
            code += lg.ID + "-";
            code += lgm.ID + "-";
            code += m.ID + "-";
            code += he.ID + "-";
            code += ch.ID + "-";
            code += le.ID + "-";
            code += am.ID + "-";
            code += r1.ID + "-";
            code += r2.ID;

            return code;
        }

        public static Build fromCode(string name,string build)
        {
            string[] ids = build.Split('-');          
            return new Build(name,int.Parse(ids[0]), int.Parse(ids[1]), int.Parse(ids[2]), int.Parse(ids[3]), int.Parse(ids[4]), int.Parse(ids[5]), int.Parse(ids[6]), int.Parse(ids[7]), int.Parse(ids[8]), int.Parse(ids[9]), int.Parse(ids[10]));
        }

        public bool Equals(Build b) {
            if (this.Code.Equals(b.Code)) { return true; }
            return false;
        
        }

    }
   
}
