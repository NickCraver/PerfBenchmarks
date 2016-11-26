using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class ForeachTests
    {
        private readonly List<string> _myThings = null;

        [Benchmark]
        public void IfStatement()
        {
            if (_myThings != null)
            {
                foreach (var thing in _myThings)
                {
                    Console.Write(thing);
                }
            }
        }

        [Benchmark]
        public void EnumerableEmpty()
        {
            foreach (var thing in _myThings ?? Enumerable.Empty<string>())
            {
                Console.Write(thing);
            }
        }
    }
}
