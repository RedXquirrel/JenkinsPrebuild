using Microsoft.Build.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Xamtastic.Patterns.CI.Dropbox
{
    public class IOSBeforeBuildTask : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string ProjectName { get; set; }

        [Required]
        public string LogDirectory { get; set; }

        [Required]
        public string LogFilename { get; set; }

        [Required]
        public string BuildNumberPrefix { get; set; }

        [Required]
        public string NextBuildNumberFilePath { get; set; }

        [Required]
        public string InfoPlistPath { get; set; }

        #region Post Build Configuration File Settings

        [Required]
        public string PostBuildConfigFileName { get; set; }

        [Required]
        public string PostBuildConfigDirectory { get; set; }

        [Required]
        public string IPASourceFileName { get; set; }

        [Required]
        public string IPATargetFileName { get; set; }

        [Required]
        public string IPASourceDirectory { get; set; }

        [Required]
        public string IPATargetDirectory { get; set; }
        #endregion

        private bool _nextBuildNumberFilePathExists;
        private string _nextBuildNumberSuffix;
        private string _nextBuildNumber;
        private bool _infoPlistFileExists;
        private int _CFBundleShortVersionStringLineNumber = 0;
        private int _CFBundleVersionLineNumber = 0;
        private List<string> _infoPlistLineCollection = new List<string>();
        private string _logPath;

        public override bool Execute()
        {
            //throw new Exception(string.Format("*** JenkinsProjectJobRoot: [{0}]", JenkinsProjectJobRoot));
            _logPath = Path.Combine(LogDirectory, LogFilename);

            if (!File.Exists(NextBuildNumberFilePath))
            {
                //throw new Exception("CIError: Next Build Number File does not exist");
                return true;
            }
            #region Setup Post Build variables
            if (!CreatePostBuildConfigDirectory()) { LogFailedMethod("CreatePostBuildConfigDirectory()"); return false; }
            if (!CreatePostBuildJsonFile()) { LogFailedMethod("CreatePostBuildJsonFile()"); return false; }
            #endregion

            #region Conduct BeforeBuild
            if (!LogBeforeBuildStart()) { LogFailedMethod("LogBeforeBuildStart()"); return false; }
            if (!UpdateVersionNumberInInfoPlist()) { LogFailedMethod("UpdateVersionNumberInInfoPlist()"); return false; }
            if (!LogAfterBuildFinished()) { LogFailedMethod("LogAfterBuildFinished()"); return false; }
            #endregion

            return true;
        }

        private bool CreatePostBuildJsonFile()
        {
            if (!CheckNextBuildNumberFilePathExists()) { LogFailedMethod("CheckNextBuildNumberFilePathExists()"); return false; }
            try
            {
                string nextBuildNumber = "x.x.x";
                if (_nextBuildNumberFilePathExists)
                {
                    var _nextBuildNumberSuffix = System.IO.File.ReadAllText(NextBuildNumberFilePath);
                    nextBuildNumber = string.Format("{0}.{1}", BuildNumberPrefix, _nextBuildNumberSuffix.Trim());
                }

                string json = JsonConvert.SerializeObject(new PostBuildConfigSettingsModel
                {
                    ProjectName = ProjectName,
                    NextBuildNumberFilePath = NextBuildNumberFilePath,
                    NextBuildNumber = nextBuildNumber,
                    IPASourceDirectory = IPASourceDirectory,
                    IPASourceFileName = IPASourceFileName,
                    IPATargetDirectory = IPATargetDirectory,
                    IPATargetFileName = IPATargetFileName
                });

                string configFilePath = System.IO.Path.Combine(PostBuildConfigDirectory, PostBuildConfigFileName);

                using (StreamWriter sw = File.CreateText(configFilePath))
                {
                    sw.WriteLine(json);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        private bool CreatePostBuildConfigDirectory()
        {
            try
            {
                if (!System.IO.Directory.Exists(PostBuildConfigDirectory))
                {
                    System.IO.Directory.CreateDirectory(PostBuildConfigDirectory);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        private bool LogBeforeBuildStart()
        {
            if (!CheckLogPathExists()) { LogFailedMethod("CheckLogPathExists()"); return false; }
            if (!LogBeforeBuildStarted()) { LogFailedMethod("LogBeforeBuildStarted()"); return false; }
            return true;
        }

        private void LogFailedMethod(string message)
        {
            if (File.Exists(_logPath))
            {
                using (StreamWriter sw = File.AppendText(_logPath))
                {
                    sw.WriteLine(string.Format("     FAIL BEFORE BUILD: {0}", message));
                }
            }
        }

        private bool UpdateVersionNumberInInfoPlist()
        {
            if (!CheckNextBuildNumberFilePathExists()) { LogFailedMethod("CheckNextBuildNumberFilePathExists()"); return false; }
            if (!DeriveNextBuildNumber()) { LogFailedMethod("DeriveNextBuildNumber()"); return false; }
            if (!CheckInfoPlistPathExists()) { LogFailedMethod("CheckInfoPlistPathExists()"); return false; }
            if (!CreateInfoPlistLineCollection()) { LogFailedMethod("CreateInfoPlistLineCollection()"); return false; }
            if (!IdentifyCFBundleShortVersionStringLineNumber()) { LogFailedMethod("IdentifyCFBundleShortVersionStringLineNumber()"); return false; }
            if (!InsertNextShortVersionBuildNumberInInfoPlistLineCollection()) { LogFailedMethod("InsertNextShortVersionBuildNumberInInfoPlistLineCollection()"); return false; }
            if (!IdentifyNextVersionBuildNumberInInfoPlistLineCollection()) { LogFailedMethod("IdentifyNextVersionBuildNumberInInfoPlistLineCollection()"); return false; }
            if (!InsertNextVersionBuildNumberInInfoPlistLineCollection()) { LogFailedMethod("InsertNextVersionBuildNumberInInfoPlistLineCollection()"); return false; }
            if (!RewriteInfoPlist()) { LogFailedMethod("RewriteInfoPlist()"); return false; }
            return true;
        }

        private bool RewriteInfoPlist()
        {
            if (File.Exists(InfoPlistPath))
            {
                _infoPlistFileExists = true;
                File.Delete(InfoPlistPath);
                File.WriteAllLines(InfoPlistPath, _infoPlistLineCollection.ToArray<string>());

                if (File.Exists(_logPath))
                {
                    using (StreamWriter sw = File.AppendText(_logPath))
                    {
                        sw.WriteLine(string.Format("     info.plist Path rewritten at {0}", _logPath));
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool InsertNextVersionBuildNumberInInfoPlistLineCollection()
        {
            if (_CFBundleVersionLineNumber != 0)
            {
                _infoPlistLineCollection[_CFBundleVersionLineNumber + 1] = string.Format("<string>{0}</string>", BuildNumberPrefix.Trim());

                if (File.Exists(_logPath))
                {
                    using (StreamWriter sw = File.AppendText(_logPath))
                    {
                        foreach (var line in _infoPlistLineCollection)
                        {
                            sw.WriteLine(string.Format("     {0}", line.Trim()));
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool IdentifyNextVersionBuildNumberInInfoPlistLineCollection()
        {
            var counter = 0;
            foreach (var line in _infoPlistLineCollection)
            {
                if (line.Trim().Equals("<key>CFBundleVersion</key>"))
                {
                    if (File.Exists(_logPath))
                    {
                        _CFBundleVersionLineNumber = counter;
                        using (StreamWriter sw = File.AppendText(_logPath))
                        {
                            sw.WriteLine(string.Format("     <keyCFBundleVersion</key> exists at line {0}", _CFBundleVersionLineNumber.ToString()));
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                counter++;
            }
            return true;
        }

        private bool InsertNextShortVersionBuildNumberInInfoPlistLineCollection()
        {
            if (_CFBundleShortVersionStringLineNumber != 0)
            {
                _infoPlistLineCollection[_CFBundleShortVersionStringLineNumber + 1] = string.Format("<string>{0}</string>", _nextBuildNumber.Trim());

                if (File.Exists(_logPath))
                {
                    using (StreamWriter sw = File.AppendText(_logPath))
                    {
                        foreach (var line in _infoPlistLineCollection)
                        {
                            sw.WriteLine(string.Format("     {0}", line.Trim()));
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool IdentifyCFBundleShortVersionStringLineNumber()
        {
            var counter = 0;
            foreach (var line in _infoPlistLineCollection)
            {
                if (line.Trim().Equals("<key>CFBundleShortVersionString</key>"))
                {
                    if (File.Exists(_logPath))
                    {
                        _CFBundleShortVersionStringLineNumber = counter;
                        using (StreamWriter sw = File.AppendText(_logPath))
                        {
                            sw.WriteLine(string.Format("     <key>CFBundleShortVersionString</key> exists at line {0}", _CFBundleShortVersionStringLineNumber.ToString()));
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                counter++;
            }
            return true;
        }

        private bool CreateInfoPlistLineCollection()
        {
            if (_infoPlistFileExists)
            {
                string line = string.Empty;
                System.IO.StreamReader file = new System.IO.StreamReader(InfoPlistPath);
                while ((line = file.ReadLine()) != null)
                {
                    _infoPlistLineCollection.Add(line);
                }

                file.Close();

                if (File.Exists(_logPath))
                {
                    using (StreamWriter sw = File.AppendText(_logPath))
                    {
                        sw.WriteLine(string.Format("     info.plist line collection created with {0} lines", _infoPlistLineCollection.Count.ToString()));
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool CheckInfoPlistPathExists()
        {
            if (File.Exists(_logPath))
            {
                using (StreamWriter sw = File.AppendText(_logPath))
                {
                    sw.WriteLine(string.Format("     info.plist Path declared as {0}", InfoPlistPath));
                }
            }
            else
            {
                return false;
            }


            if (File.Exists(InfoPlistPath))
            {
                _infoPlistFileExists = true;
                using (StreamWriter sw = File.AppendText(_logPath))
                {
                    sw.WriteLine(string.Format("     info.plist Path exists at {0}", InfoPlistPath));
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool DeriveNextBuildNumber()
        {
            if (_nextBuildNumberFilePathExists)
            {
                _nextBuildNumberSuffix = System.IO.File.ReadAllText(NextBuildNumberFilePath);
                _nextBuildNumber = string.Format("{0}.{1}", BuildNumberPrefix, _nextBuildNumberSuffix);
                using (StreamWriter sw = File.AppendText(_logPath))
                {
                    sw.WriteLine(string.Format("     NextBuildNumberSuffix is {0}", _nextBuildNumberSuffix));
                }
                using (StreamWriter sw = File.AppendText(_logPath))
                {
                    sw.WriteLine(string.Format("     NextBuildNumber is {0}", _nextBuildNumber));
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool CheckNextBuildNumberFilePathExists()
        {
            if (File.Exists(NextBuildNumberFilePath))
            {
                using (StreamWriter sw = File.AppendText(_logPath))
                {
                    sw.WriteLine(string.Format("     NextBuildNumberFilePath exists at {0}", NextBuildNumberFilePath));
                    _nextBuildNumberFilePathExists = true;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool LogAfterBuildFinished()
        {
            using (StreamWriter sw = File.AppendText(_logPath))
            {
                sw.WriteLine(string.Format("Before Build Finished for Project {0} at {1} UTC", ProjectName, DateTime.UtcNow.ToString()));
            }
            return true;
        }

        private bool LogBeforeBuildStarted()
        {
            using (StreamWriter sw = File.AppendText(_logPath))
            {
                sw.WriteLine(string.Format("Before Build Started for Project {0} at {1} UTC", ProjectName, DateTime.UtcNow.ToString()));
            }
            return true;
        }

        private bool CheckLogPathExists()
        {
            if (!System.IO.Directory.Exists(_logPath))
            {
                System.IO.Directory.CreateDirectory(LogDirectory);
            }
            if (!File.Exists(_logPath))
            {
                using (StreamWriter sw = File.CreateText(_logPath))
                {
                    sw.WriteLine(string.Format("Build Server Log File Created: {0} UTC", DateTime.UtcNow.ToString()));
                }
            }
            return true;
        }
    }
}
