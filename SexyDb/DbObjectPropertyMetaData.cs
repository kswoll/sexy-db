using System;
using System.Reflection;
using SexyReact;

namespace SexyDb
{
    public class DbObjectPropertyMetaData
    {
        public PropertyInfo Property { get; }
        public string Name => Property.Name;

        public DbObjectPropertyMetaData(PropertyInfo property)
        {
            Property = property;
        }
    }
}