using Microsoft.Build.Framework;
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
        public string ProjectName { get; set; }
        public string LogPath { get; set; }
        public string IPASourceFileName { get; set; }
        public string IPATargetFileName { get; set; }
        public string IPASourceDirectory { get; set; }
        public string IPATargetDirectory { get; set; }

        public async Task<bool> Run()
        {

            string path = @"/Users/CI/.jenkins/jobs/Jenkins Prebuild/";
            string logFile = System.IO.Path.Combine(path, "MyExeLogFile.txt");

            using (StreamWriter sw = File.AppendText(logFile))
            {
                sw.WriteLine(string.Format("After Build Started for Project {0} at {1} UTC", ProjectName, DateTime.UtcNow.ToString()));
            }

            return true;

            CheckLogPathExists();
            LogBeforeBuildStarted();

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
                destFile = System.IO.Path.Combine(IPATargetDirectory, string.Format(IPATargetFileName, "x.x.x"));
            }
            else
            {
                destFile = System.IO.Path.Combine(IPATargetDirectory, IPATargetFileName);
            }

            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine(string.Format("     IPA Target Filename {0}", IPATargetFileName));
                sw.WriteLine(string.Format("     IPA Target Directory {0}", IPATargetDirectory));
                sw.WriteLine(string.Format("     IPA Target Path {0}", destFile));
            }

            if (!System.IO.Directory.Exists(IPATargetDirectory))
            {
                System.IO.Directory.CreateDirectory(IPATargetDirectory);
            }
            //await Task.Delay(120000);
            if (File.Exists(sourceFile))
            {
                System.IO.File.Copy(sourceFile, destFile, true);
            }
            else
            {
                using (StreamWriter sw = File.AppendText(LogPath))
                {
                    sw.WriteLine(string.Format("     ERROR File Path Does not exist {0}", sourceFile));
                }
            }

            LogAfterBuildFinished();
            return true;
        }

        private void LogAfterBuildFinished()
        {
            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine(string.Format("After Build Finished for Project {0} at {1} UTC", ProjectName, DateTime.UtcNow.ToString()));
            }
        }

        private void LogBeforeBuildStarted()
        {
            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine(string.Format("After Build Started for Project {0} at {1} UTC", ProjectName, DateTime.UtcNow.ToString()));
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
