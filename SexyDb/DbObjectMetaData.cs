using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace SexyDb
{
    public class DbObjectMetaData
    {
        public ImmutableList<DbPropertyMetaData> Properties { get; }

        public DbObjectMetaData(Type type)
        {
            Properties = type.GetProperties().Where(IsSerializable).Select(x => new DbPropertyMetaData(x)).ToImmutableList();
        }

        private bool IsSerializable(PropertyInfo property)
        {
            return property.CanRead && property.CanWrite;
        }
    }
}