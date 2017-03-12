using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class ConditionalTests
    {
        private List<string> _noItems = null;
        private List<string> _items = Enumerable.Range(0, 50000).Select(i => "Num" + i).ToList();

        [Benchmark]
        public bool NullCheckItems() => _items != null && _items.Count > 0;
        [Benchmark]
        public bool SafeNavigationItems() => _items?.Count > 0;

        [Benchmark]
        public bool NullCheckNoItems() => _noItems != null && _noItems.Count > 0;
        [Benchmark]
        public bool SafeNavigationNoItems() => _noItems?.Count > 0;
    }
}
