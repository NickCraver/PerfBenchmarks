using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class FindTests
    {
        private readonly List<string> _items = Enumerable.Range(0, 50000).Select(i => "Num" + i).ToList();

        [Benchmark]
        public void Find()
        {
            var i = _items.Find(s => s == "Num400");
        }

        [Benchmark]
        public void FirstOrDefault()
        {
            var i = _items.FirstOrDefault(s => s == "Num400");
        }
    }
}
