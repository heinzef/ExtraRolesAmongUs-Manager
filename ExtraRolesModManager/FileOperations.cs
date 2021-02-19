using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace ExtraRolesModManager
{
    class FileOperations
    {
        private static readonly string[] FilesAndDirsToDelete = new string[] { "Assets", "BepinEx", "mono", "doorstop_config.ini", "steam_appid.txt", "winhttp.dll" };

        public static bool IsValidAmongUsDirectory(string pathToDirectory)
        {
            return File.Exists(Path.Combine(pathToDirectory, "Among Us.exe"));
        }

        public static string GetModVersion(string gameRootDirectory)
        {
            var modFilePath = "BepInEx/plugins/";
            var combinedPath = Path.Combine(gameRootDirectory, modFilePath);
            if (Directory.Exists(combinedPath))
            {
                var files = Directory.GetFiles(combinedPath);
                var modFile = files.Where(str => str.Contains("ExtraRoles")).FirstOrDefault();
                if (File.Exists(modFile))
                {
                    return FileVersionInfo.GetVersionInfo(modFile).ProductVersion;
                }
            }
            return "";
        }

        public static bool DeleteModFiles(string gameRootDirectory)
        {
            try
            {
                foreach (string path in FilesAndDirsToDelete)
                {
                    string combinedPath = Path.Combine(gameRootDirectory, path);
                    if (!String.IsNullOrWhiteSpace(Path.GetExtension(path)))
                    {
                        if (File.Exists(combinedPath))
                        {
                            File.Delete(combinedPath);
                        }
                    }
                    else
                    {
                        if (Directory.Exists(combinedPath))
                        {
                            Directory.Delete(combinedPath, true);
                        }
                    }
                }
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }
    }
}
