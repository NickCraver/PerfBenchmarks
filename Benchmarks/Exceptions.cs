using System;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
//using StackExchange.Exceptional;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class ExceptionTests
    {
        //private readonly StackTraceSettings _stackSettings = new StackTraceSettings();

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

        //[Benchmark]
        //public string RegexCrazy()
        //{
        //    var ex = new Exception().ToString();
        //    return ExceptionalUtils.StackTrace.HtmlPrettify(ex, _stackSettings);
        //}
    }
}
