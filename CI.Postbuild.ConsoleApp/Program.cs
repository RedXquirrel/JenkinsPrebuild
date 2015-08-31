using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI.Postbuild.ConsoleApp
{
    class Program
    {
        static CIPostBuildTask postBuild;
        static void Main(string[] args)
        {
            postBuild = new CIPostBuildTask();
            Run();
            Console.ReadLine();
        }

        private static void Run()
        {
            postBuild.Run();
        }
    }
}
