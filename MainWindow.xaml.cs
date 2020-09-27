using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RemnantBuildRandomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapImage[] images;
        Random rd = new Random();
        public MainWindow()
        {
            InitializeComponent();
            images = createImageList(@"/Armor/Head/Adventurer Goggles.png", @"/Armor/Head/Akari Mask.png", @"/Armor/Head/Cultist Hat.png");

        }



        private void ReRollImg(object sender, RoutedEventArgs e)
        {
            //var assembly = Assembly.GetExecutingAssembly();
            //string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("T_Icon_RedCrystal.png"));
           
            //Debug.WriteLine("local:"+resourceName);
            
           
            Test.Source = images[rd.Next(0,images.Length)];

        }
        private BitmapImage[] createImageList(params string[] items) {
            int index = 0;
            BitmapImage[] arr = new BitmapImage[items.Length];
            foreach (string s in items) {
                Uri uri = new Uri("pack://application:,,,/Resources/IMG" + s,UriKind.RelativeOrAbsolute);
                Debug.WriteLine(uri);
                //string path = HttpContext.Current.Server.URLDecode(uri.AbsolutePath);
                BitmapImage b= new BitmapImage(uri);
                arr[index++] = b;
            }
            return arr;
        }
    }
}
