using System;
using System.Collections;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class EnvironmentTests
	{
		private EnvironmentCache PropCache;
		private IDictionary Cached;

		[GlobalSetup]
		public void Setup()
		{
			var currentVars = Environment.GetEnvironmentVariables();
			Cached = currentVars;
			PropCache = new EnvironmentCache(currentVars);
		}

		[Benchmark]
		public string GetEnvironmentVariable() => Environment.GetEnvironmentVariable("TEMP");

		[Benchmark]
		public string Dictionary() => Cached["TEMP"] as string;

		[Benchmark]
		public string Prop() => PropCache.TempDirectory;

		[Benchmark]
		public string PropLazy() => PropCache.TempDirectoryLazy;

		public class EnvironmentCache
		{
			private readonly IDictionary Cached;
			public EnvironmentCache(IDictionary variables)
			{
				Cached = variables;
				TempDirectory = Get("TEMP");
			}

			public string TempDirectory { get; }

			private string _tempDirectory;
			public string TempDirectoryLazy => _tempDirectory ??= Get("TEMP");

			public string Get(string variableName) => Cached[variableName] as string;
		}
	}
}
