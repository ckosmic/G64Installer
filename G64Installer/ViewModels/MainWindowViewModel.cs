using Avalonia.Controls;
using Avalonia.Interactivity;
using G64Installer.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using static G64Installer.Views.G64MessageBox;

namespace G64Installer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Title => "G64 Installer";

        public string GameDirectory
        {
            get
            {
                if (ConfigurationManager.AppSettings["InstallDir"] == null || ConfigurationManager.AppSettings["InstallDir"].Length == 0)
                {
                    _gameDirectory = Installer.FindGarrysModFolder();
                    ConfigurationManager.AppSettings["InstallDir"] = _gameDirectory;
                }
                return _gameDirectory;
            }
            set
            {
                ConfigurationManager.AppSettings["InstallDir"] = value;
                this.RaiseAndSetIfChanged(ref _gameDirectory, value);
            }
        }

        public string LatestTag 
        { 
            get
            {
                return "Latest G64 version: " + Installer.releases[0].tag_name;
            }
        }

        public string InstallStatus
        {
            get { return _installStatus; }
            set { this.RaiseAndSetIfChanged(ref _installStatus, value); }
        }

        private string _gameDirectory;
        private string _installStatus;


        private async void OnInstallClicked()
        {
            int result = await Installer.InstallG64(this);
            switch(result)
            {
                case 0:
                    G64MessageBox.Show(MainWindow.Instance, "Successfully installed G64!", "Success", MessageBoxButtons.Ok);
                    break;
                case 1:
                    G64MessageBox.Show(MainWindow.Instance, "The folder you've selected is not a valid Garry's Mod folder.", "Error", MessageBoxButtons.Ok);
                    break;
                case 2:
                    G64MessageBox.Show(MainWindow.Instance, "Failed to get GitHub releases info.", "Error", MessageBoxButtons.Ok);
                    break;
            }
        }

        private void OnUninstallClicked()
        {
            int result = Installer.UninstallG64(this);
            switch (result)
            {
                case 0:
                    G64MessageBox.Show(MainWindow.Instance, "Successfully uninstalled G64!", "Success", MessageBoxButtons.Ok);
                    break;
                case 1:
                    G64MessageBox.Show(MainWindow.Instance, "The folder you've selected is not a valid Garry's Mod folder.", "Error", MessageBoxButtons.Ok);
                    break;
            }
        }

        private async void OnBrowseClicked()
        {
            string path = await GetPath();
            Debug.WriteLine(path);
            if (path != null && path.Length > 0)
                GameDirectory = path;
        }

        private async Task<string> GetPath()
        {
            OpenFolderDialog ofd = new OpenFolderDialog();
            ofd.Title = "Browse for GarrysMod folder...";
            ofd.InitialDirectory = ConfigurationManager.AppSettings["InstallDir"];

            string result = await ofd.ShowAsync(MainWindow.Instance);

            return result;
        }
    }
}
