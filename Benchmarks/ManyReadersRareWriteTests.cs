using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmarks
{
    public class ManyReadersRareWriteTests
    {
        //Scenario = You have a collection of data you want to cache
        //it is read often, it is a key/value pair set and very rarely it
        //is updated. It needs to be read by multiple threads
        //take for example you have a list of projects on a project management
        //website and every couple of days max someone adds one. You might
        //be tempted to use a ReaderWriterLock, or ReaderWriterLockSlim
        //you might be tempted to use a concurrent dictionary, let's take a look
        //at the results.
        
        private Dictionary<int, int> _traditionalDictionary = new Dictionary<int, int>();
        private ConcurrentDictionary<int, int> _concurrentDictionary = new ConcurrentDictionary<int, int>();
        private ReaderWriterLockSlim _readerWriterNoRecurse = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private ReaderWriterLockSlim _readerWriterRecurse = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private object _lockObject = new object();
        private int _writeCount = NumberOfReads;

        const int NumberOfReads = 100_000;
        const int Writes = 4;
        const int Threads = 4;
        const int Items = 100_000;
        const int WriteEvery = NumberOfReads / Writes; 


        [GlobalSetup]
        public void Setup()
        {
            for(int i = 0; i < Items;i++)
            {
                _traditionalDictionary[i] = i;
                _concurrentDictionary[i] = i;
            }
        }

        [Benchmark(Baseline = true, OperationsPerInvoke = NumberOfReads * Threads)]
        public async Task ReaderWriterNoRecurse()
        {
            var tasks = new Task[Threads];
            for(int i = 0; i < Threads;i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    var rand = new Random(i);
                    var writeOn = rand.Next(0, WriteEvery);
                    var counter = 0;
                    for (int x = 0; x < NumberOfReads; x++)
                    {
                        var readIndex = rand.Next(0, Items - 1);
                        _readerWriterNoRecurse.EnterReadLock();
                        counter += _traditionalDictionary[readIndex];
                        _readerWriterNoRecurse.ExitReadLock();
                        if(x % WriteEvery == writeOn)
                        {
                            _readerWriterNoRecurse.EnterWriteLock();
                            var writeNumber = Interlocked.Increment(ref _writeCount);
                            _traditionalDictionary.Add(writeNumber, writeNumber);
                            _readerWriterNoRecurse.ExitWriteLock();
                        }
                    }
                });
            }
            await Task.WhenAll(tasks);
        }

        [Benchmark(OperationsPerInvoke = NumberOfReads * Threads)]
        public async Task ReaderWriterRecurse()
        {
            var tasks = new Task[Threads];
            for (int i = 0; i < Threads; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    var rand = new Random(i);
                    var writeOn = rand.Next(0, WriteEvery);
                    var counter = 0;
                    for (int x = 0; x < NumberOfReads; x++)
                    {
                        var readIndex = rand.Next(0, Items - 1);
                        _readerWriterRecurse.EnterReadLock();
                        counter += _traditionalDictionary[readIndex];
                        _readerWriterRecurse.ExitReadLock();
                        if (x % WriteEvery == writeOn)
                        {
                            _readerWriterRecurse.EnterWriteLock();
                            var writeNumber = Interlocked.Increment(ref _writeCount);
                            _traditionalDictionary.Add(writeNumber, writeNumber);
                            _readerWriterRecurse.ExitWriteLock();
                        }
                    }
                });
            }
            await Task.WhenAll(tasks);
        }

        [Benchmark(OperationsPerInvoke = NumberOfReads * Threads)]
        public async Task ConcurrentDictionary()
        {
            var tasks = new Task[Threads];
            for (int i = 0; i < Threads; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    var rand = new Random(i);
                    var writeOn = rand.Next(0, WriteEvery);
                    var counter = 0;
                    for (int x = 0; x < NumberOfReads; x++)
                    {
                        var readIndex = rand.Next(0, Items - 1);
                        counter += _concurrentDictionary[readIndex];
                        if (x % WriteEvery == writeOn)
                        {
                            var writeNumber = Interlocked.Increment(ref _writeCount);
                            _concurrentDictionary[writeNumber] = writeNumber;
                        }
                    }
                });
            }
            await Task.WhenAll(tasks);
        }

        [Benchmark(OperationsPerInvoke = NumberOfReads * Threads)]
        public async Task TraditionalLock()
        {
            var tasks = new Task[Threads];
            for (int i = 0; i < Threads; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    var rand = new Random(i);
                    var writeOn = rand.Next(0, WriteEvery);
                    var counter = 0;
                    for (int x = 0; x < NumberOfReads; x++)
                    {
                        var readIndex = rand.Next(0, Items - 1);
                        lock (_lockObject)
                        {
                            counter += _traditionalDictionary[readIndex];
                        }
                        if (x % WriteEvery == writeOn)
                        {
                            lock (_lockObject)
                            {
                                var writeNumber = Interlocked.Increment(ref _writeCount);
                                _traditionalDictionary[writeNumber] = writeNumber;
                            }
                        }
                    }
                });
            }
            await Task.WhenAll(tasks);
        }
    }
}
