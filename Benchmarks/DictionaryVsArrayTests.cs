using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class DictionaryVsArrayTests
    {
        [Params(5, 10, 20, 50, 100)]
        public int ItemsInList { get; set; }

        private Dictionary<int, LittleClass> _dictionary;
        private LittleClass[] _array;
        private const int Iterations = 100000;

        [GlobalSetup]
        public void Setup()
        {
            _dictionary = new Dictionary<int, LittleClass>();
            _array = new LittleClass[ItemsInList];
            for (int i = 0; i < ItemsInList; i++)
            {
                var c = new LittleClass() { Value = (ushort)(i + 0x0300) };
                _array[i] = c;
                _dictionary[c.Value] = c;
            }
        }

        [Benchmark(OperationsPerInvoke = Iterations, Baseline = true)]
        public int Dictionary()
        {
            int count = 0;
            for (int i = 0; i < Iterations; i++)
            {
                var val = _dictionary[((i % ItemsInList) + 0x0300)].Value;
                count++;
            }
            return count;
        }

        [Benchmark(OperationsPerInvoke = Iterations)]
        public int Array()
        {
            int count = 0;
            var a = _array;
            for (int i = 0; i < Iterations; i++)
            {
                var lookup = ((i % ItemsInList) + 0x0300);
                for (int x = 0; x < a.Length; x++)
                {
                    if (a[x].Value == lookup)
                    {
                        var val = a[x].Value;
                        count++;
                        break;
                    }
                }
            }
            return count;
        }

        [Benchmark(OperationsPerInvoke = Iterations)]
        public int ArrayBackwards()
        {
            int count = 0;
            var a = _array;
            for (int i = Iterations - 1; i >= 0; i--)
            {
                var lookup = ((i % ItemsInList) + 0x0300);
                for (int x = 0; x < a.Length; x++)
                {
                    if (a[x].Value == lookup)
                    {
                        var val = a[x].Value;
                        count++;
                        break;
                    }
                }
            }
            return count;
        }

        public class LittleClass
        {
            public int Value { get; set; }
        }
    }
}
