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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace StackRedis
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

        private readonly ConcurrentDictionary<PartitionedKey, MemoryCacheEntry> _entries;
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
            _entries = new ConcurrentDictionary<PartitionedKey, MemoryCacheEntry>();
            _lastExpirationScan = MemoryCacheEntry.CurrentDateIshTicks;

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

        private ICollection<KeyValuePair<PartitionedKey, MemoryCacheEntry>> EntriesCollection => _entries;

        private void SetEntry(in PartitionedKey key, MemoryCacheEntry entry) => _entries[key] = entry;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in PartitionedKey key, out object result)
        {
            // Don't remove the invalid entry live, let the process clean it up
            if (_entries.TryGetValue(key, out MemoryCacheEntry entry) && entry.TryGet(out result))
            {
                return true;
            }
            result = null;
            return false;
        }

        /// <inheritdoc />
        public bool Remove(in PartitionedKey key) => _entries.TryRemove(key, out _);

        private void RemoveEntry(in PartitionedKey key, MemoryCacheEntry entry)
            => EntriesCollection.Remove(new KeyValuePair<PartitionedKey, MemoryCacheEntry>(key, entry));

        int _scanInProgress;
        long _lastScanTicks;

        internal DateTime LastCollection(out bool isActive, out TimeSpan duration)
        {
            isActive = Volatile.Read(ref _scanInProgress) != 0;
            duration = TimeSpan.FromTicks(_lastScanTicks);
            return new DateTime(_lastExpirationScan, DateTimeKind.Utc);
        }

        internal ValueTask<int> ScanForExpiredItems() // returns: -1 if already running (so: nothing done), else: the number of things removed
        {
            return !_disposed && Interlocked.CompareExchange(ref _scanInProgress, 1, 0) == 0 ? Impl(this) : new ValueTask<int>(-1);

            static async ValueTask<int> Impl(MemoryCache obj)
            {
                int count = 0, removed = 0;
                obj._lastExpirationScan = MemoryCacheEntry.CurrentDateIshTicks;
                try
                {
                    var start = DateTime.UtcNow;
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
                    obj._lastScanTicks = (DateTime.UtcNow - start).Ticks;
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
            => SetEntry(key, new MemoryCacheEntry(value, absoluteExpiration, slidingExpiration));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<TItem>(PartitionedKey key, TItem value, DateTime absoluteExpiration)
            => SetEntry(key, new MemoryCacheEntry(value, absoluteExpiration));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<TItem>(PartitionedKey key, TItem value, TimeSpan absoluteExpiration, TimeSpan slidingExpiration)
            => SetEntry(key, new MemoryCacheEntry(value, absoluteExpiration, slidingExpiration));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<TItem>(PartitionedKey key, TItem value, TimeSpan absoluteExpiration)
            => SetEntry(key, new MemoryCacheEntry(value, absoluteExpiration));

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
            if (_entries.TryGetValue(from, out MemoryCacheEntry entry) && !entry.IsExpired())
            {
                _entries[to] = entry;
                RemoveEntry(from, entry);
            }
            return false;
        }

        public IEnumerable<KeyValuePair<PartitionedKey, MemoryCacheEntry>> GetAllEntries()
        {
            foreach (var pair in _entries)
            {
                if (!pair.Value.IsExpired())
                    yield return pair;
            }
        }

        public IEnumerable<KeyValuePair<string, MemoryCacheEntry>> GetEntries(string partition)
        {
            if (partition == null) throw new ArgumentNullException(nameof(partition));
            foreach (var pair in _entries)
            {
                if (pair.Key.Partition == partition && !pair.Value.IsExpired())
                    yield return new KeyValuePair<string, MemoryCacheEntry>(pair.Key.Key, pair.Value);
            }
        }

        /// <summary>
        /// Performs an aggregate analysis over the data, obtaining the number and use data of keys
        /// </summary>
        /// <param name="normalizer">Optionally perform a normalization function on the keys, for example: removing number groups (assuming they are identifiers)</param>
        /// <param name="partition">Optionally restrist the data to a single partition (by default all partitions are considered)</param>
        public Dictionary<string, MemoryCacheSummary> GetSummary(out int expired, Func<string, string> normalizer = null, string partition = null)
        {
            expired = 0;
            Dictionary<string, MemoryCacheSummary> results = new Dictionary<string, MemoryCacheSummary>();
            foreach (var pair in _entries)
            {
                if (partition is object && pair.Key.Partition != partition) continue;

                if (pair.Value.IsExpired())
                {
                    expired++;
                    continue;
                }

                var key = pair.Key.Key;
                if (normalizer is object) key = normalizer(key);
                if (key is object)
                {
                    if (results.TryGetValue(key, out var existing))
                    {
                        results[key] = existing.Add(pair.Value);
                    }
                    else
                    {
                        results.Add(key, new MemoryCacheSummary(pair.Value));
                    }
                }
            }
            return results;
        }
    }


    public readonly struct MemoryCacheSummary
    {
        private static readonly Regex s_RemoveNumbers = new Regex(@"\d+", RegexOptions.Compiled);
        public static Func<string, string> RemoveNumbers { get; }
            = key => s_RemoveNumbers.Replace(key, "#");

        public override int GetHashCode() => throw new NotSupportedException();
        public override bool Equals(object obj) => throw new NotSupportedException();
        public override string ToString() => $"Count: {Count}, TotalAccessCount: {TotalAccessCount}, MaxExpiration: {MaxExpiration}";
        public int Count { get; }
        public int TotalAccessCount { get; }
        private readonly long _maxExpirationTicks;
        public DateTime MaxExpiration => _maxExpirationTicks == long.MaxValue ? DateTime.MaxValue : new DateTime(_maxExpirationTicks, DateTimeKind.Utc);

        public MemoryCacheSummary(MemoryCacheEntry value)
        {
            Count = 1;
            TotalAccessCount = value.AccessCount;
            _maxExpirationTicks = value.AbsoluteExpirationTicks;
        }
        internal MemoryCacheSummary(in MemoryCacheSummary existing, MemoryCacheEntry value)
        {
            Count = existing.Count + 1;
            TotalAccessCount = existing.TotalAccessCount + value.AccessCount;
            _maxExpirationTicks = Math.Max(existing._maxExpirationTicks, value.AbsoluteExpirationTicks);
        }
        public MemoryCacheSummary Add(MemoryCacheEntry value)
            => new MemoryCacheSummary(in this, value);
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
            => Key == null ? 0 : (Partition.GetHashCode() ^ Key.GetHashCode());
        public bool Equals(in PartitionedKey other) => Partition == other.Partition && Key == other.Key;
        bool IEquatable<PartitionedKey>.Equals(PartitionedKey other) => Partition == other.Partition && Key == other.Key;

        public static bool operator ==(in PartitionedKey x, in PartitionedKey y) => x.Partition == y.Partition && x.Key == y.Key;
        public static bool operator !=(in PartitionedKey x, in PartitionedKey y) => x.Partition != y.Partition || x.Key != y.Key;

        // not recommended: makes it too easy to get things wrong (not specifying partition)
        // public static implicit operator PartitionedKey(string key) => new PartitionedKey(key, null);
    }

    public sealed class MemoryCacheEntry
    {
        private long _absoluteExpirationTicks;
        private int _accessCount;
        private readonly uint _slidingSeconds;
        public object Value { get; }

        public int AccessCount => Volatile.Read(ref _accessCount);
        public DateTime AbsoluteExpiration => _absoluteExpirationTicks == long.MaxValue ? DateTime.MaxValue : new DateTime(_absoluteExpirationTicks, DateTimeKind.Utc);
        internal long AbsoluteExpirationTicks => _absoluteExpirationTicks;

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
        internal MemoryCacheEntry(object value, DateTime absoluteExpiration)
        {
            Value = value;
            _absoluteExpirationTicks = GetTicks(absoluteExpiration);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MemoryCacheEntry(object value, TimeSpan absoluteExpiration)
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
        internal MemoryCacheEntry(object value, DateTime absoluteExpiration, TimeSpan slidingExpiration)
            : this(value, absoluteExpiration)
            => _slidingSeconds = GetSlidingSeconds(slidingExpiration);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MemoryCacheEntry(object value, TimeSpan absoluteExpiration, TimeSpan slidingExpiration)
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
