using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using Gameloop.Vdf;
using System.Configuration;
using G64Installer.ViewModels;

namespace G64Installer
{
    internal class Installer
    {
        const string RELEASES_URL = "https://api.github.com/repos/ckosmic/g64/releases";

        public static dynamic releases = null;

        internal static async Task<dynamic> GetReleases()
        {
            string jsonString = "";
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36");
                jsonString = wc.DownloadString(RELEASES_URL);

                releases = JsonConvert.DeserializeObject(jsonString);

                return releases;
            }
        }

        internal static string FindGarrysModFolder()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Valve\\Steam"))
            {
                if (key != null)
                {
                    string steamInstallPath = (string)key.GetValue("InstallPath");
                    string libraryFoldersPath = Path.Combine(steamInstallPath, "steamapps", "libraryfolders.vdf");
                    if (File.Exists(libraryFoldersPath))
                    {
                        dynamic libraryFolders = VdfConvert.Deserialize(File.ReadAllText(libraryFoldersPath));
                        for (int i = 0; i < libraryFolders.Value.Children().Count; i++)
                        {
                            string libPath = libraryFolders.Value[i.ToString()].path.ToString() as string;
                            string appManifestPath = Path.Combine(libPath, "steamapps", "appmanifest_4000.acf");
                            if (File.Exists(appManifestPath))
                            {
                                dynamic appManifest = VdfConvert.Deserialize(File.ReadAllText(appManifestPath));
                                string gmodInstallFolder = appManifest.Value.installdir.ToString() as string;
                                string gmodPath = Path.Combine(libPath, "steamapps", "common", gmodInstallFolder);
                                return gmodPath;
                            }
                        }
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to get Steam install path from registry.");
                }
                return null;
            }
        }

        internal static async Task DownloadRelease(string zipUrl, string zipName, string unpackDir, MainWindowViewModel vm)
        {
            string zipPath = Path.Combine(Path.GetTempPath(), zipName);

            using (WebClient wc = new WebClient())
            {
                await wc.DownloadFileTaskAsync(zipUrl, zipPath);
                vm.InstallStatus = "Unpacking release...";
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, unpackDir, true);
                vm.InstallStatus = "Removing downloaded zip...";
                File.Delete(zipPath);
            }
        }

        internal static bool IsValidInstallFolder(string path)
        {
            return File.Exists(Path.Combine(path, "hl2.exe")) || File.Exists(Path.Combine(path, "gmod.exe"));
        }

        internal static async Task<int> InstallG64(MainWindowViewModel vm)
        {
            if (releases == null)
            {
                vm.InstallStatus = "Getting G64 release info...";
                await GetReleases();
                if (releases == null)
                    // Failed to get releases info
                    return 2;
            }
                

            string zipUrl = releases[0].assets[0].browser_download_url;
            string zipName = releases[0].assets[0].name;
            string installDir = ConfigurationManager.AppSettings["InstallDir"];

            vm.InstallStatus = "Checking if install directory is valid...";
            if (!IsValidInstallFolder(installDir))
                return 1;

            vm.InstallStatus = "Downloading release...";
            await DownloadRelease(zipUrl, zipName, installDir, vm);
            vm.InstallStatus = "Success!";


            return 0;
        }

        internal static int UninstallG64(MainWindowViewModel vm)
        {
            string installDir = ConfigurationManager.AppSettings["InstallDir"];

            vm.InstallStatus = "Checking if install directory is valid...";
            if (!IsValidInstallFolder(installDir))
                return 1;

            vm.InstallStatus = "Removing installed files...";
            File.Delete(Path.Combine(installDir, "bin", "win64", "sm64.dll"));
            File.Delete(Path.Combine(installDir, "bin", "win64", "bz2.dll"));
            File.Delete(Path.Combine(installDir, "bin", "win64", "jsoncpp.dll"));
            File.Delete(Path.Combine(installDir, "bin", "win64", "libcurl.dll"));
            File.Delete(Path.Combine(installDir, "bin", "win64", "zip.dll"));
            File.Delete(Path.Combine(installDir, "bin", "win64", "zlib1.dll"));
            File.Delete(Path.Combine(installDir, "garrysmod", "lua", "bin", "gmcl_g64_win64.dll"));
            File.Delete(Path.Combine(installDir, "garrysmod", "lua", "bin", "gmcl_g64_updater_win64.dll"));
            vm.InstallStatus = "Success!";

            return 0;
        }
    }
}
