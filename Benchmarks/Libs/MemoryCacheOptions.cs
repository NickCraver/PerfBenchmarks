using Microsoft.Extensions.Options;
using System;

namespace StackRedis.Internal
{
    public sealed class MemoryCacheOptions : IOptions<MemoryCacheOptions>
    {
        /// <summary>
        /// Gets or sets the minimum length of time between successive scans for expired items.
        /// </summary>
        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromMinutes(2);
        public int ExpirationScanYieldEveryItems { get; set; } = 100000;

        MemoryCacheOptions IOptions<MemoryCacheOptions>.Value => this;
    }
}
