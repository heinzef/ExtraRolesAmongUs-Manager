using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Collections;

namespace ExtraRolesUpdater
{
    public partial class mainForm : Form
    {

        public string modVersion = "";
        public string gameRootDirectory = "";

        public string[] FilesAndDirsToDelete = new string[] { "Assets", "BepinEx", "mono", "doorstop_config.ini", "steam_appid.txt", "winhttp.dll" };

        static readonly HttpClient client = new HttpClient();

        public mainForm()
        {
            InitializeComponent();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.gameRootDirectory = (string)Properties.Settings.Default.GameDirectory;
            if (this.gameRootDirectory.Length > 0)
            {
                this.SetAmongUsDirectory(this.gameRootDirectory);
            }
            if (String.IsNullOrEmpty(this.modVersion))
            {
                this.DeleteModButton.Enabled = false;
            }
        }

        private void SetStatus(string status)
        {
            this.StatusLabel.Text = $"Status: {status}";
        }

        private void SetModVersion(string modVersion)
        {
            this.modVersion = modVersion;
            this.CurrentVersionLabel.Text = $"Installed ExtraRoles Mod Version: {(modVersion == "" ? "None" : modVersion)}";
            if(String.IsNullOrEmpty(this.modVersion))
            {
                this.DeleteModButton.Enabled = false;
            }
            else
            {
                this.DeleteModButton.Enabled = true;
            }
        }

        private void SetAmongUsDirectory(string path)
        {
            if(!File.Exists($"{path}/Among Us.exe"))
            {
                MessageBox.Show("Among Us.exe could not be found. Make sure to select the Among Us root directory.", "Error");
                return;
            }
            if(!this.gameRootDirectory.Equals(path))
            {
                this.gameRootDirectory = path;
                Properties.Settings.Default.GameDirectory = this.gameRootDirectory;
                Properties.Settings.Default.Save();
            }
            this.CheckUpdateButton.Enabled = true;
            this.DeleteModButton.Enabled = true;
            this.DirectoryLabel.Text = $"Among Us Directory: {path}";
            this.LoadModVersionInfo();
        }

        private void LoadModVersionInfo()
        {
            if(this.gameRootDirectory.Length > 0)
            {
                string modFilePath = "BepInEx/plugins/";
                string combinedPath = Path.Combine(this.gameRootDirectory, modFilePath);
                if(Directory.Exists(combinedPath))
                {
                    string[] files = Directory.GetFiles(combinedPath);
                    string modFile = files.Where(str => str.Contains("ExtraRoles")).FirstOrDefault();
                    if (File.Exists(modFile))
                    {
                        this.SetModVersion(FileVersionInfo.GetVersionInfo(modFile).ProductVersion);
                    }
                }
            }
        }

        private async void CheckForUpdate() 
        {
            if(this.gameRootDirectory.Length == 0)
            {
                MessageBox.Show("You have to select the Among Us directory before checking for a new update.", "Error");
                return;
            }
            HttpResponseMessage response = await client.GetAsync("https://github.com/NotHunter101/ExtraRolesAmongUs/releases/latest/");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            string regex = "href=\"((.*)/download/(.*)/(.*)\\.zip)\"";
            Match match = Regex.Match(responseBody, regex);
            if (match.Success)
            {
                string link = match.Groups[3].Value;
                int compareResult = link.Substring(1).CompareTo(this.modVersion);
                if (compareResult == 1)
                {
                    DialogResult _Result = MessageBox.Show($"Found a new version: {link.Substring(1)} - Do you want to install it?", "New Version available", MessageBoxButtons.YesNoCancel);
                    if(_Result == DialogResult.Yes)
                    {
                        var _url = $"https://github.com{match.Groups[1].Value}";
                        this.DownloadFile(_url);
                    }
                }
                else
                {
                    MessageBox.Show("Your installed ExtraRoles Mod version is up to date.", "Up to date");
                }
            }
        }

        private async void DownloadFile(string url)
        {
            // validation
            
            var fileInfo = new FileInfo("Download.zip");

            this.SetStatus("Downloading Mod Files");

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var ms = await response.Content.ReadAsStreamAsync();
            var fs = File.Create(fileInfo.FullName);
            ms.Seek(0, SeekOrigin.Begin);
            ms.CopyTo(fs);
            fs.Dispose();
            ms.Dispose();
            if(File.Exists(fileInfo.FullName))
            {
                this.ZipFileExtractToDirectory(fileInfo.FullName, this.gameRootDirectory);
                File.Delete(fileInfo.FullName);
                this.LoadModVersionInfo();
            }
        }

        private void ZipFileExtractToDirectory(string zipPath, string extractPath)
        {
            this.SetStatus("Extracting Mod Files");
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                Array entries = archive.Entries.ToArray();
                Array.Sort(entries, new ZipEntryComparer());
                foreach (ZipArchiveEntry entry in entries)
                {
                    if(entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\") || entry.Name == "")
                    {
                        if(!Directory.Exists(Path.Combine(extractPath, entry.FullName)))
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
            this.SetStatus("Finished");
            MessageBox.Show("ExtraRoles Mod was installed succesfully!", "Success");
        }

        private void CheckUpdateButton_Click(object sender, EventArgs e)
        {
            this.CheckForUpdate();
        }

        private void SelectDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;
            DialogResult _result = fbd.ShowDialog();
            if(_result == DialogResult.OK)
            {
                this.SetAmongUsDirectory(fbd.SelectedPath);
            }
        }

        private void LinkLabelClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/heinzef");
        }

        private void DeleteModClicked(object sender, EventArgs e)
        {
            DialogResult _result = MessageBox.Show("Do you really want to remove all mod related files?", "Delete ExtraRoles Mod", MessageBoxButtons.YesNoCancel);
            if(_result == DialogResult.Yes)
            {
                foreach (string path in this.FilesAndDirsToDelete)
                {
                    string combinedPath = Path.Combine(this.gameRootDirectory, path);
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
                this.SetModVersion("");
            }
           
        }

        private void ModLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/NotHunter101/ExtraRolesAmongUs");
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
