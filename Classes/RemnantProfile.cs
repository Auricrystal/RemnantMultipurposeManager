using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace RemnantMultipurposeManager
{
    public class RemnantProfile
    {
        public List<RemnantCharacter> Characters { get; }
        public RemnantProfile(string saveProfilePath)
        {
            try
            {
                Debug.WriteLine(saveProfilePath+":"+File.Exists(saveProfilePath));
                Characters = RemnantCharacter.GetCharactersFromSave(saveProfilePath);
            }
            catch (Exception)
            {
                MainWindow.MW.LogMessage("Error Parsing Profile", MainWindow.LogType.Error);
                Characters = null;
            }
        }
    }
}

