using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Reflection.Emit;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace DuoVia.FuzzyStrings
{
    public static class TypeHelper
    {
        public static int SizeOf<T>(T? obj) where T : struct
        {
            if (obj == null) throw new ArgumentNullException("obj");
            return SizeOf(typeof(T?));
        }

        public static int SizeOf<T>(T obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            return SizeOf(obj.GetType());
        }

        public static int SizeOf(Type t)
        {
            if (t == null) throw new ArgumentNullException("t");

            return Cache.GetOrAdd(t, t2 =>
            {
                var dm = new DynamicMethod("$", typeof(int), Type.EmptyTypes);
                ILGenerator il = dm.GetILGenerator();
                il.Emit(OpCodes.Sizeof, t2);
                il.Emit(OpCodes.Ret);

                var func = (Func<int>)dm.CreateDelegate(typeof(Func<int>));
                return func();
            });
        }

        private static readonly ConcurrentDictionary<Type, int>
            Cache = new ConcurrentDictionary<Type, int>();
    }

    public interface IMemoryCacheWithItemLimit<TValue>
    {
        long CacheItemCount { get; }
        TValue GetOrAdd(string key, Func<string, TValue> addMethodWhenKeyNotFoundAction);
    }

    public class MemoryCacheWithItemLimit<TValue> : IMemoryCacheWithItemLimit<TValue>
    {
        private readonly long _cacheSize;

        //private readonly object _lockObject = new object();
        private readonly bool _withSpinWait;
        private readonly MemoryCache _cache;

        private readonly TimeSpan _slidingExpiration;
        //private readonly object _taskLockObject = new object();
        private Task _trimCacheTask;

        public MemoryCacheWithItemLimit(int cacheSize, bool withSpinWait) : this(cacheSize, 300, withSpinWait)
        {
        }

        public MemoryCacheWithItemLimit(int cacheSize = 25000, int defaultCacheDurationInSeconds = 90, bool withSpinWait = true)
        {
            _cacheSize = cacheSize;
            _withSpinWait = withSpinWait;
            int sizeOfValueTypeInBytes = TypeHelper.SizeOf(typeof (TValue)) + TypeHelper.SizeOf(typeof(string));

            long totalNumberOfBytesNeeded = sizeOfValueTypeInBytes*cacheSize;
            long numberOfMbsNeeded = (totalNumberOfBytesNeeded/1024)/1024;

            if (numberOfMbsNeeded > 256)
            {
                numberOfMbsNeeded = 256;
            }
            if (numberOfMbsNeeded < 1)
            {
                numberOfMbsNeeded = 1;
            }
            _trimEnabled = false;
            string cacheName = GetType().FullName;

            _cache = new MemoryCache(cacheName, new NameValueCollection
            {
                {"CacheMemoryLimitMegabytes", numberOfMbsNeeded.ToString()},
                {"PollingInterval","00:00:30" }
            });
            _slidingExpiration = new TimeSpan(0, 0, defaultCacheDurationInSeconds, 0);
            
        }       

        public long CacheItemCount
        {
            get { return _cache.GetCount(); }
        }
       

        public TValue GetOrAdd(string key, Func<string, TValue> addMethodWhenKeyNotFoundAction)
        {
            //if (key == null)
            //{
            //    throw new ArgumentNullException(nameof(key));
            //}

            object valueFromCache = _cache.Get(key);
            if (valueFromCache == null)
            {
                TValue valueToStoreInCache = addMethodWhenKeyNotFoundAction.Invoke(key);
                if (valueToStoreInCache != null)
                {
                    _cache.Set(key, valueToStoreInCache, new CacheItemPolicy {SlidingExpiration = _slidingExpiration});
                    TryTrimCache();
                }
                return valueToStoreInCache;
            }

            return (TValue) valueFromCache;
        }

        private long _executingTrim;
        private readonly bool _trimEnabled;

        private void TryTrimCache()
        {
            if (!_trimEnabled)
            {
                return;
            }

            if (Interlocked.Read(ref _executingTrim) == 1)
            {
                return;
            }

            if (_trimCacheTask == null && _cache.GetCount() <= _cacheSize)
            {
                return;
            }

            if (Interlocked.Read(ref _executingTrim) == 0 && _cache.GetCount() >= _cacheSize)
            {
                if (Interlocked.CompareExchange(ref _executingTrim, 1, 0) == 0)
                {
                    _trimCacheTask = Task.Run(() =>
                    {
                        _cache.Trim(10);
                        _trimCacheTask = null;
                        if (_withSpinWait)
                        {
                            Thread.SpinWait(Environment.ProcessorCount * 1024);
                        }
                        Interlocked.Exchange(ref _executingTrim, 0);
                    });

                }
            }

/*
            if (Monitor.TryEnter(_lockObject))
            {
                try
                {
                    if (_trimCacheTask == null && _cache.GetCount() >= _cacheSize )
                    {
                        if (Monitor.TryEnter(_taskLockObject))
                        {
                            try
                            {
                                if (_trimCacheTask == null)
                                {

                                    _trimCacheTask = Task.Run(() =>
                                    {
                                        _cache.Trim(10);
                                        _trimCacheTask = null;
                                        if (_withSpinWait)
                                        {
                                            Thread.SpinWait(Environment.ProcessorCount*1024);
                                        }


                                    });
                                }

                            }
                            finally
                            {
                                Monitor.Exit(_taskLockObject);
                            }
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_lockObject);
                }
            }
*/
        }
    }
}