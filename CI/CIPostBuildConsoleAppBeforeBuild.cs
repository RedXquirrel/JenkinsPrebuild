using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI
{
    public class CIPostBuildConsoleAppBeforeBuild : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string ProjectName { get; set; }

        [Required]
        public string LogPath { get; set; }
        [Required]
        public string CIDLLPath { get; set; }

        [Required]
        public string CIDLLDestinationPath { get; set; }

        /// <summary>
        /// The main purpose here is to copy a newly built CI.dll reference to the project to cater for the matter that it's project might have been updated.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            if (!LogBeforeBuildStart()) { LogFailedMethod("LogBeforeBuildStart()"); return false; }

            if (!CheckCIDLLPathExists()) { LogFailedMethod("CheckCIDLLPathExists()"); return false; }

            if (!CopyCIDLL()) { LogFailedMethod("CopyCIDLL()"); return false; }

            if (!LogAfterBuildFinished()) { LogFailedMethod("LogAfterBuildFinished()"); return false; }

            return true;
        }

        private bool LogAfterBuildFinished()
        {
            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine(string.Format("Before Build Finished for Project {0} at {1} UTC", ProjectName, DateTime.UtcNow.ToString()));
            }
            return true;
        }

        private bool CopyCIDLL()
        {
            try
            {
                System.IO.File.Copy(CIDLLPath, CIDLLDestinationPath, true);
            }
            catch (Exception ex)
            {
                return false;
            }

            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine(string.Format("     File Copied to: {0}", CIDLLDestinationPath));
            }

            return true;
        }

        private bool CheckCIDLLPathExists()
        {
            if (File.Exists(CIDLLPath))
            {
                using (StreamWriter sw = File.AppendText(LogPath))
                {
                    sw.WriteLine(string.Format("     File Exists: {0}", CIDLLPath));
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(LogPath))
                {
                    sw.WriteLine(string.Format("     FAIL File does not exist: {0}", CIDLLPath));
                }
            }

            return true;
        }

        private void LogFailedMethod(string message)
        {
            if (File.Exists(LogPath))
            {
                using (StreamWriter sw = File.AppendText(LogPath))
                {
                    sw.WriteLine(string.Format("     FAIL BEFORE BUILD: {0}", message));
                }
            }
        }

        private bool LogBeforeBuildStart()
        {
            if (!CheckLogPathExists()) { LogFailedMethod("CheckLogPathExists()"); return false; }
            if (!LogBeforeBuildStarted()) { LogFailedMethod("LogBeforeBuildStarted()"); return false; }
            return true;
        }

        private bool LogBeforeBuildStarted()
        {
            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine(string.Format("Before Build Started for Project {0} at {1} UTC", ProjectName, DateTime.UtcNow.ToString()));
            }
            return true;
        }

        private bool CheckLogPathExists()
        {
            if (!File.Exists(LogPath))
            {
                using (StreamWriter sw = File.CreateText(LogPath))
                {
                    sw.WriteLine(string.Format("Build Server Log File Created: {0} UTC", DateTime.UtcNow.ToString()));
                }
            }
            return true;
        }
    }
}
