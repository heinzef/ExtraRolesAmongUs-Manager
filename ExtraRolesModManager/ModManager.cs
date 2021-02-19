using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtraRolesModManager
{
    class ModManager
    {
        public string ModVersion { get; set;  }
        public string GameRootDirectory { get; set; }
        public bool ModFileLoaded { get; set; }

        public ModManager()
        {
            this.ModVersion = "";
            this.GameRootDirectory = "";
            this.ModFileLoaded = false;
        }

        public void InitManager()
        {
            this.GameRootDirectory = Properties.Settings.Default.GameDirectory;
        }

        public bool SetGameDirectory(string pathToDirectory)
        {
            if(!FileOperations.IsValidAmongUsDirectory(pathToDirectory))
            {
                return false;
            }
            if(!this.GameRootDirectory.Equals(pathToDirectory))
            {
                this.GameRootDirectory = pathToDirectory;
                Properties.Settings.Default.GameDirectory = pathToDirectory;
                Properties.Settings.Default.Save();
            }
            return true;
        }

        public bool LoadModVersion()
        {
            if(this.GameRootDirectory.Length == 0 || !FileOperations.IsValidAmongUsDirectory(this.GameRootDirectory))
            {
                return false;
            }
            this.ModVersion = FileOperations.GetModVersion(this.GameRootDirectory);
            this.ModFileLoaded = this.ModVersion.Length > 0;
            return true;
        }

        public bool CheckForNewVersion()
        {
            return NetworkOperations.IsNewVersionAvailable(this.ModVersion);
        }

        public bool DownloadLatestVersion()
        {
            var downloadedAndExtracted = NetworkOperations.DownloadLatestVersion(this.GameRootDirectory);
            if(downloadedAndExtracted)
            {
                this.LoadModVersion();
                return true;
            }
            return false;
        }

        public bool DeleteMod()
        {
            var deleted = FileOperations.DeleteModFiles(this.GameRootDirectory);
            if(deleted)
            {
                this.LoadModVersion();
            }
            return deleted;
        }
    }
}
