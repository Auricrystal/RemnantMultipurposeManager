using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemnantBuildRandomizer
{
    class RemnantProfile
    {
        
        string foldername;



        public string FolderName
        {
            get => foldername; 
            set
            { 
                if (Directory.Exists(BackupProfileFolder + "\\" + foldername)) {
                    
                } 
            }
        }

       
        public string BackupProfileFolder
        {
            get
            {
                string folder = MainWindow.RBRDirPath + "\\Profiles";
                Directory.CreateDirectory(folder);
                return folder;
            }
        }



        List<RemnantCharacter> chars;

        public RemnantProfile(string path, string filename)
        {

            this.FolderName = filename;
            chars = RemnantCharacter.GetCharactersFromSave(path, filename);

        }


    }
}
