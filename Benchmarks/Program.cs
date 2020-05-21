using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
            => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }

    internal class Config : ManualConfig
    {
        public Config()
        {
            Add(MemoryDiagnoser.Default);
            //Add(Job.Default.With(ClrRuntime.Net472));
            Add(Job.Default.With(CoreRuntime.Core31));
            //Add(new InliningDiagnoser());
        }
    }
}
