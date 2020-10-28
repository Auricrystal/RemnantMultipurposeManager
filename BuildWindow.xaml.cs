using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RemnantBuildRandomizer
{
    /// <summary>
    /// Interaction logic for BuildWindow.xaml
    /// </summary>
    public partial class BuildWindow : Window
    {
        public BuildWindow(Build b)
        {
            InitializeComponent();
           // Width = 0;
            //Height = 0;
            
            ShowInTaskbar = false;
            ShowActivated = false;
            Visibility = Visibility.Hidden;
            DisplayBuild(b);
            this.Show();
            this.Hide();
            GrabScreenshot();


        }
        private void DisplayBuild(Build b)
        {
            BuildName.Text = b.BuildName;
            BuildNum.Text = b.toStringCode();

            setImage(HandGunImg, b.HandGun);
            setImage(HandModImg, b.HandMod);
            setImage(LongGunImg, b.LongGun);
            setImage(LongModImg, b.LongMod);
            setImage(MeleeImg, b.Melee);
            setImage(HeadImg, b.Head);
            setImage(ChestImg, b.Chest);
            setImage(LegImg, b.Legs);
            setImage(AmImg, b.Amulet);
            setImage(Ri1Img, b.Ring1);
            setImage(Ri2Img, b.Ring2);
        }

        private void setImage(Image i, RemnantItem ri)
        {
            i.Source = ri.Data.GetImage();
        }


        public void GrabScreenshot()
        {
            CopyUIElementToClipboard(FullBuildScreen);
            this.Close();
            MessageBox.Show("Copied to Clipboard!");
        }
        public static void CopyUIElementToClipboard(FrameworkElement element)
        {


            double width = element.ActualWidth;
            double height = element.ActualHeight;
            Debug.WriteLine("Window size: " + width + " " + height + " " + element.IsLoaded);
            RenderTargetBitmap bmpCopied = new RenderTargetBitmap((int)Math.Round(width), (int)Math.Round(height), 96, 96, PixelFormats.Default);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(element);
                dc.DrawRectangle(vb, null, new Rect(new Point(), new Size(width, height)));
            }
            bmpCopied.Render(dv);
            Clipboard.SetImage(bmpCopied);
        }
    }
}
