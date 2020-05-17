// From https://raw.githubusercontent.com/dotnet/runtime/master/src/libraries/Microsoft.Extensions.Caching.Memory/src/
//
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Benchmarks.Libs
{
    /// <summary>
    /// An implementation of <see cref="IMemoryCache"/> using a dictionary to
    /// store its entries.
    /// </summary>
    public class MemoryCache
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _entries;
        private bool _disposed;

        private readonly MemoryCacheOptions _options;
        private DateTime _lastExpirationScan;

        /// <summary>
        /// Creates a new <see cref="MemoryCache"/> instance.
        /// </summary>
        /// <param name="optionsAccessor">The options of the cache.</param>
        public MemoryCache(IOptions<MemoryCacheOptions> optionsAccessor)
        {
            _ = optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor));

            _options = optionsAccessor.Value;
            _entries = new ConcurrentDictionary<string, CacheEntry>();
            _lastExpirationScan = DateTime.UtcNow;
        }

        /// <summary>
        /// Cleans up the background collection events.
        /// </summary>
        ~MemoryCache() => Dispose(false);

        /// <summary>
        /// Gets the count of the current entries for diagnostic purposes.
        /// </summary>
        public int Count => _entries.Count;

        private ICollection<KeyValuePair<string, CacheEntry>> EntriesCollection => _entries;

        private void SetEntry(string key, CacheEntry entry) => _entries[key] = entry;

        /// <inheritdoc />
        public bool TryGetValue(string key, out object result)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            // Don't remove the invalid entry live, let the process clean it up
            if (_entries.TryGetValue(key, out CacheEntry entry) && !entry.IsExpired())
            {
                entry.AccessCount++;
                result = entry.Value;
                return true;
            }

            result = null;
            return false;
        }

        /// <inheritdoc />
        public void Remove(string key)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));

            if (_entries.TryRemove(key, out CacheEntry entry))
            {
                entry.SetExpired();
            }

            StartScanForExpiredItems();
        }

        private void RemoveEntry(string key, CacheEntry entry) => 
            EntriesCollection.Remove(new KeyValuePair<string, CacheEntry>(key, entry));

        // Called by multiple actions to see how long it's been since we last checked for expired items.
        // If sufficient time has elapsed then a scan is initiated on a background task.
        private void StartScanForExpiredItems()
        {
            var now = DateTime.UtcNow;
            if (_options.ExpirationScanFrequency < now - _lastExpirationScan)
            {
                _lastExpirationScan = now;
                Task.Factory.StartNew(state => ScanForExpiredItems((MemoryCache)state), this,
                    CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            }
        }

        private static void ScanForExpiredItems(MemoryCache cache)
        {
            var now = DateTime.UtcNow;
            foreach (var pair in cache._entries)
            {
                if (pair.Value.IsExpired(now))
                {
                    cache.RemoveEntry(pair.Key, pair.Value);
                }
            }
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
                _disposed = true;
            }
        }

        // From Extensions
        public TItem Set<TItem>(string key, TItem value, DateTime absoluteExpiration)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            var entry = new CacheEntry(value, absoluteExpiration);
            SetEntry(key, entry);

            return value;
        }

        public object Get(string key) => TryGetValue(key, out object value) ? value : null;
        public TItem Get<TItem>(string key) => TryGetValue(key, out object value) ? (TItem)value : default;
    }

    public class CacheEntry
    {
        public object Value { get; }
        public DateTime AbsoluteExpiration { get; }
        internal int AccessCount;
        private bool _isExpired;

        internal CacheEntry(object value, DateTime absoluteExpiration) =>
            (Value, AbsoluteExpiration) = (value, absoluteExpiration);

        internal void SetExpired() => _isExpired = true;
        internal bool IsExpired() => _isExpired || CheckForExpiredTime(DateTime.UtcNow);
        internal bool IsExpired(DateTime now) => _isExpired || CheckForExpiredTime(now);

        private bool CheckForExpiredTime(DateTime now)
        {
            if (AbsoluteExpiration <= now)
            {
                SetExpired();
                return true;
            }
            return false;
        }
    }

    public class MemoryCacheOptions : IOptions<MemoryCacheOptions>
    {
        /// <summary>
        /// Gets or sets the minimum length of time between successive scans for expired items.
        /// </summary>
        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromMinutes(2);

        MemoryCacheOptions IOptions<MemoryCacheOptions>.Value => this;
    }
}
