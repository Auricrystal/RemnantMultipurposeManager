using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RemnantBuildRandomizer.WorldSave;

namespace RemnantBuildRandomizer
{
    public static class MyExtensions
    {
       
        public static void RenameEntry(this ZipArchive archive, string oldName, string newName)
        {
            ZipArchiveEntry oldEntry = archive.GetEntry(oldName),
                newEntry = archive.CreateEntry(newName);
            using (Stream oldStream = oldEntry.Open())
            using (Stream newStream = newEntry.Open())
            {
                oldStream.CopyTo(newStream);
            }
            oldEntry.Delete();
        }

        public static WorldSave Copy(this WorldSave rb)
        {
            return new WorldSave(rb.path, rb.Diff, rb.Name, rb.World, rb.Modifiers,rb.Description);
        }

    }
}
