using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RemnantBuildRandomizer
{
    static class XmlElementExtension
    {
        public static string GetXPath(this XmlElement element)
        {
            string path = "/" + element.Name;

            XmlElement parentElement = element.ParentNode as XmlElement;
            if (parentElement != null)
            {
                // Gets the position within the parent element.
                // However, this position is irrelevant if the element is unique under its parent:
                XmlNodeList siblings = parentElement.SelectNodes(element.Name);
                if (siblings != null && siblings.Count > 1) // There's more than 1 element with the same name
                {
                    int position = 1;
                    foreach (XmlElement sibling in siblings)
                    {
                        if (sibling == element)
                            break;

                        position++;
                    }

                    path = path + "/"+element.GetAttribute("name")+".png";
                }

                // Climbing up to the parent elements:
                path = parentElement.GetXPath() + path;
            }

            return path;
        }
        /*
        public static string GetXPath_element(this XmlElement element)
        {
            string path = "/" + element.BuildName;

            XmlElement parentElement = element.ParentNode as XmlElement;
            if (parentElement != null)
            {
                // Gets the position within the parent element.
                // However, this position is irrelevant if the element is unique under its parent:
                XmlNodeList siblings = parentElement.SelectNodes(element.BuildName);
                if (siblings != null && siblings.Count > 1) // There's more than 1 element with the same name
                {
                    int position = 1;
                    foreach (XmlElement sibling in siblings)
                    {
                        if (sibling == element)
                            break;

                        position++;
                    }

                    path = path + "/" + element.GetAttribute("name") + ".png";
                }   
            }

            return path;
        }
        */
    }
}
