using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;

namespace RemnantBuildRandomizer
{
    public class RemnantCharacter
    {
        public string Archetype { get; set; }
        public List<string> Inventory { get; set; }


        public int Progression
        {
            get
            {
                return this.Inventory.Count;
            }
        }

        private List<Item> missingItems;

        private string savePath;


        public override string ToString()
        {
            return this.Archetype + " (" + this.Progression + ")";
        }

        public string ToFullString()
        {
            string str = "CharacterData{ Archetype: " + this.Archetype + ", Inventory: [" + string.Join(", ", this.Inventory) + "]}";
            return str;
        }

        public RemnantCharacter()
        {
            this.Archetype = "";
            this.Inventory = new List<string>();
            this.missingItems = new List<Item>();
            this.savePath = null;
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
            Debug.WriteLine("GET CHAR FROM SAVE!!!");
            return GetCharactersFromSave(saveFolderPath, CharacterProcessingMode.All);
        }

        public static List<RemnantCharacter> GetCharactersFromSave(string saveFolderPath, CharacterProcessingMode mode)
        {
            List<RemnantCharacter> charData = new List<RemnantCharacter>();
            try
            {
                string profileData = File.ReadAllText(saveFolderPath + "\\profile.sav");
                string[] characters = profileData.Split(new string[] { "/Game/Characters/Player/Base/Character_Master_Player.Character_Master_Player_C" }, StringSplitOptions.None);
                for (var i = 1; i < characters.Length; i++)
                {
                    RemnantCharacter cd = new RemnantCharacter();
                    //cd.Archetype = GameInfo.Archetypes["Undefined"];
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
                    cd.savePath = saveFolderPath;
                    List<string> saveItems = new List<string>();
                    string charEnd = "Character_Master_Player_C";
                    string inventory = characters[i].Substring(0, characters[i].IndexOf(charEnd));

                    Regex rx = new Regex(@"/Items/Weapons(/[a-zA-Z0-9_]+)+/[a-zA-Z0-9_]+");
                    MatchCollection matches = rx.Matches(inventory);
                    foreach (Match match in matches)
                    {
                        saveItems.Add(match.Value);
                    }

                    rx = new Regex(@"/Items/Armor/([a-zA-Z0-9_]+/)?[a-zA-Z0-9_]+");
                    matches = rx.Matches(inventory);
                    foreach (Match match in matches)
                    {
                        saveItems.Add(match.Value);
                    }

                    rx = new Regex(@"/Items/Trinkets/(BandsOfCastorAndPollux/)?[a-zA-Z0-9_]+");
                    matches = rx.Matches(inventory);
                    foreach (Match match in matches)
                    {
                        saveItems.Add(match.Value);
                    }

                    rx = new Regex(@"/Items/Mods/[a-zA-Z0-9_]+");
                    matches = rx.Matches(inventory);
                    foreach (Match match in matches)
                    {
                        saveItems.Add(match.Value);
                    }

                    rx = new Regex(@"/Items/Traits/[a-zA-Z0-9_]+");
                    matches = rx.Matches(inventory);
                    foreach (Match match in matches)
                    {
                        saveItems.Add(match.Value);
                    }

                    rx = new Regex(@"/Items/QuestItems(/[a-zA-Z0-9_]+)+/[a-zA-Z0-9_]+");
                    matches = rx.Matches(inventory);
                    foreach (Match match in matches)
                    {
                        saveItems.Add(match.Value);
                    }

                    rx = new Regex(@"/Quests/[a-zA-Z0-9_]+/[a-zA-Z0-9_]+");
                    matches = rx.Matches(inventory);
                    foreach (Match match in matches)
                    {
                        saveItems.Add(match.Value);
                    }

                    rx = new Regex(@"/Player/Emotes/Emote_[a-zA-Z0-9]+");
                    matches = rx.Matches(inventory);
                    foreach (Match match in matches)
                    {
                        saveItems.Add(match.Value);
                    }

                    cd.Inventory = saveItems;
                    charData.Add(cd);
                }

                if (mode == CharacterProcessingMode.All)
                {
                    string[] saves = Directory.GetFiles(saveFolderPath, "save_*.sav");
                    for (int i = 0; i < saves.Length && i < charData.Count; i++)
                    {
                        charData[i].processSaveData(File.ReadAllText(saves[i]));
                    }
                }
            }
            catch (IOException ex)
            {
                if (ex.Message.Contains("being used by another process"))
                {
                    Console.WriteLine("Save file in use; waiting 0.5 seconds and retrying.");
                    System.Threading.Thread.Sleep(500);
                    charData = GetCharactersFromSave(saveFolderPath, mode);
                }
            }
            return charData;
        }

        public List<Item> GetMissingItems()
        {
            return missingItems;
        }
    }
}

