using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarks
{
    /// <summary>
    /// This test shows that the JIT can eliminate (turn into consts) static readonly
    /// But it cannot if you put in an explicit static constructor instead it has to make
    /// a check on each call that the constructor has already run
    /// </summary>
    [Config(typeof(Config))]
    public class StaticConstructorsTests
    {
        const int Operations = 10000000;

        [Benchmark(OperationsPerInvoke = Operations)]
        public int WithStaticConstructor()
        {
            int returnValue = 0;
            for(int i = 0; i < Operations;i++)
            {
                returnValue += Constructor.Number;
            }
            return returnValue;
        }

        [Benchmark(Baseline =true, OperationsPerInvoke = Operations)]
        public int WithoutStaticConstructor()
        {
            int returnValue = 0;
            for (int i = 0; i < Operations; i++)
            {
                returnValue += NoConstructor.Number;
            }
            return returnValue;
        }

        public static class Constructor
        {
            static Constructor()
            {
                Number = Environment.ProcessorCount;
            }

            public static readonly int Number;
        }

        public static class NoConstructor
        {
            public static readonly int Number = Environment.ProcessorCount;
        }

    }
}
