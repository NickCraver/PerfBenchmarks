using BenchmarkDotNet.Attributes;
using System;
using System.Threading.Tasks;
using SlimCache = StackRedis.Internal.MemoryCache;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class MemoryCacheCollectorTests
    {
        private readonly SlimCache _cache = new SlimCache(new StackRedis.Internal.MemoryCacheOptions
        {
            ExpirationScanFrequency = TimeSpan.Zero, ExpirationScanYieldEveryItems = 250000 }
        );

        public int[] ContentsSizes => new[] { 10_000, 1_000_000, 10_000_000};
        [ParamsSource(nameof(ContentsSizes))]
        public int N { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _cache.Clear();
            var n = N;
            object el = new object();
            var now = DateTime.UtcNow;
            var future = now.AddDays(2);
            for (int i = 0; i < n; i++)
            {
                var expiry = future;
                _cache.Set("key" + i, el, expiry);
            }

        }

        [Benchmark]
        public Task CheckExpired() => _cache.ScanForExpiredItems().AsTask();
    }
}
