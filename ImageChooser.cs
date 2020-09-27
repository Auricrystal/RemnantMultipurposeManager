using System.Collections;
using System.Collections.Generic;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace RemnantBuildRandomizer
{
    class ImageChooser
    {

        private void Awake()
        {

        }
        public void Reroll()
        {


        }

        /*
        public static void RefreshGameInfo()
        {
            zones.Clear();
            events.Clear();
            eventItem.Clear();
            subLocations.Clear();
            mainLocations.Clear();
            archetypes.Clear();
            string eventName = null;
            string altEventName = null;
            string itemMode = null;
            string itemNotes = null;
            string itemAltName = null;
            List<RemnantItem> eventItems = new List<RemnantItem>();
            XmlTextReader reader = new XmlTextReader("GameInfo.xml");
            reader.WhitespaceHandling = WhitespaceHandling.None;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name.Equals("Event"))
                        {
                            eventName = reader.GetAttribute("name");
                            altEventName = reader.GetAttribute("altname");
                            if (altEventName == null)
                            {
                                altEventName = eventName;
                            }
                            events.Add(eventName, altEventName);
                        }
                        else if (reader.Name.Equals("Item"))
                        {
                            itemMode = reader.GetAttribute("mode");
                            itemNotes = reader.GetAttribute("notes");
                            itemAltName = reader.GetAttribute("altname");
                        }
                        else if (reader.Name.Equals("Zone"))
                        {
                            zones.Add(reader.GetAttribute("key"), reader.GetAttribute("name"));
                        }
                        else if (reader.Name.Equals("SubLocation"))
                        {
                            subLocations.Add(reader.GetAttribute("eventName"), reader.GetAttribute("location"));
                        }
                        else if (reader.Name.Equals("MainLocation"))
                        {
                            mainLocations.Add(reader.GetAttribute("key"), reader.GetAttribute("name"));
                        }
                        else if (reader.Name.Equals("Archetype"))
                        {
                            archetypes.Add(reader.GetAttribute("key"), reader.GetAttribute("name"));
                        }
                        break;
                    case XmlNodeType.Text:
                        if (eventName != null)
                        {
                            RemnantItem rItem = new RemnantItem(reader.Value);
                            if (itemMode != null)
                            {
                                if (itemMode.Equals("hardcore"))
                                {
                                    rItem.ItemMode = RemnantItem.RemnantItemMode.Hardcore;
                                }
                                else if (itemMode.Equals("survival"))
                                {
                                    rItem.ItemMode = RemnantItem.RemnantItemMode.Survival;
                                }
                            }
                            if (itemNotes != null)
                            {
                                rItem.ItemNotes = itemNotes;
                            }
                            if (itemAltName != null)
                            {
                                rItem.ItemAltName = itemAltName;
                            }
                            eventItems.Add(rItem);
                            itemMode = null;
                            itemNotes = null;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name.Equals("Event"))
                        {
                            eventItem.Add(eventName, eventItems.ToArray());
                            eventName = null;
                            eventItems.Clear();
                        }
                        break;
                }
            }
            reader.Close();
        }
        */
    }
}