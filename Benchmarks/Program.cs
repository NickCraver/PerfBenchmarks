using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ForeachTests>();
        }
    }

    class Config : ManualConfig
    {
        public Config()
        {
            //Add(new MemoryDiagnoser());
            //Add(new InliningDiagnoser());
        }
    }
}
