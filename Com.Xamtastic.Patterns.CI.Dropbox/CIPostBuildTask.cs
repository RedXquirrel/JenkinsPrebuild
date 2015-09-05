using Com.Xamtastic.Patterns.CI.DropboxCI;
using Dropbox.Api;
using Dropbox.Api.Files;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Xamtastic.Patterns.CI.Dropbox
{
    public class CIPostBuildTask
    {
        private PostBuildConfigSettingsModel _config;

        public string DropboxClientKey { get; set; }

        public string ProjectName { get; set; }

        public string LogPath { get; set; }
        public string AutomationLogDirectory { get; set; }
        public string AutomationLogFilename { get; set; }
        public string NextBuildNumber { get; set; }
        public string IPASourceFileName { get; set; }
        public string IPATargetFileName { get; set; }
        public string IPASourceDirectory { get; set; }
        public string IPATargetDirectory { get; set; }

        public async Task<bool> Run(
            string dropboxGeneratedAccessToken,
            string automationLogDirectory,
            string automationLogFilename,
            string automationConfigDirectory,
            string automationConfigFilename)
        {

            DropboxClientKey = dropboxGeneratedAccessToken;

            AutomationLogFilename = automationLogFilename;
            AutomationLogDirectory = automationLogDirectory;

            LogPath = System.IO.Path.Combine(AutomationLogDirectory, AutomationLogFilename);

            CheckLogPathExists();
            LogBeforeBuildStarted();

            LogMessage(string.Format("     Configuration Directory: {0}", automationConfigDirectory));
            LogMessage(string.Format("     Configuration File Name: {0}", automationConfigFilename));


            string CIConfigJsonFilePath = Path.Combine(automationConfigDirectory, automationConfigFilename);

            LogMessage(string.Format("     Configuration File Path: {0}", CIConfigJsonFilePath));

            string configjson = File.ReadAllText(CIConfigJsonFilePath);

            LogMessage(string.Format("     Configuration JSON: {0}", configjson));

            _config = JsonConvert.DeserializeObject<PostBuildConfigSettingsModel>(configjson);

            ProjectName = _config.ProjectName;

            NextBuildNumber = _config.NextBuildNumber;
            IPASourceFileName = _config.IPASourceFileName;
            IPATargetFileName = _config.IPATargetFileName;
            IPASourceDirectory = _config.IPASourceDirectory;
            IPATargetDirectory = _config.IPATargetDirectory;

            string sourceFile = System.IO.Path.Combine(IPASourceDirectory, IPASourceFileName);

            LogMessage(string.Format("     IPA Source Filename {0}", IPASourceFileName));
            LogMessage(string.Format("     IPA Source Directory {0}", IPASourceDirectory));
            LogMessage(string.Format("     IPA Source Path {0}", sourceFile));

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

            LogMessage(string.Format("     IPA Target Filename {0}", IPATargetFileName));
            LogMessage(string.Format("     IPA Target Directory {0}", IPATargetDirectory));
            LogMessage(string.Format("     IPA Target Path {0}", destFile));

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

            LogMessage(string.Format("     Copied {0} to IPAArchives Directory", IPATargetFileName));

            await UploadToDropBox("/test", IPATargetFileName, destFile);

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

        async Task UploadToDropBox(string dropboxfolder, string dropboxfilename, string filepathtoUpload)
        {
            LogMessage("     :     In UploadToDropBox(...)");
            LogMessage(string.Format("     :          DropBox folder {0}", dropboxfolder));
            LogMessage(string.Format("     :          DropBox filename {0}", dropboxfolder));
            LogMessage(string.Format("     :          File to Upload {0}", filepathtoUpload));

            var dc = new DropboxCIClient(DropboxClientKey);
            dc.LogMessageAction = (message) =>
            {
                LogMessage(string.Format("     :          {0}", message));
            };
            dc.LogErrorAction = (message) =>
            {
                throw new Exception(message);
            };
            await dc.Upload(filepathtoUpload, dropboxfolder + "/" + dropboxfilename);

            LogMessage("     :     Exiting UploadToDropBox(...)");
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
            if (!System.IO.Directory.Exists(AutomationLogDirectory))
            {
                System.IO.Directory.CreateDirectory(AutomationLogDirectory);
            }
            if (!File.Exists(LogPath))
            {
                LogMessage(string.Format("Build Server Log File Created: {0} UTC", DateTime.UtcNow.ToString()));
            }
        }
    }
}
