// From https://raw.githubusercontent.com/dotnet/runtime/master/src/libraries/Microsoft.Extensions.Caching.Memory/src/
//
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace StackRedis.Internal
{
    /// <summary>
    /// An implementation of <see cref="IMemoryCache"/> using a dictionary to
    /// store its entries.
    /// </summary>
    public sealed class MemoryCache
    {
        static MemoryCache()
        {
            if (sizeof(long) > IntPtr.Size)
                throw new PlatformNotSupportedException("Please use x64 mode. ECMA-335 §I.12.6.2 only guarantees not to tear things up to the word size.");
        }

        private readonly ConcurrentDictionary<PartitionedKey, CacheEntry> _entries;
        private volatile bool _disposed;

        private readonly MemoryCacheOptions _options;
        private long _lastExpirationScan;
        public DateTime LastExpirationScan => new DateTime(_lastExpirationScan, DateTimeKind.Utc);

        /// <summary>
        /// Creates a new <see cref="MemoryCache"/> instance.
        /// </summary>
        /// <param name="optionsAccessor">The options of the cache.</param>
        public MemoryCache(IOptions<MemoryCacheOptions> optionsAccessor = null)
        {
            optionsAccessor ??= new MemoryCacheOptions();

            _options = optionsAccessor.Value;
            _entries = new ConcurrentDictionary<PartitionedKey, CacheEntry>();
            _lastExpirationScan = CacheEntry.CurrentDateIshTicks;

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

        private ICollection<KeyValuePair<PartitionedKey, CacheEntry>> EntriesCollection => _entries;

        private void SetEntry(in PartitionedKey key, CacheEntry entry) => _entries[key] = entry;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in PartitionedKey key, out object result)
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
        public bool Remove(in PartitionedKey key) => _entries.TryRemove(key, out _);

        private void RemoveEntry(in PartitionedKey key, CacheEntry entry)
            => EntriesCollection.Remove(new KeyValuePair<PartitionedKey, CacheEntry>(key, entry));

        int _scanInProgress;

        internal ValueTask<int> ScanForExpiredItems() // returns: -1 if already running (so: nothing done), else: the number of things removed
        {
            return !_disposed && Interlocked.CompareExchange(ref _scanInProgress, 1, 0) == 0 ? Impl(this) : new ValueTask<int>(-1);

            static async ValueTask<int> Impl(MemoryCache obj)
            {
                int count = 0, removed = 0;
                obj._lastExpirationScan = CacheEntry.CurrentDateIshTicks;
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
                        {
                            await Task.Yield();
                            if (obj._disposed) break;
                        }
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

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    GC.SuppressFinalize(this);

                    try { _scanEveryTimer?.Dispose(); }
                    catch { }
                    _scanEveryTimer = null;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<TItem>(PartitionedKey key, TItem value, DateTime absoluteExpiration, TimeSpan slidingExpiration)
            => SetEntry(key, new CacheEntry(value, absoluteExpiration, slidingExpiration));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<TItem>(PartitionedKey key, TItem value, DateTime absoluteExpiration)
            => SetEntry(key, new CacheEntry(value, absoluteExpiration));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<TItem>(PartitionedKey key, TItem value, TimeSpan absoluteExpiration, TimeSpan slidingExpiration)
            => SetEntry(key, new CacheEntry(value, absoluteExpiration, slidingExpiration));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<TItem>(PartitionedKey key, TItem value, TimeSpan absoluteExpiration)
            => SetEntry(key, new CacheEntry(value, absoluteExpiration));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Get(PartitionedKey key)
        {
            TryGetValue(key, out object value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TItem Get<TItem>(PartitionedKey key)
            => TryGetValue(key, out object value) ? (TItem)value : default;

        internal void Clear() => _entries.Clear();

        public bool Rename(PartitionedKey from, PartitionedKey to)
        {
            if (from == to) return false;
            if (_entries.TryGetValue(from, out CacheEntry entry) && !entry.IsExpired())
            {
                _entries[to] = entry;
                RemoveEntry(from, entry);
            }
            return false;
        }

        public IEnumerable<KeyValuePair<PartitionedKey, CacheEntry>> GetAllEntries()
        {
            foreach (var pair in _entries)
            {
                if (!pair.Value.IsExpired())
                    yield return pair;
            }
        }

        public IEnumerable<KeyValuePair<string, CacheEntry>> GetEntries(string partition)
        {
            if (partition == null) throw new ArgumentNullException(nameof(partition));
            foreach (var pair in _entries)
            {
                if (pair.Key.Partition == partition && !pair.Value.IsExpired())
                    yield return new KeyValuePair<string, CacheEntry>(pair.Key.Key, pair.Value);
            }
        }
    }

    public readonly struct PartitionedKey : IEquatable<PartitionedKey>
    {
        public PartitionedKey(string key, string partition = "")
        {
            if (key == null) ThrowNullKey();
            Partition = partition ?? "";
            Key = key;
            static void ThrowNullKey() => throw new ArgumentNullException(nameof(key));
        }

        public string Partition { get; }
        public string Key { get; }

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is PartitionedKey typed && Equals(in typed);
        /// <inheritdoc/>
        public override string ToString() => Partition + Key;
        /// <inheritdoc/>
        public override int GetHashCode()
            => Key == null  ? 0 : (Partition.GetHashCode() ^ Key.GetHashCode());
        public bool Equals(in PartitionedKey other) => Partition == other.Partition && Key == other.Key;
        bool IEquatable<PartitionedKey>.Equals(PartitionedKey other) => Partition == other.Partition && Key == other.Key;

        public static bool operator == (in PartitionedKey x, in PartitionedKey y) => x.Partition == y.Partition && x.Key == y.Key;
        public static bool operator != (in PartitionedKey x, in PartitionedKey y) => x.Partition != y.Partition || x.Key != y.Key;

        public static implicit operator PartitionedKey(string key) => new PartitionedKey(key, null);
    }

    public sealed class CacheEntry
    {
        private long _absoluteExpirationTicks;
        private int _accessCount;
        private readonly uint _slidingSeconds;
        public object Value { get; }

        public int AccessCount => Volatile.Read(ref _accessCount);
        public DateTime AbsoluteExpiration => new DateTime(_absoluteExpirationTicks, DateTimeKind.Utc);

        private static long s_currentDateIshTicks = DateTime.UtcNow.Ticks;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Timer yo.")]
        private static readonly Timer ExpirationTimeUpdater =
            new Timer(state => s_currentDateIshTicks = DateTime.UtcNow.Ticks, null, 1000, 1000);

        internal static long CurrentDateIshTicks
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_currentDateIshTicks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal CacheEntry(object value, DateTime absoluteExpiration)
        {
            Value = value;
            _absoluteExpirationTicks = GetTicks(absoluteExpiration);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal CacheEntry(object value, TimeSpan absoluteExpiration)
        {
            Value = value;
            _absoluteExpirationTicks = GetTicks(absoluteExpiration);
        }

        private static long GetTicks(DateTime expiration)
        {
            if (expiration == DateTime.MaxValue) return long.MaxValue;
            if (expiration.Kind == DateTimeKind.Local) expiration = expiration.ToUniversalTime();
            if (expiration == DateTime.MaxValue) return long.MaxValue;
            return expiration.Ticks;
        }

        private static long GetTicks(TimeSpan expiration)
        {
            if (expiration == TimeSpan.MaxValue) return long.MaxValue;
            return s_currentDateIshTicks + expiration.Ticks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal CacheEntry(object value, DateTime absoluteExpiration, TimeSpan slidingExpiration)
            : this(value, absoluteExpiration)
            => _slidingSeconds = GetSlidingSeconds(slidingExpiration);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal CacheEntry(object value, TimeSpan absoluteExpiration, TimeSpan slidingExpiration)
            : this(value, absoluteExpiration)
            => _slidingSeconds = GetSlidingSeconds(slidingExpiration);

        private uint GetSlidingSeconds(TimeSpan value)
        {   // note: don't enable sliding if it never expires (expiration == long.MaxValue)
            if (_absoluteExpirationTicks != long.MaxValue && value > TimeSpan.Zero)
            {
                return value.TotalSeconds >= uint.MaxValue
                    ? uint.MaxValue : (uint)value.TotalSeconds;
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsExpired() => _absoluteExpirationTicks <= s_currentDateIshTicks;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetExpired() => _absoluteExpirationTicks = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool TryGet(out object value)
        {
            value = Value;
            if (IsExpired()) return false;

            _accessCount++; // not concerned about losing occasional values due to threading
            var slide = _slidingSeconds;
            if (slide != 0) _absoluteExpirationTicks = s_currentDateIshTicks + (slide * TimeSpan.TicksPerSecond);
            return true;
        }
    }
}
