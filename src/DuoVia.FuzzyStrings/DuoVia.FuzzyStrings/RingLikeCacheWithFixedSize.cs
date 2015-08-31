using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DuoVia.FuzzyStrings
{
    public class RingLikeCacheWithFixedSize<TKey, TValue>
    {
        private readonly int _cacheSize;
        private readonly TKey[] _keyRingBuffer;
        private readonly ConcurrentDictionary<TKey, TValue> _valueStore;
        private int _head;
        private int _length;

        private int _tail;


        public RingLikeCacheWithFixedSize(IEqualityComparer<TKey> comparer, int cacheSize = 1000)
        {
            if (comparer != null)
            {
                _valueStore = new ConcurrentDictionary<TKey, TValue>(comparer);
            }
            else
            {
                _valueStore = new ConcurrentDictionary<TKey, TValue>();
            }

            _cacheSize = cacheSize;
            _keyRingBuffer = new TKey[cacheSize];
            _tail = 0;
            _head = _cacheSize - 1;
        }

        public RingLikeCacheWithFixedSize(int cacheSize = 1000) : this(null, cacheSize)
        {
        }

        protected bool IsFull
        {
            get { return _length == _cacheSize; }
        }


        public TValue GetOrAdd(TKey key, Func<TKey, TValue> addMethodWhenKeyNotFoundAction)
        {
          
                return _valueStore.GetOrAdd(key, theKey =>
                {
                    TValue valueToAdd = addMethodWhenKeyNotFoundAction.Invoke(theKey);
                    _head = GetNextPosition(_head);

                    if (_keyRingBuffer[_head] != null)
                    {
                        //remove old value in that place
                        TValue tmpValue;
                        _valueStore.TryRemove(_keyRingBuffer[_head], out tmpValue);                        
                        //replace with new value                    
                    }

                    _keyRingBuffer[_head] = key;
                    if (IsFull)
                    {
                        _tail = GetNextPosition(_tail);
                        TValue tmpValue;
                        _valueStore.TryRemove(_keyRingBuffer[_tail], out tmpValue);
                    }
                    else
                    {
                        _tail = GetNextPosition(_tail);
                        _length++;
                    }

                    return valueToAdd;
                });
        }

        private int GetNextPosition(int currentPosition)
        {
            return (currentPosition + 1)%_cacheSize;
        }
    }
}