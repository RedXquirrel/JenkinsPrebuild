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

            Console.ReadLine();
        }

        private async Task Run()
        {
            await postBuild.Run();
        }
    }
}
