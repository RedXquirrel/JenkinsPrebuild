using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI
{
    public class DemoBeforeBuildTask : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string ProjectName { get; set; }

        [Required]
        public string LogPath { get; set; }

        public override bool Execute()
        {

            string path = string.Format(LogPath);
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(string.Format("Build Server Log File Created: {0} UTC", DateTime.UtcNow.ToString()));
                }
            }

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(string.Format("Before Build Started for Project {0} at {1} UTC", ProjectName, DateTime.UtcNow.ToString()));
            }

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(string.Format("Before Build Finished for Project {0} at {1} UTC", ProjectName, DateTime.UtcNow.ToString()));
            }


            return true;
        }


    }
}
