using System.Net;
using BenchmarkDotNet.Attributes;
using Benchmarks.Libs;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class IPParseTests
    {
        [Benchmark]
        public void IPNetParsev4() => IPNet.Parse("127.0.0.1");
        [Benchmark]
        public void IPNetCidrv4() => IPNet.Parse("127.0.0.1/16");
        [Benchmark]
        public void IPNetwork2v4() => IPNetwork.Parse("127.0.0.1");
        [Benchmark]
        public void IPNetwork2Cidrv4() => IPNetwork.Parse("127.0.0.1/24");

        [Benchmark]
        public void IPNetParsev6() => IPNet.Parse("::1");
        [Benchmark]
        public void IPNetCidrv6() => IPNet.Parse("::1/96");
        [Benchmark]
        public void IPNetwork2v6() => IPNetwork.Parse("::1");
        [Benchmark]
        public void IPNetwork2Cidrv6() => IPNetwork.Parse("::1/96");
    }
}
