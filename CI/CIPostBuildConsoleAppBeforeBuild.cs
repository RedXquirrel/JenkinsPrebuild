using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI
{
    public class CIPostBuildConsoleAppBeforeBuild : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string LogPath { get; set; }
        [Required]
        public string CIDLLPath { get; set; }

        [Required]
        public string CIDLLDestinationPath { get; set; }

        public override bool Execute()
        {
            if (!LogBeforeBuildStart()) { LogFailedMethod("LogBeforeBuildStart()"); return false; }

            //System.IO.File.Copy(CIDLLPath, CIDLLDestinationPath, true);

            return true;
        }

        private bool LogBeforeBuildStart()
        {
            if (!CheckLogPathExists()) { LogFailedMethod("CheckLogPathExists()"); return false; }
            if (!LogBeforeBuildStarted()) { LogFailedMethod("LogBeforeBuildStarted()"); return false; }
            return true;
        }

        private bool LogBeforeBuildStart()
        {
            if (!CheckLogPathExists()) { LogFailedMethod("CheckLogPathExists()"); return false; }
            if (!LogBeforeBuildStarted()) { LogFailedMethod("LogBeforeBuildStarted()"); return false; }
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
