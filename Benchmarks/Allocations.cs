using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class AllocationTests
    {
        [Benchmark(Description = "HashSet<string>")]
        public HashSet<string> HashSetString() => new HashSet<string>();

        [Benchmark]
        public StringBuilder StringBuilder() => new StringBuilder();
    }
}
