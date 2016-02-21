using System;
using System.Collections.Concurrent;

namespace SexyDb
{
    public static class DbObjectMetaDataCache
    {
        private static readonly ConcurrentDictionary<Type, DbObjectMetaData> cache = new ConcurrentDictionary<Type, DbObjectMetaData>();

        public static DbObjectMetaData GetMetaData(Type type)
        {
            return cache.GetOrAdd(type, x => new DbObjectMetaData(x));
        }
    }
}