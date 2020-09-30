using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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

        public RemnantItem(string path, string description, SlotType slot)
        {
            this.Path = path;
            this.Slot = slot;
            this.Mod = "";
            this.Itemname = path.Substring(path.LastIndexOf("/")+1).Replace(".png", "");
            this.Image = createImage(path);
            this.Description = description;
            this.Disabled = false;
        }

        public string Itemname { get => itemname; set => itemname = value; }
        public string Description { get => description; set => description = value; }
        public string Path { get => path; set => path = value; }
        public BitmapImage Image { get => image; set => image = value; }
        public SlotType Slot { get => slot; set => slot = value; }
        public string Dlc { get => dlc; set => dlc = value; }
        public string Mod { get => mod; set => mod = value; }
        public void setDis(bool b) { 
            this.disabled = b;
            Debug.WriteLine("CHANGING "+Itemname+" TO "+b);
        }
        public bool Disabled { get => disabled; set => setDis(value); }

        private BitmapImage createImage(string path)
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/IMG" + path, UriKind.RelativeOrAbsolute));
        }
    }
}
