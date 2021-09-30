using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;


namespace RemnantMultipurposeManager
{
    public class RemnantCharacter
    {
        public string Archetype { get; set; }
        public int Progression { get; set; }
        //public List<Build> Builds { get; set; }
        public List<InventoryItem> Inventory { get; set; }

        public override string ToString()
        {
            return Archetype + " (" + Progression + ")";
        }

        public RemnantCharacter()
        {
            this.Archetype = "";
            this.Inventory = new List<InventoryItem>();
            
        }

        public static List<RemnantCharacter> GetCharactersFromSave(string saveFolderPath)
        {
            List<RemnantCharacter> charData = new List<RemnantCharacter>();
            try
            {
                string profileData = File.ReadAllText(saveFolderPath);

                string[] characters = profileData.Split(new string[] { "/Game/Characters/Player/Base/Character_Master_Player.Character_Master_Player_C" }, StringSplitOptions.None);
                for (var i = 1; i < characters.Length; i++)
                {
                    RemnantCharacter cd = new RemnantCharacter();

                    Match archetypeMatch = new Regex(@"/Game/_Core/Archetypes/[a-zA-Z_]+").Match(characters[i - 1]);
                    cd.Archetype = archetypeMatch.Success ? archetypeMatch.Value.Replace("/Game/_Core/Archetypes/", "").Split('_')[1] : "Undefined";

                    List<string> saveItems = new List<string>();
                    string charEnd = "Character_Master_Player_C";
                    string inventory = characters[i].Substring(0, characters[i].IndexOf(charEnd));

                    FindMatches(saveItems, inventory,
                      new Regex(@"/Items/Weapons(/[a-zA-Z0-9_]+)+/[a-zA-Z0-9_]+")
                    , new Regex(@"/Items/Armor/([a-zA-Z0-9_]+/)?[a-zA-Z0-9_]+")
                    , new Regex(@"/Items/Trinkets/(BandsOfCastorAndPollux/)?[a-zA-Z0-9_]+")
                    , new Regex(@"/Items/Mods/[a-zA-Z0-9_]+")
                    , new Regex(@"/Items/Traits/[a-zA-Z0-9_]+")
                    , new Regex(@"/Items/QuestItems(/[a-zA-Z0-9_]+)+/[a-zA-Z0-9_]+")
                    , new Regex(@"/Quests/[a-zA-Z0-9_]+/[a-zA-Z0-9_]+")
                    , new Regex(@"/Player/Emotes/Emote_[a-zA-Z0-9]+"));

                    cd.Progression = saveItems.Count;
                    cd.Inventory = new List<InventoryItem>();
                    foreach (InventoryItem item in GearInfo.Items)
                    {
                        if (saveItems.Contains(item.File) ||
                            GearInfo.Items.Where(x => x.Name.Contains("_"))
                            .Select(x => x.Name)
                            .Contains(item.Name))
                        {
                            cd.Inventory.Add(item);
                        }
                    }
                    cd.Inventory.Sort();
                    charData.Add(cd);
                }
            }
            catch (IOException ex)
            {
                if (ex.Message.Contains("being used by another process"))
                {
                    Console.WriteLine("Save file in use; waiting 0.5 seconds and retrying.");
                    System.Threading.Thread.Sleep(500);
                    charData = GetCharactersFromSave(saveFolderPath);
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

    }
}

