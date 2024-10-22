﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RemnantBuildRandomizer
{
    public class RemnantCharacter
    {
        public int charNum;
        public string Archetype { get; set; }
        public List<string> Inventory { get; set; }

        private List<Item> missingItems;


        public int Progression
        {
            get
            {
                return this.Inventory.Count;
            }
        }



        public override string ToString()
        {
            return this.Archetype + " (" + this.Progression + ")";
        }

        public string ToFullString()
        {
            string str = "CharacterData{ Archetype: " + this.Archetype + ", Inventory: [" + string.Join(", ", this.Inventory) + "]}";
            return str;
        }

        public RemnantCharacter(int charNum)
        {
            this.charNum = charNum;
            this.Archetype = "";
            this.Inventory = new List<string>();
            this.missingItems = new List<Item>();
        }

        public void processSaveData(string savetext)
        {
            missingItems.Clear();

            foreach (Item item in GearInfo.Items)
            {
                if (!this.Inventory.Contains(item.GetKey()))
                {
                    if (!missingItems.Contains(item))
                    {
                        missingItems.Add(item);
                    }
                }
            }
            missingItems.Sort();
        }

        public enum CharacterProcessingMode { All, NoEvents };

        public static List<RemnantCharacter> GetCharactersFromSave(string saveFolderPath)
        {

            return GetCharactersFromSave(saveFolderPath, CharacterProcessingMode.All);
        }
        public static List<RemnantCharacter> GetCharactersFromSave(string saveFolderPath, string filename)
        {

            return GetCharactersFromSave(saveFolderPath, filename, CharacterProcessingMode.All);
        }
        public static List<RemnantCharacter> GetCharactersFromSave(string saveFolderPath, CharacterProcessingMode mode)
        {

            return GetCharactersFromSave(saveFolderPath, "\\profile.sav", mode);
        }

        public static List<RemnantCharacter> GetCharactersFromSave(string saveFolderPath, string filename, CharacterProcessingMode mode)
        {
            List<RemnantCharacter> charData = new List<RemnantCharacter>();
            try
            {
                string profileData = File.ReadAllText(saveFolderPath + filename);

                string[] characters = profileData.Split(new string[] { "/Game/Characters/Player/Base/Character_Master_Player.Character_Master_Player_C" }, StringSplitOptions.None);
                for (var i = 1; i < characters.Length; i++)
                {
                    RemnantCharacter cd = new RemnantCharacter(i - 1);

                    cd.Archetype = GearInfo.Archetypes["Undefined"];
                    Match archetypeMatch = new Regex(@"/Game/_Core/Archetypes/[a-zA-Z_]+").Match(characters[i - 1]);
                    if (archetypeMatch.Success)
                    {
                        string archetype = archetypeMatch.Value.Replace("/Game/_Core/Archetypes/", "").Split('_')[1];
                        if (GearInfo.Archetypes.ContainsKey(archetype))
                        {
                            cd.Archetype = GearInfo.Archetypes[archetype];
                        }
                        else
                        {
                            cd.Archetype = archetype;
                        }
                    }

                    List<string> saveItems = new List<string>();
                    string charEnd = "Character_Master_Player_C";
                    string inventory = characters[i].Substring(0, characters[i].IndexOf(charEnd));

                    FindMatches(saveItems, inventory, new Regex(@"/Items/Weapons(/[a-zA-Z0-9_]+)+/[a-zA-Z0-9_]+")
                    , new Regex(@"/Items/Armor/([a-zA-Z0-9_]+/)?[a-zA-Z0-9_]+")
                    , new Regex(@"/Items/Trinkets/(BandsOfCastorAndPollux/)?[a-zA-Z0-9_]+")
                    , new Regex(@"/Items/Mods/[a-zA-Z0-9_]+")
                    , new Regex(@"/Items/Traits/[a-zA-Z0-9_]+")
                    , new Regex(@"/Items/QuestItems(/[a-zA-Z0-9_]+)+/[a-zA-Z0-9_]+")
                    , new Regex(@"/Quests/[a-zA-Z0-9_]+/[a-zA-Z0-9_]+")
                    , new Regex(@"/Player/Emotes/Emote_[a-zA-Z0-9]+"));

                    cd.Inventory = saveItems;
                    charData.Add(cd);
                }

                if (mode == CharacterProcessingMode.All)
                {
                    string[] saves = Directory.GetFiles(saveFolderPath, "save_*.sav");
                    for (int i = 0; i < saves.Length && i < charData.Count; i++)
                    {
                        charData[i].processSaveData(File.ReadAllText(saves[i]));
                        Debug.WriteLine(charData[i].missingItems.Count);
                    }

                }
            }
            catch (IOException ex)
            {
                if (ex.Message.Contains("being used by another process"))
                {
                    Console.WriteLine("Save file in use; waiting 0.5 seconds and retrying.");

                    System.Threading.Thread.Sleep(500);
                    charData = GetCharactersFromSave(saveFolderPath, filename, mode);
                }
            }
            return charData;
        }

        private static void FindMatches(List<string> saveItems, string inventory, params Regex[] rxs)
        {
            foreach (Regex rx in rxs)
            {
                FindMatches(saveItems, inventory, rx);
            }
        }
        private static void FindMatches(List<string> saveItems, string inventory, Regex rx)
        {
            foreach (Match match in rx.Matches(inventory)) { saveItems.Add(match.Value); }
        }

        public List<Item> GetMissingItems()
        {
            if (missingItems.Count == 0 && Progression < 382)
            {
                Debug.WriteLine("Something isnt right, fixing missing data");
                string[] saves = Directory.GetFiles(MainWindow.SaveDirPath, "save_*.sav");
                for (int i = 0; i < saves.Length; i++)
                {
                    this.processSaveData(File.ReadAllText(saves[i]));
                }
            }
            return missingItems;
        }
    }
}

