using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI
{
    public class IOSAfterBuildTask : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string ProjectName { get; set; }

        [Required]
        public string LogPath { get; set; }

        [Required]
        public string IPASourceFileName { get; set; }

        [Required]
        public string IPATargetFileName { get; set; }

        [Required]
        public string IPASourceDirectory { get; set; }

        [Required]
        public string IPATargetDirectory { get; set; }


        public override bool Execute()
        {
            return Run().Result;
        }

        private async Task<bool> Run()
        {
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
            await Task.Delay(5000);
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
