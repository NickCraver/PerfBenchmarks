using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class InterpolationTests
    {
        private readonly int num = 5;
        private readonly string str = "test";

        private readonly int num2 = 5;
        private readonly string str2 = "test";
        private readonly string str3 = "test";
        private readonly string str4 = "test";

        [Benchmark]
        public string IntBoxing() => $"Some {num} thing {str}";
        [Benchmark]
        public string IntToString() => $"Some {num.ToString()} thing {str}";

        [Benchmark]
        public string IntBoxingMoreArgs() => $"Some {num} thing {str} {num2} {str2} {str3} {str4}";
        [Benchmark]
        public string IntToStringMoreArgs() => $"Some {num.ToString()} thing {str} {num2.ToString()} {str2} {str3} {str4}";
    }
}
