using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class LINQLoopTests
    {
        private static readonly List<string> _items = Enumerable.Range(0, 50000).Select(i => "Num" + i).ToList();
        private readonly Consumer consumer = new Consumer();

        [Benchmark(Description = "foreach")]
        public void ForEach()
        {
            IEnumerable<string> Inner()
            {
                foreach (var item in _items)
                {
                    if (item.Length < 3)
                    {
                        yield return item;
                    }
                }
            }
            Inner().Consume(consumer);
        }

        [Benchmark(Description = "LINQ")]
        public void LINQ()
        {
            (from item in _items
             where item.Length < 3
             select item).Consume(consumer);
        }

        [Benchmark(Description = "LINQ (call form)")]
        public void LINQCallForm()
        {
            _items.Where(item => item.Length < 3).Select(item => item).Consume(consumer);
        }

        [Benchmark(Description = "LINQ (call form optimal)")]
        public void LINQCallFormOptimal()
        {
            _items.Where(item => item.Length < 3).Consume(consumer);
        }
    }
}
