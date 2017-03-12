using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<TrimTests>();
            //BenchmarkRunner.Run<TimeSpanTests>();
            //BenchmarkRunner.Run<JSONTests>();
            BenchmarkRunner.Run<ConditionalTests>();
        }
    }

    class Config : ManualConfig
    {
        public Config()
        {
            Add(new MemoryDiagnoser());
            //Add(new InliningDiagnoser());
        }
    }
}
