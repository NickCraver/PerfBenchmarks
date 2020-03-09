using Benchmarks.Libs;
using System.Diagnostics;
using static System.Console;

namespace Benchmarks
{
    public static class Program
    {
        const int loops = 50_000_000;

        public static void Main(string[] args)
        {
            WriteLine("Press any key to begin");
            ReadKey();

            WriteLine("Running Loop ({0} iteations)...", loops);
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < loops; i++)
            {
                IPNet.Parse("127.0.0.1/16");
            }
            sw.Stop();
            WriteLine("Done: " + sw.ElapsedMilliseconds + " ms");
            //WriteLine("Press any key to exit");
            //ReadKey();
        }
    }
}
