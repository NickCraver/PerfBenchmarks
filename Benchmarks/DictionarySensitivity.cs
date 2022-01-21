using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class DictionarySensitivityTests
    {
		private static readonly HashSet<string> _hashSensitive = new(StringComparer.Ordinal);
		private static readonly HashSet<string> _hashInsensitive = new(StringComparer.OrdinalIgnoreCase);
		private static readonly Dictionary<string, string> _dictSensitive = new(StringComparer.Ordinal);
		private static readonly Dictionary<string, string> _dictInsensitive = new(StringComparer.OrdinalIgnoreCase);

		[Params("foo0", "foo5", "foo50")]
		public string Key { get; set; }

		[GlobalSetup]
		public void Setup()
		{
			for (var i = 0; i < 10000; i++)
			{
				var key = "foo" + i.ToString();
				var value = "Hey There:" + i.ToString();
				_hashSensitive.Add(key);
				_hashInsensitive.Add(key);
				_dictSensitive[key] = value;
				_dictInsensitive[key] = value;
			}
		}

		[Benchmark(Description = "HashSet (Sensitive)")]
		public bool HashCaseSensitive() => _hashSensitive.Contains(Key);

		[Benchmark(Description = "HashSet (Insensitive)")]
		public bool HashCaseInsensitive() => _hashInsensitive.Contains(Key);

		[Benchmark(Description = "Dictionary (Sensitive)")]
		public string DictCaseSensitive() => _dictSensitive[Key];

		[Benchmark(Description = "Dictionary (Insensitive)")]
		public string CaseInsensitive() => _dictInsensitive[Key];
	}
}
