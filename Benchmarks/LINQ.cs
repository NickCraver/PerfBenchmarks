using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class LINQTests
    {
        private readonly List<string> _items = Enumerable.Range(0, 50000).Select(i => "Num" + i).ToList();

        [Benchmark(Description = ".Count()")]
        public void Count()
        {
            var i = _items.Count();
        }

        [Benchmark(Description = ".Count")]
        public void RawCount()
        {
            var i = _items.Count;
        }
    }
}
