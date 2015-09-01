using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI.Postbuild.ConsoleApp
{
    class Program
    {
        //static CIPostBuildTask postBuild;

        static void Main(string[] args)
        {
            var task = Task.Run((Func<Task>)Program.Run);
            task.Wait();

            //postBuild = new CIPostBuildTask();
            //Run();
            //Console.ReadLine();
        }

        static async Task Run()
        {
            CIPostBuildTask postBuild = new CIPostBuildTask();
            await postBuild.Run();
        }

        //private static void Run()
        //{
        //    postBuild.Run();
        //}
    }
}
