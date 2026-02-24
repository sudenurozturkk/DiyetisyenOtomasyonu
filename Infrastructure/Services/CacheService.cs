using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace DiyetisyenOtomasyonu.Infrastructure.Services
{
    /// <summary>
    /// Cache Servisi - Memory Cache yönetimi
    /// Performans optimizasyonu için sık kullanılan verileri cache'ler
    /// </summary>
    public class CacheService
    {
        private static readonly MemoryCache _cache = MemoryCache.Default;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Cache'den veri al veya oluştur
        /// </summary>
        public T GetOrSet<T>(string key, Func<T> getItem, TimeSpan expiration)
        {
            lock (_lockObject)
            {
                if (_cache.Contains(key))
                {
                    return (T)_cache.Get(key);
                }

                var item = getItem();
                if (item != null)
                {
                    var policy = new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTimeOffset.Now.Add(expiration),
                        SlidingExpiration = TimeSpan.Zero
                    };
                    _cache.Set(key, item, policy);
                }
                return item;
            }
        }

        /// <summary>
        /// Cache'den veri al
        /// </summary>
        public T Get<T>(string key)
        {
            if (_cache.Contains(key))
            {
                return (T)_cache.Get(key);
            }
            return default(T);
        }

        /// <summary>
        /// Cache'e veri ekle
        /// </summary>
        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            lock (_lockObject)
            {
                var policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.Add(expiration),
                    SlidingExpiration = TimeSpan.Zero
                };
                _cache.Set(key, value, policy);
            }
        }

        /// <summary>
        /// Cache'den veri sil
        /// </summary>
        public void Remove(string key)
        {
            lock (_lockObject)
            {
                if (_cache.Contains(key))
                {
                    _cache.Remove(key);
                }
            }
        }

        /// <summary>
        /// Pattern'e göre cache temizle
        /// </summary>
        public void RemoveByPattern(string pattern)
        {
            lock (_lockObject)
            {
                var keysToRemove = new List<string>();
                foreach (var item in _cache)
                {
                    if (item.Key.Contains(pattern))
                    {
                        keysToRemove.Add(item.Key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                }
            }
        }

        /// <summary>
        /// Tüm cache'i temizle
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                var keys = new List<string>();
                foreach (var item in _cache)
                {
                    keys.Add(item.Key);
                }

                foreach (var key in keys)
                {
                    _cache.Remove(key);
                }
            }
        }

        /// <summary>
        /// Cache istatistikleri
        /// </summary>
        public CacheStatistics GetStatistics()
        {
            int count = 0;
            long memorySize = 0;

            foreach (var item in _cache)
            {
                count++;
                // Memory size estimation (approximate)
                memorySize += EstimateSize(item.Value);
            }

            return new CacheStatistics
            {
                ItemCount = count,
                EstimatedMemorySize = memorySize
            };
        }

        private long EstimateSize(object obj)
        {
            // Basit boyut tahmini
            if (obj == null) return 0;
            if (obj is string str) return str.Length * 2; // Unicode
            if (obj is System.Collections.ICollection collection) return collection.Count * 100; // Approximate
            return 100; // Default size
        }
    }

    /// <summary>
    /// Cache istatistikleri
    /// </summary>
    public class CacheStatistics
    {
        public int ItemCount { get; set; }
        public long EstimatedMemorySize { get; set; }
    }
}
