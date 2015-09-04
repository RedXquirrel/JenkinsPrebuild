﻿using Com.Xamtastic.Patterns.CI.DropboxCI;
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

            LogMessage(string.Format("     Configuration Directory: {0}", CIConfigDirectory));

            string CIConfigJsonFileName = @"CIPostBuildConfig.json";

            LogMessage(string.Format("     Configuration File Name: {0}", CIConfigJsonFileName));


            string CIConfigJsonFilePath = Path.Combine(CIConfigDirectory, CIConfigJsonFileName);

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

            LogMessage("     COMMENCING DROPBOX UPLOAD SEQUENCE");

            using (var dbx = new DropboxClient("NYDBfyWzefAAAAAAAAAABqwodGdUMmmYVDuixOQaIOOXj3KejN36z1EOtgC5iy8O"))
            {
                LogMessage("     :     Entered using new DropbBoxClient() phase");
                try
                {
                    var full = await dbx.Users.GetCurrentAccountAsync();
                    LogMessage("     :     Awaited dbx.Users.GetCurrentAccountAsync()");
                    LogMessage(string.Format("     :     DropBox User DropBox Client Created for {0} - {1}", full.Name.DisplayName, full.Email));
                }
                catch (Exception ex)
                {
                    LogMessage(string.Format("     :     ERROR: {0}", ex.Message));
                }

                await UploadToDropBox(dbx, "/test", IPATargetFileName, destFile);
                //await UploadToDropBox(dbx, "/test", IPATargetFileName, CIConfigDirectory + "/test.zip");

                LogMessage("     :     Exiting using new DropbBoxClient() phase");
            }

            LogMessage("     :     Exited using new DropbBoxClient() phase");

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

        async Task UploadTextToDropBox(DropboxClient dbx, string folder, string filename, string content)
        {
            LogMessage("     :     In UploadToDropBox(...)");
            using (var mem = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                LogMessage("     :     In using (var mem = new MemoryStream(Encoding.UTF8.GetBytes(content)))");
                try
                {
                    var updated = await dbx.Files.UploadAsync(
                        folder + "/" + filename,
                        WriteMode.Overwrite.Instance,
                        body: mem);
                }
                catch (Exception ex)
                {
                    LogMessage(string.Format("     :     ERROR: {0}", ex.Message));
                }
                LogMessage("     :     Processed var updated = await dbx.Files.UploadAsync(...)");
                LogMessage("     :     Exiting  using (var mem = new MemoryStream(Encoding.UTF8.GetBytes(content)))");
            }
            LogMessage("     :     Exiting UploadToDropBox(...)");
        }

        async Task UploadToDropBox(DropboxClient dbxbak, string dropboxfolder, string dropboxfilename, string filepathtoUpload)
        {
            LogMessage("     :     In UploadToDropBox(...)");
            LogMessage(string.Format("     :          DropBox folder {0}", dropboxfolder));
            LogMessage(string.Format("     :          DropBox filename {0}", dropboxfolder));
            LogMessage(string.Format("     :          File to Upload {0}", filepathtoUpload));

            var dc = new DropboxCIClient("NYDBfyWzefAAAAAAAAAABqwodGdUMmmYVDuixOQaIOOXj3KejN36z1EOtgC5iy8O");
            dc.LogMessageAction = (message) =>
            {
                LogMessage(string.Format("     :          {0}", message));
            };
            dc.LogErrorAction = (message) =>
            {
                throw new Exception(message);
            };
            await dc.Upload(filepathtoUpload, dropboxfolder + "/" + dropboxfilename);

            //using (var dbx = new DropboxClient("NYDBfyWzefAAAAAAAAAABqwodGdUMmmYVDuixOQaIOOXj3KejN36z1EOtgC5iy8O"))
            //{
            //    using (FileStream fsSource = new FileStream(filepathtoUpload,
            //    FileMode.Open, FileAccess.Read))
            //    {
            //        try
            //        {
            //            var updated = await dbx.Files.UploadAsync(
            //                dropboxfolder + "/" + dropboxfilename,
            //                WriteMode.Overwrite.Instance,
            //                body: fsSource);
            //        }
            //        catch (Exception ex)
            //        {
            //            LogMessage(string.Format("     :     ERROR: {0}", ex.Message));
            //            throw;
            //        }
            //    }
            //}

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
            if (!File.Exists(LogPath))
            {
                LogMessage(string.Format("Build Server Log File Created: {0} UTC", DateTime.UtcNow.ToString()));
            }
        }
    }
}