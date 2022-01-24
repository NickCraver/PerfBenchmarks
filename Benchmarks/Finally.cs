using System;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class FinallyTests
	{
		[Benchmark]
		public int NoFinally()
		{
			int a = 0;
			try
			{
				a = 2;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			return a;
		}

		[Benchmark]
		public int EmptyFinally()
		{
			int a = 0;
			try
			{
				a = 2;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			finally
			{
				NotPresentMethod();
			}
			return a;
		}

		[Conditional("ADKHADAJSKDA")]
		private void NotPresentMethod() { }
	}
}
