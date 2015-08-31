using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI
{
    public class PostBuildConfigSettingsModel
    {
        public string ProjectName { get; set; }
        public string NextBuildNumberFilePath { get; set; }
        public string NextBuildNumber { get; set; }
        public string IPASourceFileName { get; set; }
        public string IPATargetFileName { get; set; }
        public string IPASourceDirectory { get; set; }
        public string IPATargetDirectory { get; set; }
    }
}
