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

        [Required]
        public string BuildNumberPrefix { get; set; }

        [Required]
        public string NextBuildNumberFilePath { get; set; }

        [Required]
        public string InfoPlistPath { get; set; }

        private bool _nextBuildNumberFilePathExists;
        private string _nextBuildNumberSuffix;
        private string _nextBuildNumber;
        private bool _infoPlistFileExists;
        private int _CFBundleShortVersionStringLineNumber = 0;
        private int _CFBundleVersionLineNumber = 0;
        private List<string> _infoPlistLineCollection = new List<string>();

        public override bool Execute()
        {
            CheckLogPathExists();
            LogBeforeBuildStarted();
            CheckNextBuildNumberFilePathExists();
            DeriveNextBuildNumber();
            CheckInfoPlistPathExists();
            CreateInfoPlistLineCollection();
            IdentifyCFBundleShortVersionStringLineNumber();
            InsertNextShortVersionBuildNumberInInfoPlistLineCollection();
            IdentifyNextVersionBuildNumberInInfoPlistLineCollection();
            InsertNextVersionBuildNumberInInfoPlistLineCollection();

            if (File.Exists(InfoPlistPath))
            {
                _infoPlistFileExists = true;
                File.Delete(InfoPlistPath);
                File.WriteAllLines(InfoPlistPath, _infoPlistLineCollection.ToArray<string>());

                if (File.Exists(LogPath))
                {
                    using (StreamWriter sw = File.AppendText(LogPath))
                    {
                        sw.WriteLine(string.Format("     info.plist Path rewritten at {0}", LogPath));
                    }
                }
            }
            else
            {
                return false;
            }

            LogAfterBuildFinished();
            return true;
        }

        private void InsertNextVersionBuildNumberInInfoPlistLineCollection()
        {
            if (_CFBundleVersionLineNumber != 0)
            {
                _infoPlistLineCollection[_CFBundleVersionLineNumber + 1] = string.Format("<string>{0}</string>", BuildNumberPrefix.Trim());

                if (File.Exists(LogPath))
                {
                    using (StreamWriter sw = File.AppendText(LogPath))
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
        }

        private void IdentifyNextVersionBuildNumberInInfoPlistLineCollection()
        {
            var counter = 0;
            foreach (var line in _infoPlistLineCollection)
            {
                if (line.Trim().Equals("<key>CFBundleVersion</key>"))
                {
                    if (File.Exists(LogPath))
                    {
                        _CFBundleVersionLineNumber = counter;
                        using (StreamWriter sw = File.AppendText(LogPath))
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
        }

        private void InsertNextShortVersionBuildNumberInInfoPlistLineCollection()
        {
            if (_CFBundleShortVersionStringLineNumber != 0)
            {
                _infoPlistLineCollection[_CFBundleShortVersionStringLineNumber + 1] = string.Format("<string>{0}</string>", _nextBuildNumber.Trim());

                if (File.Exists(LogPath))
                {
                    using (StreamWriter sw = File.AppendText(LogPath))
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
        }

        private void IdentifyCFBundleShortVersionStringLineNumber()
        {
            var counter = 0;
            foreach (var line in _infoPlistLineCollection)
            {
                if (line.Trim().Equals("<key>CFBundleShortVersionString</key>"))
                {
                    if (File.Exists(LogPath))
                    {
                        _CFBundleShortVersionStringLineNumber = counter;
                        using (StreamWriter sw = File.AppendText(LogPath))
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
        }

        private void CreateInfoPlistLineCollection()
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

                if (File.Exists(LogPath))
                {
                    using (StreamWriter sw = File.AppendText(LogPath))
                    {
                        sw.WriteLine(string.Format("     info.plist line collection created with {0} lines", _infoPlistLineCollection.Count.ToString()));
                    }
                }
            }
            else
            {
                return false;
            }
        }

        private void CheckInfoPlistPathExists()
        {
            if (File.Exists(LogPath))
            {
                using (StreamWriter sw = File.AppendText(LogPath))
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
                using (StreamWriter sw = File.AppendText(LogPath))
                {
                    sw.WriteLine(string.Format("     info.plist Path exists at {0}", InfoPlistPath));
                }
            }
            else
            {
                return false;
            }

        }

        private void DeriveNextBuildNumber()
        {
            if (_nextBuildNumberFilePathExists)
            {
                _nextBuildNumberSuffix = System.IO.File.ReadAllText(NextBuildNumberFilePath);
                _nextBuildNumber = string.Format("{0}.{1}", BuildNumberPrefix, _nextBuildNumberSuffix);
                using (StreamWriter sw = File.AppendText(LogPath))
                {
                    sw.WriteLine(string.Format("     NextBuildNumberSuffix is {0}", _nextBuildNumberSuffix));
                }
                using (StreamWriter sw = File.AppendText(LogPath))
                {
                    sw.WriteLine(string.Format("     NextBuildNumber is {0}", _nextBuildNumber));
                }
            }
            else
            {
                return false;
            }

        }

        private void CheckNextBuildNumberFilePathExists()
        {
            if (File.Exists(NextBuildNumberFilePath))
            {
                using (StreamWriter sw = File.AppendText(LogPath))
                {
                    sw.WriteLine(string.Format("     NextBuildNumberFilePath exists at {0}", NextBuildNumberFilePath));
                    _nextBuildNumberFilePathExists = true;
                }
            }
            else
            {
                return false;
            }
        }

        private void LogAfterBuildFinished()
        {
            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine(string.Format("Before Build Finished for Project {0} at {1} UTC", ProjectName, DateTime.UtcNow.ToString()));
            }
        }

        private void LogBeforeBuildStarted()
        {
            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine(string.Format("Before Build Started for Project {0} at {1} UTC", ProjectName, DateTime.UtcNow.ToString()));
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
