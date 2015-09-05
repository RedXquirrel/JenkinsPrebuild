
using System.Configuration;
using System.Collections.Specialized;
using Com.Xamtastic.Patterns.CI.Dropbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamtastic.Automation
{
    class Program
    {
        static void Main(string[] args)
        {
            var task = Task.Run((Func<Task>)Program.Run);
            task.Wait();
        }

        static async Task Run()
        {
            CIPostBuildTask postBuild = new CIPostBuildTask();
            await postBuild.Run(
                ConfigurationManager.AppSettings.Get("DropboxGeneratedAccessToken"),
                ConfigurationManager.AppSettings.Get("AutomationLogDirectory"),
                ConfigurationManager.AppSettings.Get("AutomationLogFilename"),
                ConfigurationManager.AppSettings.Get("AutomationConfigDirectory"),
                ConfigurationManager.AppSettings.Get("AutomationConfigFilename")
                );
        }
    }
}
