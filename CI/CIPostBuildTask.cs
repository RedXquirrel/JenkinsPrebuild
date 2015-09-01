using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI
{
    public class CIPostBuildTask
    {
        private PostBuildConfigSettingsModel _config;

        public string ProjectName { get; set; }
        public string LogPath { get; set; }
        public string NextBuildNumber { get; set; }
        public string IPASourceFileName { get; set; }
        public string IPATargetFileName { get; set; }
        public string IPASourceDirectory { get; set; }
        public string IPATargetDirectory { get; set; }

        public async Task<bool> Run()
        {
            string logDirectory = @"/Users/CI/.jenkins/jobs/Jenkins Automatic Deployment Template/";
            string logPath = System.IO.Path.Combine(logDirectory, "MyExeLogFile.txt");
            LogPath = logPath;

            CheckLogPathExists();
            LogBeforeBuildStarted();

            string CIConfigDirectory = @"/Users/CI/.jenkins/jobs/Jenkins Automatic Deployment Template/CIConfig";

            using (StreamWriter sw = File.AppendText(logPath))
            {
                sw.WriteLine(string.Format("     Configuration Directory: {0}", CIConfigDirectory));
            }

            string CIConfigJsonFileName = @"CIPostBuildConfig.json";

            using (StreamWriter sw = File.AppendText(logPath))
            {
                sw.WriteLine(string.Format("     Configuration File Name: {0}", CIConfigJsonFileName));
            }

            string CIConfigJsonFilePath = Path.Combine(CIConfigDirectory, CIConfigJsonFileName);

            using (StreamWriter sw = File.AppendText(logPath))
            {
                sw.WriteLine(string.Format("     Configuration File Path: {0}", CIConfigJsonFilePath));
            }

            string configjson = File.ReadAllText(CIConfigJsonFilePath);

            using (StreamWriter sw = File.AppendText(logPath))
            {
                sw.WriteLine(string.Format("     Configuration JSON: {0}", configjson));
            }

            _config = JsonConvert.DeserializeObject<PostBuildConfigSettingsModel>(configjson);

            ProjectName = _config.ProjectName;

            NextBuildNumber = _config.NextBuildNumber;
            IPASourceFileName = _config.IPASourceFileName;
            IPATargetFileName = _config.IPATargetFileName;
            IPASourceDirectory = _config.IPASourceDirectory;
            IPATargetDirectory = _config.IPATargetDirectory;

            string sourceFile = System.IO.Path.Combine(IPASourceDirectory, IPASourceFileName);

            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine(string.Format("     IPA Source Filename {0}", IPASourceFileName));
                sw.WriteLine(string.Format("     IPA Source Directory {0}", IPASourceDirectory));
                sw.WriteLine(string.Format("     IPA Source Path {0}", sourceFile));
            }

            string destFile = string.Empty;
            if (IPATargetFileName.Contains("{0}"))
            {
                IPATargetFileName = string.Format(IPATargetFileName, NextBuildNumber.Trim());

                destFile = System.IO.Path.Combine(IPATargetDirectory, IPATargetFileName);
            }
            else
            {
                destFile = System.IO.Path.Combine(IPATargetDirectory, IPATargetFileName);
            }

            using (StreamWriter sw = File.AppendText(LogPath))
            {
                //LogMessage(string.Format("     IPA Target Filename {0}", IPATargetFileName));
                //LogMessage(string.Format("     IPA Target Directory {0}", IPATargetDirectory));
                //LogMessage(string.Format("     IPA Target Path {0}", destFile));

                sw.WriteLine(string.Format("     IPA Target Filename {0}", IPATargetFileName));
                sw.WriteLine(string.Format("     IPA Target Directory {0}", IPATargetDirectory));
                sw.WriteLine(string.Format("     IPA Target Path {0}", destFile));
            }

            if (!System.IO.Directory.Exists(IPATargetDirectory))
            {
                System.IO.Directory.CreateDirectory(IPATargetDirectory);
            }

            if (File.Exists(sourceFile))
            {
                System.IO.File.Copy(sourceFile, destFile, true);
            }
            else
            {
                LogMessage(string.Format("     ERROR File Path Does not exist {0}", sourceFile));
            }

            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine(string.Format("     Copied {0} to IPAArchives Directory", IPATargetFileName));
            }

            LogMessage("     COMMENCING DROPBOX UPLOAD SEQUENCE");

            using (var dbx = new DropboxClient("562js3vx70samgc"))
            {
                LogMessage("     :     Entered using new DropbBoxClient() phase.");
                var full = await dbx.Users.GetCurrentAccountAsync();
                LogMessage(string.Format("     DropBox User DropBox Client Created for {0} - {1}", full.Name.DisplayName, full.Email));

            ////    //await UploadToCIDropBox(dbx, "test", "test.txt", "Hello Dropbox");

            ////    //using (StreamWriter sw = File.AppendText(LogPath))
            ////    //{
            ////    //    sw.WriteLine("     Hello World uploaded to dropbox");
            ////    //}
            }

            LogAfterBuildFinished();
            return true;
        }

        private void LogMessage(string message)
        {
            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine(message);
            }
        }

        async Task UploadToCIDropBox(DropboxClient dbx, string folder, string file, string content)
        {
            using (var mem = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var updated = await dbx.Files.UploadAsync(
                    folder + "/" + file,
                    WriteMode.Overwrite.Instance,
                    body: mem);
                Console.WriteLine("Saved {0}/{1} rev {2}", folder, file, updated.Rev);
                using (StreamWriter sw = File.AppendText(LogPath))
                {
                    sw.WriteLine(string.Format("     DropBox Saved {0}/{1} rev {2}", folder, file, updated.Rev));
                }
            }
        }

        private void LogAfterBuildFinished()
        {
            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine(string.Format("Post Build Finished for Project {0} at {1} UTC", ProjectName, DateTime.UtcNow.ToString()));
            }
        }

        private void LogBeforeBuildStarted()
        {
            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine(string.Format("Post Build Started for Project {0} at {1} UTC", ProjectName, DateTime.UtcNow.ToString()));
            }
        }

        private void CheckLogPathExists()
        {
            if (!File.Exists(LogPath))
            {
                using (StreamWriter sw = File.CreateText(LogPath))
                {
                    sw.WriteLine(string.Format("Build Server Log File Created: {0} UTC", DateTime.UtcNow.ToString()));
                }
            }
        }
    }
}
