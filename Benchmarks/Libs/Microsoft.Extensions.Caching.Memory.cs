// From https://raw.githubusercontent.com/dotnet/runtime/master/src/libraries/Microsoft.Extensions.Caching.Memory/src/
//
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmarks.Libs
{
    /// <summary>
    /// An implementation of <see cref="IMemoryCache"/> using a dictionary to
    /// store its entries.
    /// </summary>
    public class MemoryCache
    {
        static MemoryCache()
        {
            if (sizeof(long) > IntPtr.Size)
                throw new PlatformNotSupportedException("ECMA-335 §I.12.6.2 only guarantees not to tear things up to the word size; assertion failed");
        }

        private readonly ConcurrentDictionary<string, CacheEntry> _entries;
        private bool _disposed;

        private readonly MemoryCacheOptions _options;
        private DateTime _lastExpirationScan;

        /// <summary>
        /// Creates a new <see cref="MemoryCache"/> instance.
        /// </summary>
        /// <param name="optionsAccessor">The options of the cache.</param>
        public MemoryCache(IOptions<MemoryCacheOptions> optionsAccessor = null)
        {
            optionsAccessor ??= new MemoryCacheOptions();

            _options = optionsAccessor.Value;
            _entries = new ConcurrentDictionary<string, CacheEntry>();
            _lastExpirationScan = DateTime.UtcNow;

            var scanEvery = _options.ExpirationScanFrequency;
            if (scanEvery > TimeSpan.Zero)
            {
                _scanEveryTimer = new Timer(state =>
                {
                    if (state is WeakReference wr && wr.Target is MemoryCache obj)
                    {
                        _ = obj.ScanForExpiredItems();
                    }
                }, new WeakReference(this), scanEvery, scanEvery);
            }
        }
        private Timer _scanEveryTimer;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(string key, out object result)
        {
            // Don't remove the invalid entry live, let the process clean it up
            if (_entries.TryGetValue(key, out CacheEntry entry) && entry.TryGet(out result))
            {
                return true;
            }
            result = null;
            return false;
        }

        /// <inheritdoc />
        public bool Remove(string key) => _entries.TryRemove(key, out _);

        private void RemoveEntry(string key, CacheEntry entry)
            => EntriesCollection.Remove(new KeyValuePair<string, CacheEntry>(key, entry));

        int _scanInProgress;

        internal ValueTask<int> ScanForExpiredItems() // returns: -1 if already running (so: nothing done), else: the number of things removed
        {
            return Interlocked.CompareExchange(ref _scanInProgress, 1, 0) == 0 ? Impl(this) : new ValueTask<int>(-1);

            static async ValueTask<int> Impl(MemoryCache obj)
            {
                int count = 0, removed = 0;
                try
                {
                    var yieldEvery = obj._options.ExpirationScanYieldEveryItems;
                    foreach (var pair in obj._entries)
                    {
                        if (pair.Value.IsExpired())
                        {
                            obj.RemoveEntry(pair.Key, pair.Value);
                            removed++;
                        }
                        if ((++count % yieldEvery) == 0)
                            await Task.Yield();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    Interlocked.CompareExchange(ref obj._scanInProgress, 0, 1);
                }
                return removed;
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

                    try { _scanEveryTimer?.Dispose(); }
                    catch { }
                    _scanEveryTimer = null;
                }
                _disposed = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<TItem>(string key, TItem value, DateTime absoluteExpiration, TimeSpan slidingExpiration)
            => SetEntry(key, new CacheEntry(value, absoluteExpiration, slidingExpiration));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<TItem>(string key, TItem value, DateTime absoluteExpiration)
            => SetEntry(key, new CacheEntry(value, absoluteExpiration));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Get(string key)
        {
            TryGetValue(key, out object value);
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TItem Get<TItem>(string key)
            => TryGetValue(key, out object value) ? (TItem)value : default;

        internal void Clear() => _entries.Clear();
    }

    public sealed class CacheEntry
    {
        private long _absoluteExpirationTicks;
        private int _accessCount;
        private readonly uint _slidingSeconds;
        public object Value { get; }

        public int AccessCount => Volatile.Read(ref _accessCount);
        public DateTime AbsoluteExpiration => new DateTime(_absoluteExpirationTicks, DateTimeKind.Utc);

        private static long _currentDateIshTicks = DateTime.UtcNow.Ticks;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Timer yo.")]
        private static readonly Timer ExpirationTimeUpdater =
            new Timer(state => _currentDateIshTicks = DateTime.UtcNow.Ticks, null, 1000, 1000);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal CacheEntry(object value, DateTime absoluteExpiration)
        {
            Value = value;
            if (absoluteExpiration.Kind == DateTimeKind.Local) absoluteExpiration = absoluteExpiration.ToUniversalTime();
            _absoluteExpirationTicks = absoluteExpiration.Ticks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal CacheEntry(object value, DateTime absoluteExpiration, TimeSpan slidingExpiration) : this(value, absoluteExpiration)
        {
            if (slidingExpiration > TimeSpan.Zero)
            {
                _slidingSeconds = slidingExpiration.TotalSeconds >= uint.MaxValue
                    ? uint.MaxValue : (uint)slidingExpiration.TotalSeconds;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsExpired() => _absoluteExpirationTicks <= _currentDateIshTicks;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetExpired() => _absoluteExpirationTicks = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool TryGet(out object value)
        {
            value = Value;
            if (IsExpired()) return false;

            _accessCount++; // not concerned about losing occasional values due to threading
            var slide = _slidingSeconds;
            if (slide != 0) _absoluteExpirationTicks = _currentDateIshTicks + (slide * TimeSpan.TicksPerSecond);
            return true;
        }
    }

    public class MemoryCacheOptions : IOptions<MemoryCacheOptions>
    {
        /// <summary>
        /// Gets or sets the minimum length of time between successive scans for expired items.
        /// </summary>
        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromMinutes(2);
        public int ExpirationScanYieldEveryItems { get; set; } = 100000;

        MemoryCacheOptions IOptions<MemoryCacheOptions>.Value => this;
    }
}
