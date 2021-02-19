using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ExtraRolesModManager
{
    class ZipOperations
    {

        public static bool ZipFileExtractToDirectory(string zipPath, string extractPath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                Array entries = archive.Entries.ToArray();
                Array.Sort(entries, new ZipEntryComparer());
                foreach (ZipArchiveEntry entry in entries)
                {
                    if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\") || entry.Name == "")
                    {
                        if (!Directory.Exists(Path.Combine(extractPath, entry.FullName)))
                        {
                            Directory.CreateDirectory(Path.Combine(extractPath, entry.FullName));
                        }
                    }
                    else
                    {
                        entry.ExtractToFile(Path.Combine(extractPath, entry.FullName), true);
                    }

                }
            }
            return false;
        }
    }

    class ZipEntryComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            return (new CaseInsensitiveComparer()).Compare(((ZipArchiveEntry)x).FullName.Length, ((ZipArchiveEntry)y).FullName.Length);
        }
    }
}
