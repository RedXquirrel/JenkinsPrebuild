using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CI.Postbuild.ConsoleApp
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
            await postBuild.Run();
        }
    }
}
