using System;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class ExceptionTests
    {
        [Benchmark(Baseline = true)]
        public string Baseline()
        {
            var ex = new Exception();
            return ex.ToString();
        }

        [Benchmark]
        public string Demystify()
        {
            var ex = new Exception().Demystify();
            return ex.ToString();
        }
    }
}
