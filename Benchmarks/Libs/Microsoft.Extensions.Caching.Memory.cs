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

        private void SetEntry(string key, CacheEntry entry)
        {
            if (_entries.TryGetValue(key, out CacheEntry priorEntry))
            {
                priorEntry.SetExpired(EvictionReason.Replaced);
            }

            if (!entry.CheckExpired())
            {
                bool entryAdded;
                if (priorEntry == null)
                {
                    // Try to add the new entry if no previous entries exist.
                    entryAdded = _entries.TryAdd(key, entry);
                }
                else
                {
                    // Try to update with the new entry if a previous entries exist.
                    entryAdded = _entries.TryUpdate(key, entry, priorEntry);

                    if (!entryAdded)
                    {
                        // The update will fail if the previous entry was removed after retrival.
                        // Adding the new entry will succeed only if no entry has been added since.
                        // This guarantees removing an old entry does not prevent adding a new entry.
                        entryAdded = _entries.TryAdd(key, entry);
                    }
                }

                if (!entryAdded)
                {
                    entry.SetExpired(EvictionReason.Replaced);
                }
            }
            else
            {
                if (priorEntry != null)
                {
                    RemoveEntry(key, priorEntry);
                }
            }

            // TODO: Threadpool background cache purging
            //StartScanForExpiredItems();
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out object result)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));

            if (_entries.TryGetValue(key, out CacheEntry entry))
            {
                // Check if expired due to expiration tokens, timers, etc. and if so, remove it.
                // Allow a stale Replaced value to be returned due to concurrent calls to SetExpired during SetEntry.

                if (entry.CheckExpired() && entry.EvictionReason != EvictionReason.Replaced)
                {
                    // TODO: For efficiency queue this up for batch removal
                    RemoveEntry(key, entry);
                }
                else
                {
                    entry.AccessCount++;
                    result = entry.Value;
                    return true;
                }
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
                entry.SetExpired(EvictionReason.Removed);
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
                if (pair.Value.CheckExpired(now))
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
            var entry = new CacheEntry
            {
                AbsoluteExpiration = absoluteExpiration,
                Value = value,
            };
            SetEntry(key, entry);

            return value;
        }

        public object Get(string key) => TryGetValue(key, out object value) ? value : null;
        public TItem Get<TItem>(string key) => TryGetValue(key, out object value) ? (TItem)value : default;
    }

    public class CacheEntry
    {
        public object Value { get; internal set; }
        public DateTime AbsoluteExpiration { get; internal set; }
        internal int AccessCount;
        internal EvictionReason EvictionReason { get; private set; }
        private bool _isExpired;

        internal CacheEntry() { }

        internal bool CheckExpired() => _isExpired || CheckForExpiredTime(DateTime.UtcNow);
        internal bool CheckExpired(DateTime now) => _isExpired || CheckForExpiredTime(now);

        internal void SetExpired(EvictionReason reason)
        {
            if (EvictionReason == EvictionReason.None)
            {
                EvictionReason = reason;
            }
            _isExpired = true;
        }

        private bool CheckForExpiredTime(DateTime now)
        {
            if (AbsoluteExpiration <= now)
            {
                SetExpired(EvictionReason.Expired);
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

    public enum EvictionReason : byte
    {
        None,

        /// <summary>
        /// Manually
        /// </summary>
        Removed,

        /// <summary>
        /// Overwritten
        /// </summary>
        Replaced,

        /// <summary>
        /// Timed out
        /// </summary>
        Expired,/*

        /// <summary>
        /// Overflow
        /// </summary>
        Capacity,*/
    }
}
