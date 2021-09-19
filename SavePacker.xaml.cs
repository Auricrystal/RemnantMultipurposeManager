using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RemnantBuildRandomizer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SavePacker : Window
    {
        private List<Data> saves;
        private Data sel;
        public SavePacker()
        {
            InitializeComponent();
            saves = new List<Data>();
            dgSaves.ItemsSource = saves;
            dgSaves.Items.Refresh();
        }

        private void GenerateRBB_Click(object sender, RoutedEventArgs e)
        {
            JWorldSave obj = new JWorldSave()
            { Type = SaveType.Text, World = World.Text, Name = Name.Text, Difficulty = DifficultyType.Text };
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "RMM file|*.rmm";
            sfd.Title = "Save an RMM file";
            sfd.ShowDialog();
            obj.File = saves.ToDictionary(x => x.Name, x => File.ReadAllBytes(x.File));
            if (sfd.FileName != "")
            {
                File.WriteAllText(sfd.FileName, JsonConvert.SerializeObject(obj, Formatting.Indented));
                this.Close();
            }

        }
        protected struct Data
        {
            public string Name { get; set; }
            public string File { get; set; }
        }
        private void AddFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "(.sav) Files|*.sav";
            fd.Multiselect = true;
            fd.Title = "Add File";
            fd.ShowDialog();
            foreach (string s in fd.FileNames)
            {
                saves.Add(new Data { Name = "Edit Me!", File = s });
            }
            dgSaves.Items.Refresh();
        }

        private void dgSaves_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSaves.SelectedIndex > -1) { sel = (Data)dgSaves.SelectedItem; }
        }
        bool editting = false;
        private void dgSaves_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
        }

        private void dgSaves_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            editting = true;
        }

        private void dgSaves_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            var dg = (System.Windows.Controls.DataGrid)sender;
            if (editting)
            {
                editting = false;
                dg.CommitEdit();
            }
            var data = ((Data)e.Row.Item);
            Debug.WriteLine(data.Name);
            int index = saves.FindIndex(x => x.File == data.File);
            saves[index] = data;
        }
    }
}
