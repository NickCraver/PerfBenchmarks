using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;

using SlimCache = StackRedis.MemoryCache;
using SysCache = System.Runtime.Caching.MemoryCache;
using ExtCache = Microsoft.Extensions.Caching.Memory.MemoryCache;
using MemoryCacheOptions = Microsoft.Extensions.Caching.Memory.MemoryCacheOptions;
using System.Collections.Concurrent;
using StackRedis;
using System.Runtime.CompilerServices;

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

        public static SlimCache SlimCacheEmpty = new SlimCache(),
                                SlimCacheSm = new SlimCache(),
                                SlimCacheMd = new SlimCache(),
                                SlimCacheLg = new SlimCache();

        public static SysCache SysCacheEmpty = new SysCache("Empty"),
                               SysCacheSm = new SysCache("Small"),
                               SysCacheMd = new SysCache("Medium"),
                               SysCacheLg = new SysCache("Large");

        public static ExtCache ExtCacheempty = new ExtCache(Options.Create(new MemoryCacheOptions())),
                               ExtCacheSm = new ExtCache(Options.Create(new MemoryCacheOptions())),
                               ExtCacheMd = new ExtCache(Options.Create(new MemoryCacheOptions())),
                               ExtCacheLg = new ExtCache(Options.Create(new MemoryCacheOptions()));
        public static DateTimeOffset Expires { get; } = DateTime.UtcNow.AddDays(7);
        public static TimeSpan ExpiresTs { get; } = TimeSpan.FromDays(7);

        static MemoryCacheTests()
        {
            for (int i = 0; i < 1000; i++)
            {
                var val = i.ToString();
                DictSm[val] = val;
                DictSm["pre/" + val] = val;
                SlimCacheSm.Set(new PartitionedKey(val, ""), val, ExpiresTs);
                SlimCacheSm.Set(new PartitionedKey(val, "pre/"), val, ExpiresTs);
                SysCacheSm.Add(val, val, Expires);
                SysCacheSm.Add("pre/" + val, val, Expires);
                ExtCacheSm.Set(val, val, Expires);
                ExtCacheSm.Set("pre/" + val, val, Expires);
            }

            //for (int i = 0; i < 100_000; i++)
            //{
            //    var val = i.ToString();
            //    DictMd[val] = val;
            //    SlimCacheMd.Set(val, val, ExpiresTs);
            //    SysCacheMd.Add(val, val, Expires);
            //    ExtCacheMd.Set(val, val, Expires);
            //}

            //for (int i = 0; i < 1_000_000; i++)
            //{
            //    var val = i.ToString();
            //    DictLg[val] = val;
            //    SlimCacheLg.Set(val, val, ExpiresTs);
            //    SysCacheLg.Add(val, val, Expires);
            //    ExtCacheLg.Set(val, val, Expires);
            //}
        }

        [ParamsAllValues]
        public bool WithPartition { get; set; }


        private string Key(string key) => WithPartition ? ("pre/" + key) : key;

        private PartitionedKey PartitionedKey(string key) => new PartitionedKey(key, WithPartition ? "pre/" : "");

        [Benchmark, BenchmarkCategory("Get (string) - Small")]
        public string ConCurDictGetSm() => DictSm.TryGetValue(Key("123"), out var val) ? (string)val : null;

        [Benchmark, BenchmarkCategory("Get (object) - Small")]
        public object ConCurDictGetSmObject() => DictSm.TryGetValue(Key("123"), out var val) ? val : null;

        //[Benchmark, BenchmarkCategory("Get (string) - Medium")]
        //public string ConCurDictGetMd() => DictMd.TryGetValue("1234", out var val) ? (string)val : null;

        //[Benchmark, BenchmarkCategory("Get (string) - Large")]
        //public string ConCurDictGetLg() => DictLg.TryGetValue("12345", out var val) ? (string)val : null;

        [Benchmark, BenchmarkCategory("Set (string) - Small")]
        public void ConCurDictSetSm() => DictSm[Key("1234")] = "1234";


        [Benchmark, BenchmarkCategory("Get (string) - Small")]
        public string SlimGetSm() => SlimCacheSm.Get<string>(PartitionedKey("123"));

        [Benchmark, BenchmarkCategory("Get (object) - Small")]
        public object SlimGetSmObject() => SlimCacheSm.Get(PartitionedKey("123"));

        //[Benchmark, BenchmarkCategory("Get (string) - Medium")]
        //public string SlimGetMd() => (string)SlimCacheMd.Get("1234");

        //[Benchmark, BenchmarkCategory("Get (string) - Large")]
        //public string SlimGetLg() => (string)SlimCacheLg.Get("12345");

        [Benchmark, BenchmarkCategory("Set (string) - Small")]
        public void SlimSetSm() => SlimCacheSm.Set(PartitionedKey("1234"), "1234", ExpiresTs);


        [Benchmark(Baseline = true), BenchmarkCategory("Get (string) - Small")]
        public string SysGetSm() => (string)SysCacheSm.Get(Key("123"));

        [Benchmark(Baseline = true), BenchmarkCategory("Get (object) - Small")]
        public object SysGetSmObject() => SysCacheSm.Get(Key("123"));

        //[Benchmark(Baseline = true), BenchmarkCategory("Get (string) - Medium")]
        //public string SysGetMd() => (string)SysCacheMd.Get("1234");

        //[Benchmark(Baseline = true), BenchmarkCategory("Get (string) - Large")]
        //public string SysGetLg() => (string)SysCacheLg.Get("12345");

        [Benchmark(Baseline = true), BenchmarkCategory("Set (string) - Small")]
        public void SysSetSm() => SysCacheSm.Set(Key("1234"), "1234", Expires);


        [Benchmark, BenchmarkCategory("Get (string) - Small")]
        public string ExtGetSm() => ExtCacheSm.Get<string>(Key("123"));

        [Benchmark, BenchmarkCategory("Get (object) - Small")]
        public object ExtGetSmObject() => ExtCacheSm.Get(Key("123"));

        //[Benchmark, BenchmarkCategory("Get (string) - Medium")]
        //public string ExtGetMd() => ExtCacheMd.Get<string>("1234");

        //[Benchmark, BenchmarkCategory("Get (string) - Large")]
        //public string ExtGetLg() => ExtCacheLg.Get<string>("12345");

        [Benchmark, BenchmarkCategory("Set (string) - Small")]
        public void ExtSetSm() => ExtCacheSm.Set(Key("1234"), "1234", Expires);
    }
}
