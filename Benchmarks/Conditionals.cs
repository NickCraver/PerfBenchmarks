using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class ConditionalTests
    {
        private readonly List<string> _noItemsReadOnly = null;
        private readonly List<string> _itemsReadOnly = Enumerable.Range(0, 50000).Select(i => "Num" + i).ToList();
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

        [Benchmark]
        public bool NullCheckItemsReadOnly() => _itemsReadOnly != null && _itemsReadOnly.Count > 0;
        [Benchmark]
        public bool SafeNavigationItemsReadOnly() => _itemsReadOnly?.Count > 0;

        [Benchmark]
        public bool NullCheckNoItemsReadOnly() => _noItemsReadOnly != null && _noItemsReadOnly.Count > 0;
        [Benchmark]
        public bool SafeNavigationNoItemsReadOnly() => _noItemsReadOnly?.Count > 0;


        [Benchmark]
        public bool ItemsCountOnly() => _items.Count > 0;
        [Benchmark]
        public bool ItemsCountOnlyReadOnly() => _itemsReadOnly.Count > 0;
    }
}
