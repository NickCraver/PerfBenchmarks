using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class RegexTests
    {
        private const string DoesntContainShort = "It takes 0.6s for both cases. If number of iterations in the external loop is increased 10 times the execution time increases 10 times too to 6s in both cases.";
        private const string DoesContainShort = @"It takes 0.6s for both cases. <a href=""https://www.amazon.com/dp/B0792K2BK6/"">If number of iterations</a> in the external loop is increased 10 times the execution time increases 10 times too to 6s in both cases.";
        private const string DoesntContainLong = "Just for the record. On Windows / VS2017 / i7-6700K 4GHz there is NO difference between two versions. It takes 0.6s for both cases. If number of iterations in the external loop is increased 10 times the execution time increases 10 times too to 6s in both cases.";
        private const string DoesContainLong = @"Just for the record. On Windows / VS2017 / i7-6700K 4GHz there is NO difference between two versions. It takes 0.6s for both cases. <a href=""https://www.amazon.com/dp/B0792K2BK6/"">If number of iterations</a> in the external loop is increased 10 times the execution time increases 10 times too to 6s in both cases.";
        private const string _amazonReplace = @"href=""https://mydomain.com/amzn/click/$1/$2"" rel=""nofollow noreferrer""";
        private static readonly Regex _amazonLink = new Regex(
            @"href=""" + // href part
            @"(?:https?:)?//(?:www\.)?(?:amazon|amzn)\.(com|co.uk|de|fr|es|it)/" + // valid domains we allow
            @"(?>" + // valid paths we allow
                @"[^""]+/dp/" +
                @"|d/Books/(?:[^""]+/)?" +
                @"|dp/" +
                @"|gp/reader/" + // will break the reader view
                @"|gp/aw/d/" +
                @"|gp/product/(?:toc/|product-description/|glance/)?" + // will break the non product page views
                @"|exec/obidos/ASIN/" +
                @"|exec/obidos/ISBN(?:=|%3D)" +
                @"|product-reviews/" +
                @"|[^""?]+\?(?:[^""]*\&)?(?:a|asin)=" +
            @")?" +
            @"(B0[A-Z0-9]{8}|\d{9}[\dX])" + // the exactly 10 character product identifier (capturing group)
            @"[^""]*""", // all of the garbage at the end
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // IsMatch + Replace
        [Benchmark(Description = "IsMatch+Replace: ContainsShort")]
        public string IsMatchReplaceContainsShort() => IsMatchReplace(DoesContainShort);
        [Benchmark(Description = "IsMatch+Replace: ContainsLong")]
        public string IsMatchReplaceContainsLong() => IsMatchReplace(DoesContainLong);
        [Benchmark(Description = "IsMatch+Replace: NoContainsShort")]
        public string IsMatchReplaceNoContainsShort() => IsMatchReplace(DoesntContainShort);
        [Benchmark(Description = "IsMatch+Replace: NoContainsLong")]
        public string IsMatchReplaceNoContainsLong() => IsMatchReplace(DoesntContainLong);

        private string IsMatchReplace(string safeHtml)
        {
            if (!_amazonLink.IsMatch(safeHtml)) return safeHtml;
            return _amazonLink.Replace(safeHtml, _amazonReplace);
        }

        // Sanity + Replace
        [Benchmark(Description = "Sanity+hReplace: ContainsShort")]
        public string SanityReplaceContainsShort() => SanityReplace(DoesContainShort);
        [Benchmark(Description = "Sanity+Replace: ContainsLong")]
        public string SanityReplaceContainsLong() => SanityReplace(DoesContainLong);
        [Benchmark(Description = "Sanity+Replace: NoContainsShort")]
        public string SanityReplaceNoContainsShort() => SanityReplace(DoesntContainShort);
        [Benchmark(Description = "Sanity+Replace: NoContainsLong")]
        public string SanityReplaceNoContainsLong() => SanityReplace(DoesntContainLong);

        private string SanityReplace(string safeHtml)
        {
            if (!safeHtml.Contains("amazon") && !safeHtml.Contains("amzn")) return DoesntContainLong;
            return _amazonLink.Replace(safeHtml, _amazonReplace);
        }
    }
}
