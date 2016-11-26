using System;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class TimeSpanTests
    {
        [Benchmark]
        public TimeSpan GetTimeSpan() => TimeSpan.FromSeconds(30);

        [Benchmark]
        public TimeSpan GetTimeSpanConstructor() => new TimeSpan(30 * 600000000L);

        [Benchmark]
        public TimeSpan GetExtension() => 30.Seconds();

        [Benchmark]
        public int GetMs() => 30 * 1000;
    }

    public static class TimeExtensions
    {
        public static TimeSpan Seconds(this int i) => new TimeSpan(i * 600000000L);
    }
}
