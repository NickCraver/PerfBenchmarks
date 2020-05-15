using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class BoolTests
    {
        [Benchmark]
        public string DotToString() => true.ToString();

        [Benchmark]
        public string TrueString() => bool.TrueString;
    }
}
