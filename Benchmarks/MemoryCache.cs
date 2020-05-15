using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;

using SysCache = System.Runtime.Caching.MemoryCache;
using ExtCache = Microsoft.Extensions.Caching.Memory.MemoryCache;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class MemoryCacheTests
    {
        // 3 sets of cache sizes
        public static SysCache SysCacheEmpty = new SysCache("Empty"),
                               SysCacheSm = new SysCache("Small"),
                               SysCacheMd = new SysCache("Medium"),
                               SysCacheLg = new SysCache("Large");

        public static ExtCache ExtCacheempty = new ExtCache(Options.Create(new MemoryCacheOptions())),
                               ExtCacheSm = new ExtCache(Options.Create(new MemoryCacheOptions())),
                               ExtCacheMd = new ExtCache(Options.Create(new MemoryCacheOptions())),
                               ExtCacheLg = new ExtCache(Options.Create(new MemoryCacheOptions()));
        public static DateTimeOffset Expires { get; } = DateTime.UtcNow.AddDays(7);

        static MemoryCacheTests()
        {
            for (int i = 0; i < 1000; i++)
            {
                SysCacheSm.Add(i.ToString(), i.ToString(), Expires);
                ExtCacheSm.Set(i.ToString(), i.ToString(), Expires);
            }

            //for (int i = 0; i < 100_000; i++)
            //{
            //    SysCacheMd.Add(i.ToString(), i.ToString(), Expires);
            //    ExtCacheMd.Set(i.ToString(), i.ToString(), Expires);
            //}

            //for (int i = 0; i < 1_000_000; i++)
            //{
            //    SysCacheLg.Add(i.ToString(), i.ToString(), Expires);
            //    ExtCacheLg.Set(i.ToString(), i.ToString(), Expires);
            //}
        }

        [Benchmark(Description = "SysGet (string) - Small")]
        public string SysGetSm() => (string)SysCacheSm.Get("123");

        [Benchmark(Description = "SysGet (object) - Small")]
        public object SysGetSmObject() => SysCacheSm.Get("123");

        //[Benchmark(Description = "SysGet - Medium")]
        //public string SysGetMd() => (string)SysCacheMd.Get("1234");

        //[Benchmark(Description = "SysGet - Large")]
        //public string SysGetLg() => (string)SysCacheLg.Get("12345");


        [Benchmark(Description = "ExtGet (string) - Small")]
        public string ExtGetSm() => ExtCacheSm.Get<string>("123");

        [Benchmark(Description = "ExtGet (object) - Small")]
        public object ExtGetSmObject() => ExtCacheSm.Get("123");

        //[Benchmark(Description = "ExtGet - Medium")]
        //public string ExtGetMd() => ExtCacheMd.Get<string>("1234");

        //[Benchmark(Description = "ExtGet - Large")]
        //public string ExtGetLg() => ExtCacheLg.Get<string>("12345");
    }
}
