using System.IO.Compression;
using System.IO;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using System.Windows.Forms;
using System;

namespace RemnantMultipurposeManager
{
    
    public class Equipment : ICloneable
    {
        public enum SlotType { HG, LG, M, HE, CH, LE, AM, RI, MO };
        public SlotType Slot { get; private set; }
        public string Name { get; private set; }
        public string IMG { get; private set; }
        public string File { get; private set; }

        public Equipment(SlotType slot, string name, string file, string img)
        {
            Slot = slot;
            Name = name;
            File = file;
            IMG = img;
        }
        [JsonIgnore]
        public BitmapImage GetImage
        {
            get
            {
                BitmapImage bmp = null;

                if (!System.IO.File.Exists(MainWindow.RmmInstallPath + "\\IMG.zip"))
                    if (!MainWindow.DownloadZip("IMG"))
                        return new BitmapImage();

                var zip = ZipFile.OpenRead(MainWindow.RmmInstallPath + "\\IMG.zip");


                var entry = IMG != null ? zip.GetEntry(IMG) : null;
                if (entry != null)
                {
                    using (var zipStream = entry.Open())
                    using (var memoryStream = new MemoryStream())
                    {
                        zipStream.CopyTo(memoryStream);
                        memoryStream.Position = 0;

                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = memoryStream;
                        bitmap.EndInit();

                        bmp = bitmap;
                    }
                }
                return bmp;
            }
        }

        public override string ToString()
        {
            return string.Format("Slot: {0} Name: {1}", Slot, Name);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
 
    public class Gun : Equipment
    {
        public Mod Mod { get; private set; }
        public Gun(SlotType slot, string name, string file, string img, Mod mod = null) : base(slot, name, file, img)
        {
            Mod = mod;
        }

        public void EquipMod(Mod mod)
        {
            if (Mod is not null && Mod.Boss && !Mod.Name.Contains("_"))
                return;
            if (Name.Contains("_"))
                return;
            Mod = mod;
        }

        public override string ToString()
        {
            return base.ToString() + ((Mod == null) ? " No Mod" : " "+Mod.ToString());
        }
        
    }
    public class Mod : Equipment
    {
        public bool Boss { get; private set; }
        public Mod(string name, string file, string img, bool boss = false) : base(SlotType.MO, name, file, img)
        {
            Boss = boss;
        }

        public override string ToString()
        {
            return base.ToString()+" Boss: "+Boss;
        }
    }
    public class HandGun : Gun
    {
        public HandGun(string name, string file, string img, Mod mod = null) : base(SlotType.HG, name, file, img, mod)
        {
        }
    }
    public class LongGun : Gun
    {
        public LongGun(string name, string file, string img, Mod mod = null) : base(SlotType.LG, name, file, img, mod)
        {
        }
    }
}
