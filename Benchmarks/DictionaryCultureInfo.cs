using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class DictionaryCultureInfoTests
    {
        private static readonly CultureInfo english = CultureInfo.GetCultureInfo(1033);
        private static readonly Dictionary<CultureInfo, object> _byCultureInfo = CultureInfo.GetCultures(CultureTypes.AllCultures).ToDictionary(c => c, _ => new object());
        private static readonly Dictionary<string, object> _byCultureName = CultureInfo.GetCultures(CultureTypes.AllCultures).ToDictionary(c => c.Name, _ => new object());
        private static readonly object[] _byIdArray = new object[32_000];

        [GlobalSetup]
        public void Init()
        {
            for (var i = 0; i < _byIdArray.Length; i++)
            {
                _byIdArray[i] = new object();
            }
        }

        [Benchmark]
        public object CultureInfoLookup() => _byCultureInfo.TryGetValue(english, out var obj) ? obj : null;

        [Benchmark]
        public object StringLookup() => _byCultureName.TryGetValue("en-US", out var obj) ? obj : null;

        [Benchmark]
        public object ArrayLookup() => _byIdArray[1033];
    }
}
