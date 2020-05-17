using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;

using SlimCache = Benchmarks.Libs.MemoryCache;
using SysCache = System.Runtime.Caching.MemoryCache;
using ExtCache = Microsoft.Extensions.Caching.Memory.MemoryCache;
using System.Collections.Concurrent;

namespace Benchmarks
{
    [Config(typeof(Config))]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class MemoryCacheTests
    {
        // 3 sets of cache sizes
        public static ConcurrentDictionary<string, object> 
                                DictEmpty = new ConcurrentDictionary<string, object>(),
                                DictSm = new ConcurrentDictionary<string, object>(),
                                DictMd = new ConcurrentDictionary<string, object>(),
                                DictLg = new ConcurrentDictionary<string, object>();

        public static SlimCache SlimCacheEmpty = new SlimCache(new Libs.MemoryCacheOptions()),
                                SlimCacheSm = new SlimCache(new Libs.MemoryCacheOptions()),
                                SlimCacheMd = new SlimCache(new Libs.MemoryCacheOptions()),
                                SlimCacheLg = new SlimCache(new Libs.MemoryCacheOptions());

        public static SysCache SysCacheEmpty = new SysCache("Empty"),
                               SysCacheSm = new SysCache("Small"),
                               SysCacheMd = new SysCache("Medium"),
                               SysCacheLg = new SysCache("Large");

        public static ExtCache ExtCacheempty = new ExtCache(Options.Create(new MemoryCacheOptions())),
                               ExtCacheSm = new ExtCache(Options.Create(new MemoryCacheOptions())),
                               ExtCacheMd = new ExtCache(Options.Create(new MemoryCacheOptions())),
                               ExtCacheLg = new ExtCache(Options.Create(new MemoryCacheOptions()));
        public static DateTimeOffset Expires { get; } = DateTime.UtcNow.AddDays(7);
        public static DateTime ExpiresDt { get; } = DateTime.UtcNow.AddDays(7);

        static MemoryCacheTests()
        {
            for (int i = 0; i < 1000; i++)
            {
                var val = i.ToString();
                DictSm[val] = val;
                SlimCacheSm.Set(val, val, ExpiresDt);
                SysCacheSm.Add(val, val, Expires);
                ExtCacheSm.Set(val, val, Expires);
            }

            //for (int i = 0; i < 100_000; i++)
            //{
            //    var val = i.ToString();
            //    DictMd[val] = val;
            //    SlimCacheMd.Set(val, val, ExpiresDt);
            //    SysCacheMd.Add(val, val, Expires);
            //    ExtCacheMd.Set(val, val, Expires);
            //}

            //for (int i = 0; i < 1_000_000; i++)
            //{
            //    var val = i.ToString();
            //    DictLg[val] = val;
            //    SlimCacheLg.Set(val, val, ExpiresDt);
            //    SysCacheLg.Add(val, val, Expires);
            //    ExtCacheLg.Set(val, val, Expires);
            //}
        }

        [Benchmark, BenchmarkCategory("Get (string) - Small")]
        public string ConCurDictGetSm() => DictSm.TryGetValue("123", out var val) ? (string)val : null;

        [Benchmark, BenchmarkCategory("Get (object) - Small")]
        public object ConCurDictGetSmObject() => DictSm.TryGetValue("123", out var val) ? val : null;

        //[Benchmark, BenchmarkCategory("Get (string) - Medium")]
        //public string ConCurDictGetMd() => DictMd.TryGetValue("1234", out var val) ? (string)val : null;

        //[Benchmark, BenchmarkCategory("Get (string) - Large")]
        //public string ConCurDictGetLg() => DictLg.TryGetValue("12345", out var val) ? (string)val : null;

        [Benchmark, BenchmarkCategory("Set (string) - Small")]
        public void ConCurDictSetSm() => DictSm["1234"] = "1234";


        [Benchmark, BenchmarkCategory("Get (string) - Small")]
        public string SlimGetSm() => SlimCacheSm.Get<string>("123");

        [Benchmark, BenchmarkCategory("Get (object) - Small")]
        public object SlimGetSmObject() => SlimCacheSm.Get("123");

        //[Benchmark, BenchmarkCategory("Get (string) - Medium")]
        //public string SlimGetMd() => (string)SlimCacheMd.Get("1234");

        //[Benchmark, BenchmarkCategory("Get (string) - Large")]
        //public string SlimGetLg() => (string)SlimCacheLg.Get("12345");

        [Benchmark, BenchmarkCategory("Set (string) - Small")]
        public void SlimSetSm() => SlimCacheSm.Set("1234", "1234", ExpiresDt);


        [Benchmark(Baseline = true), BenchmarkCategory("Get (string) - Small")]
        public string SysGetSm() => (string)SysCacheSm.Get("123");

        [Benchmark(Baseline = true), BenchmarkCategory("Get (object) - Small")]
        public object SysGetSmObject() => SysCacheSm.Get("123");

        //[Benchmark(Baseline = true), BenchmarkCategory("Get (string) - Medium")]
        //public string SysGetMd() => (string)SysCacheMd.Get("1234");

        //[Benchmark(Baseline = true), BenchmarkCategory("Get (string) - Large")]
        //public string SysGetLg() => (string)SysCacheLg.Get("12345");

        [Benchmark(Baseline = true), BenchmarkCategory("Set (string) - Small")]
        public void SysSetSm() => SysCacheSm.Set("1234", "1234", Expires);


        [Benchmark, BenchmarkCategory("Get (string) - Small")]
        public string ExtGetSm() => ExtCacheSm.Get<string>("123");

        [Benchmark, BenchmarkCategory("Get (object) - Small")]
        public object ExtGetSmObject() => ExtCacheSm.Get("123");

        //[Benchmark, BenchmarkCategory("Get (string) - Medium")]
        //public string ExtGetMd() => ExtCacheMd.Get<string>("1234");

        //[Benchmark, BenchmarkCategory("Get (string) - Large")]
        //public string ExtGetLg() => ExtCacheLg.Get<string>("12345");

        [Benchmark, BenchmarkCategory("Set (string) - Small")]
        public void ExtSetSm() => ExtCacheSm.Set("1234", "1234", Expires);
    }
}
