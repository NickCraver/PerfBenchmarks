using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Benchmarks.Libs;
using System.Net;

namespace Benchmarks
{
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    [Config(typeof(Config))]
    public class IPNetSpanTests
    {
        private static readonly IPAddress LocalIp = IPAddress.Parse("127.0.0.1");
        [Benchmark, BenchmarkCategory("Orig")] public void IPAddressParsev4() => IPAddress.Parse("127.0.0.1");
        //[Benchmark, BenchmarkCategory("Orig")] public void IPAddressParsev6() => IPAddress.Parse("::1");

        [Benchmark, BenchmarkCategory("Orig")] public void IPAddressTryParsev4() => IPAddress.TryParse("127.0.0.1", out var _);
        //[Benchmark, BenchmarkCategory("Orig")] public void IPAddressTryParsev6() => IPAddress.TryParse("::1", out var _);

        //[Benchmark, BenchmarkCategory("Orig")] public void IPNetCostructor() => new IPNetOriginal(LocalIp, null);
        [Benchmark, BenchmarkCategory("Span")] public void IPNetSpanCostructor() => new IPNet(LocalIp);

        //[Benchmark, BenchmarkCategory("Orig")] public void IPNetParsev4() => IPNetOriginal.Parse("127.0.0.1");
        [Benchmark, BenchmarkCategory("Span")] public void IPNetSpanParsev4() => IPNet.Parse("127.0.0.1");
        //[Benchmark, BenchmarkCategory("Orig")] public void IPNetCidrv4() => IPNetOriginal.Parse("127.0.0.1/16");
        [Benchmark, BenchmarkCategory("Span")] public void IPNetSpanCidrv4() => IPNet.Parse("127.0.0.1/24");

        //[Benchmark, BenchmarkCategory("Orig")] public void IPNetParsev6() => IPNetOriginal.Parse("::1");
        [Benchmark, BenchmarkCategory("Span")] public void IPNetSpanParsev6() => IPNet.Parse("::1");
        //[Benchmark, BenchmarkCategory("Orig")] public void IPNetCidrv6() => IPNetOriginal.Parse("::1/96");
        [Benchmark, BenchmarkCategory("Span")] public void IPNetSpanCidrv6() => IPNet.Parse("::1/96");
    }
}
