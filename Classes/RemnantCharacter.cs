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
        private static int num = 0;
        public int Slot { get; set; }
        public string Archetype { get; set; }
        public int Progression { get; set; }
        public List<string> Inventory { get; set; }

        public override string ToString()
        {
            return Archetype + " (" + Progression + ")";
        }

        public RemnantCharacter(int? Slot = null, string Archetype = null, int? Progression = null, List<string> Inventory = null)
        {
            this.Slot = Slot ?? num++;
            this.Archetype = Archetype ?? "None";
            this.Progression = Progression ?? 0;
            this.Inventory = Inventory ?? new List<string>();

        }


        public static List<RemnantCharacter> GenerateCharacters(string saveFolderPath)
        {
            List<RemnantCharacter> charData = new List<RemnantCharacter>();
            try
            {
                string profileData = File.ReadAllText(saveFolderPath);

                string[] characters = profileData.Split(new string[] { "/Game/Characters/Player/Base/Character_Master_Player.Character_Master_Player_C" }, StringSplitOptions.None);
                //string[] test = profileData.Split(new string[] { "/Game/_Core/Archetypes/Archetype_Cultist_UI.Archetype_Cultist_UI_C" }, StringSplitOptions.None);


                //for (int i = 0; i < test.Length; i++)
                //{
                //    File.WriteAllText(@"C:\Users\AuriCrystal\AppData\Local\Remnant\Saved\SaveGames\Character" + i + ".sav", test[i]);
                //}
                for (var i = 1; i < characters.Length; i++)
                {
                    RemnantCharacter cd = new RemnantCharacter(Slot: i - 1);

                    Match archetypeMatch = new Regex(@"/Game/_Core/Archetypes/[a-zA-Z_]+").Match(characters[i - 1]);
                    cd.Archetype = archetypeMatch.Success ? archetypeMatch.Value.Replace("/Game/_Core/Archetypes/", "").Split('_')[1] : "Undefined";

                    List<string> saveItems = new List<string>();
                    string charEnd = "Character_Master_Player_C";

                    FindMatches(saveItems,
                        characters[i].Substring(0, characters[i].IndexOf(charEnd)),
                        new Regex(@"/(Items/(?:Weapons(?:/[\w]+)+|Armor(?:/[\w]+)?|Trinkets(?:/BandsOfCastorAndPollux)?|Mods|Traits|QuestItems(?:/[\w]+)+)/[\w]+|Quests/[\w]+/[\w]+|Player/Emotes/Emote_[\w]+)"));
                    cd.Progression = saveItems.Count;
                    cd.Inventory = new List<string>();

                    foreach (Equipment item in EquipmentDirectory.Items)
                        if (saveItems.Contains(item.File))
                            if (EquipmentDirectory.Items.Where(x => !x.Name.Contains("_")).Select(x => x.Name).Contains(item.Name))
                                cd.Inventory.Add(item.Name);

                    cd.Inventory.Sort();
                    charData.Add(cd);
                }
            }
            catch (IOException ex)
            {
                if (ex.Message.Contains("being used by another process"))
                {
                    Console.WriteLine("WorldSave file in use; waiting 0.5 seconds and retrying.");
                    System.Threading.Thread.Sleep(500);
                    charData = GenerateCharacters(saveFolderPath);
                }
            }
            return charData;
        }

        private static void FindMatches(List<string> saveItems, string inventory, Regex rx)
        {
            foreach (Match match in rx.Matches(inventory))
            {
                saveItems.Add(match.Value);
            }
        }

    }
}

