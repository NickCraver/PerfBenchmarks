using System;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class LambdaTests
    {
        [Benchmark(Baseline = true)]
        public void Direct() => Console.Write("");

        [Benchmark]
        public void ViaLambda() => Step(() => Console.Write(""));

        private static void Step(Action action) => action();
    }
}
