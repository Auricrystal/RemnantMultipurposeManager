﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace RemnantBuildRandomizer
{
    public class RemnantSave
    {
        private string profilePath;
        private List<RemnantCharacter> saveCharacters;

        public RemnantSave(string saveProfilePath)
        {
            if (!saveProfilePath.EndsWith("profile.sav"))
            {
                if (File.Exists(saveProfilePath + "\\profile.sav"))
                {
                    saveProfilePath += "\\profile.sav";
                }
                else
                {
                    throw new Exception(saveProfilePath + " is not a valid save.");
                }
            }
            else if (!File.Exists(saveProfilePath))
            {
                throw new Exception(saveProfilePath + " does not exist.");
            }
            this.profilePath = saveProfilePath;
            if (saveCharacters == null)
            {
                saveCharacters = RemnantCharacter.GetCharactersFromSave(this.SaveFolderPath);
            }
        }

        public string SaveFolderPath
        {
            get
            {
                return this.profilePath.Replace("\\profile.sav", "");
            }
        }

        public List<RemnantCharacter> Characters
        {
            get
            {
                return saveCharacters;
            }
        }

        public static Boolean ValidSaveFolder(String folder)
        {
            if (!File.Exists(folder + "\\profile.sav"))
            {
                return false;
            }
            return true;
        }

        public void UpdateCharacters()
        {
            var test = RemnantCharacter.GetCharactersFromSave(this.SaveFolderPath);

            if (saveCharacters != null && test.Count == saveCharacters.Count)
            {
                for (int i = 0; i < test.Count; i++)
                {
                    if (test[i].Progression > saveCharacters[i].Progression) { NewData(test); break; }
                }
                Debug.WriteLine("Data Not Updated");
            }
            else { NewData(test); }

        }
        private void NewData(List<RemnantCharacter> test)
        {
            Debug.WriteLine("New Char data");
            saveCharacters = test;
            MainWindow.SlogMessage("Updating Character Info:");
            foreach (RemnantCharacter rc in this.Characters)
            {
                MainWindow.SlogMessage(rc + " Has " + rc.GetMissingItems().Count + " Missing Items");
            }
        }
    }
}

