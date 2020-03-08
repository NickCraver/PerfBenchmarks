using System.Net;
using BenchmarkDotNet.Attributes;
using Benchmarks.Libs;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class IPContainsTests
    {
        private static readonly IPAddress Containeev4 = IPAddress.Parse("127.0.0.1");
        private static readonly IPNet IPNetContainerv4 = IPNet.Parse("127.0.0.0/16");
        private static readonly IPNet IPNetContaineev4 = IPNet.Parse("127.0.0.1");
        private static readonly IPNetwork IPNetworkContainerv4 = IPNetwork.Parse("127.0.0.0/16");
        private static readonly IPNetwork IPNetworkContaineev4 = IPNetwork.Parse("127.0.0.1");

        private static readonly IPAddress Containeev6 = IPAddress.Parse("127.0.0.1");
        private static readonly IPNet IPNetContainerv6 = IPNet.Parse("::0/64");
        private static readonly IPNet IPNetContaineev6 = IPNet.Parse("::1");
        private static readonly IPNetwork IPNetworkContainerv6 = IPNetwork.Parse("::0/64");
        private static readonly IPNetwork IPNetworkContaineev6 = IPNetwork.Parse("::1");

        [Benchmark]
        public void IPNetContainsIPv4() => IPNetContainerv4.Contains(Containeev4);
        [Benchmark]
        public void IPNetContainsIPNetv4() => IPNetContainerv4.Contains(IPNetContaineev4);
        [Benchmark]
        public void IPNetwork2ContainsIPv4() => IPNetworkContainerv4.Contains(Containeev4);
        [Benchmark]
        public void IPNetwork2ContainsIPNetwork2v4() => IPNetworkContainerv4.Contains(IPNetworkContaineev4);

        [Benchmark]
        public void IPNetContainsIPv6() => IPNetContainerv6.Contains(Containeev6);
        [Benchmark]
        public void IPNetContainsIPNetv6() => IPNetContainerv6.Contains(IPNetContaineev6);
        [Benchmark]
        public void IPNetwork2ContainsIPv6() => IPNetworkContainerv6.Contains(Containeev6);
        [Benchmark]
        public void IPNetwork2ContainsIPNetwork2v6() => IPNetworkContainerv6.Contains(IPNetworkContaineev6);
    }
}
