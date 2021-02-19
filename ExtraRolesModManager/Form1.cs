using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Collections;

namespace ExtraRolesModManager
{
    public partial class mainForm : Form
    {

        ModManager Manager = new ModManager();

        public mainForm()
        {
            InitializeComponent();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.Manager.InitManager();
            this.DirectoryLabel.Text = $"Game Directory: {this.Manager.GameRootDirectory}";

            if (FileOperations.IsValidAmongUsDirectory(this.Manager.GameRootDirectory))
            {
                this.CheckUpdateButton.Enabled = true;
            }

            this.Manager.LoadModVersion();
            if(this.Manager.ModFileLoaded)
            {
                this.DeleteModButton.Enabled = true;
                this.CurrentVersionLabel.Text = $"Installed ExtraRoles Mod Version: {(this.Manager.ModVersion == "" ? "None" : this.Manager.ModVersion)}";
            }
        }
        private void SetStatus(string status)
        {
            this.StatusLabel.Text = $"Status: {status}";
        }

        private void SelectDirectory_Click(object sender, EventArgs e)
        {
            this.SetStatus("Selecting Game Directory");
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;
            DialogResult _result = fbd.ShowDialog();
            if (_result == DialogResult.OK)
            {
                var setSuccessfully = this.Manager.SetGameDirectory(fbd.SelectedPath);
                if(!setSuccessfully)
                {
                    MessageBox.Show("Among Us.exe could not be found. Make sure to select the Among Us root directory.", "Error");
                }
                else
                {
                    this.DirectoryLabel.Text = $"Game Directory: {this.Manager.GameRootDirectory}";
                }
            }
            this.SetStatus("");
        }

        private void DeleteModClicked(object sender, EventArgs e)
        {
            DialogResult _result = MessageBox.Show("Do you really want to remove all mod related files?", "Delete ExtraRoles Mod", MessageBoxButtons.YesNoCancel);
            if (_result == DialogResult.Yes)
            {
                this.SetStatus("Deleting Mod Files");
                var deletedSuccessfully = this.Manager.DeleteMod();
                this.SetStatus("");
                if (deletedSuccessfully)
                {
                    this.DeleteModButton.Enabled = false;
                    this.CurrentVersionLabel.Text = $"Installed ExtraRoles Mod Version: {(this.Manager.ModVersion == "" ? "None" : this.Manager.ModVersion)}";
                    MessageBox.Show("The mod files were deleted succesfully.", "Success");
                }
                else
                {
                    MessageBox.Show("The mod files could not be deleted from your hard drive. Please try again.", "Error");
                }
            }
        }

        private void CheckUpdateButton_Click(object sender, EventArgs e)
        {
            if (!FileOperations.IsValidAmongUsDirectory(this.Manager.GameRootDirectory))
            {
                MessageBox.Show("You have to select the Among Us directory before checking for a new update.", "Error");
                return;
            }
            this.SetStatus("Checking for update");
            var newVersionAvailable = this.Manager.CheckForNewVersion();
            this.SetStatus("");
            if (newVersionAvailable)
            {
                DialogResult _Result = MessageBox.Show($"Found a new version: {NetworkOperations.GetLatestVersion()} - Do you want to install it?", "New Version available", MessageBoxButtons.YesNoCancel);
                if (_Result == DialogResult.Yes)
                {
                    this.SetStatus("Downloading and Extracting Mod Files");
                    var succesfully = this.Manager.DownloadLatestVersion();
                    this.SetStatus("");
                    if (succesfully && this.Manager.ModFileLoaded)
                    {
                        this.DeleteModButton.Enabled = true;
                        this.CurrentVersionLabel.Text = $"Installed ExtraRoles Mod Version: {(this.Manager.ModVersion == "" ? "None" : this.Manager.ModVersion)}";
                        MessageBox.Show($"ExtraRoles Mod Version: {this.Manager.ModVersion} was installed succesfully.", "Installation was succesful");
                    }
                    else
                    {
                        MessageBox.Show($"ExtraRoles Mod Version: {this.Manager.ModVersion} could not be installed. Please try again.", "Error");
                    }
                    
                }
            }
            else
            {
                MessageBox.Show($"Your installed ExtraRoles Mod version ({this.Manager.ModVersion}) is up to date.", "Up to date");
            }
        }

        private void LinkLabelClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/heinzef");
        }

        private void ModLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/NotHunter101/ExtraRolesAmongUs");
        }       
    }
}
