using System;
using BenchmarkDotNet.Attributes;
using LanguageDetection;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class LanguageDetectionTests
    {
        private static LanguageDetector EnglishDetector;
        private static LanguageDetector AllDetector;

        public LanguageDetectionTests()
        {
            var ed = new LanguageDetector();
            ed.AddLanguages("eng");
            EnglishDetector = ed;

            var ad = new LanguageDetector();
            ad.AddLanguages("spa", "fra", "deu", "jpn", "por", "ukr", "zho", "ita", "rus", "kor");
            AllDetector = ad;
        }

        //[Benchmark]
        public LanguageDetector EnglishLoad()
        {
            var d = new LanguageDetector();
            d.AddLanguages("eng");
            return d;
        }

        //[Benchmark]
        public LanguageDetector AllLoad()
        {
            var d = new LanguageDetector();
            d.AddLanguages("spa", "fra", "deu", "jpn", "por", "ukr", "zho", "ita", "rus", "kor");
            return d;
        }

        [Benchmark]
        public string EnglishOnlyDetect() => EnglishDetector.Detect("Schneller brauner Fuchs springt über den faulen Hund");

        [Benchmark]
        public string AllDetect() => AllDetector.Detect("Schneller brauner Fuchs springt über den faulen Hund");
    }
}
