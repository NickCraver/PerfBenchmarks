using System;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class LookupTests
    {
        private static readonly int[] validSizes = { 15, 30, 50 };

        [Params(null, 1, 15, 30, 50)]
        public int? Size { get; set; }

        [Benchmark(Description = "Array.IndexOf")]
        public int? ArrayIndex() => Size.HasValue && Array.IndexOf(validSizes, Size.Value) > -1 ? Size.Value : (int?)null;

        [Benchmark(Description = "switch")]
        public int? SwitchLookup()
        {
            switch (Size)
            {
                case 15:
                case 30:
                case 50:
                    return Size;
                default:
                    return null;
            }
        }
    }
}
