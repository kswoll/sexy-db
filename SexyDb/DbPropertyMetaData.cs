using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SexyReact;

namespace SexyDb
{
    public class DbPropertyMetaData
    {
        public PropertyInfo Property { get; }
        public string Name => Property.Name;
        public DbPropertyType Type { get; }

        private static readonly HashSet<Type> primitiveTypes = new HashSet<Type>
        {
            typeof(string), 
            typeof(DateTime),
            typeof(int),
            typeof(bool),
            typeof(byte),
            typeof(short),
            typeof(long),
            typeof(decimal),
            typeof(float),
            typeof(double)
        };

        public DbPropertyMetaData(PropertyInfo property)
        {
            Property = property;

            if (typeof(IRxList).IsAssignableFrom(property.PropertyType))
            {
                var itemType = property.PropertyType.GetGenericArguments().Single();
                if (IsPrimitive(itemType))
                    Type = DbPropertyType.ValueList;
                else
                    Type = DbPropertyType.ObjectList;
            }
            else if (typeof(IRxDictionary).IsAssignableFrom(property.PropertyType))
            {
                var keyType = property.PropertyType.GetGenericArguments()[0];
                if (!IsPrimitive(keyType))
                    throw new ArgumentException("The key to a dictionary must be a primitive");
                var valueType = property.PropertyType.GetGenericArguments()[1];
                if (IsPrimitive(valueType))
                    Type = DbPropertyType.ValueDictionary;
                else
                    Type = DbPropertyType.ObjectDictionary;
            }
            else if (IsPrimitive(property.PropertyType))
                Type = DbPropertyType.Value;
            else
                Type = DbPropertyType.Object;
        }

        private bool IsPrimitive(Type type)
        {
            return primitiveTypes.Contains(type) || type.IsEnum;
        }
    }
}