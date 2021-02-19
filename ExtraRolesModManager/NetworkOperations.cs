using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExtraRolesModManager
{
    class NetworkOperations
    {
        private static readonly HttpClient client = new HttpClient();
        
        private static string LatestDownloadURL = "";
        private static string LatestVersion = "";

        public static bool IsNewVersionAvailable(string currentVersion)
        {
            
            var response = client.GetAsync("https://github.com/NotHunter101/ExtraRolesAmongUs/releases/latest/").Result;
            response.EnsureSuccessStatusCode();
            var responseBody = response.Content.ReadAsStringAsync().Result;
            var regex = "href=\"((.*)/download/(.*)/(.*)\\.zip)\"";
            var match = Regex.Match(responseBody, regex);
            if (match.Success)
            {
                var version = match.Groups[3].Value;
                LatestVersion = version.Substring(1);
                LatestDownloadURL = $"https://github.com{match.Groups[1].Value}";
                var compareResult = LatestVersion.CompareTo(currentVersion);
                return compareResult == 1;
            }
            return false;
        }

        public static string GetLatestVersion()
        {
            return LatestVersion;
        }

        public static bool DownloadLatestVersion(string gameRootDirectory)
        {
            try
            {
                var fileInfo = new FileInfo("ExtraRoles.zip");

                var response = client.GetAsync(LatestDownloadURL).Result;
                response.EnsureSuccessStatusCode();
                var ms = response.Content.ReadAsStreamAsync().Result;
                var fs = File.Create(fileInfo.FullName);
                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(fs);
                fs.Dispose();
                ms.Dispose();
                if (File.Exists(fileInfo.FullName))
                {
                    ZipOperations.ZipFileExtractToDirectory(fileInfo.FullName, gameRootDirectory);
                    File.Delete(fileInfo.FullName);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception e)
            {
                return false;
            }
        }
    }
}
