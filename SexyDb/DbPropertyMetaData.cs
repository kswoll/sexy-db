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
        public Type ElementType { get; }

        private static readonly Dictionary<Type, object> primitiveTypesAndDefaultValues = new Dictionary<Type, object>
        {
            [typeof(string)] = null, 
            [typeof(DateTime)] = default(DateTime),
            [typeof(int)] = default(int),
            [typeof(bool)] = default(bool),
            [typeof(byte)] = default(byte),
            [typeof(short)] = default(short),
            [typeof(long)] = default(long),
            [typeof(decimal)] = default(decimal),
            [typeof(float)] = default(float),
            [typeof(double)] = default(double)
        };

        public DbPropertyMetaData(PropertyInfo property)
        {
            Property = property;

            if (typeof(IRxList).IsAssignableFrom(property.PropertyType))
            {
                var itemType = property.PropertyType.GetGenericArguments().Single();
                ElementType = itemType;
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
                ElementType = valueType;
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
            return primitiveTypesAndDefaultValues.ContainsKey(type) || type.IsEnum;
        }

        public object DefaultValue
        {
            get
            {
                if (IsPrimitive(Property.PropertyType))
                {
                    return primitiveTypesAndDefaultValues[Property.PropertyType];
                }
                else if (Property.PropertyType.IsValueType)
                {
                    return Activator.CreateInstance(Property.PropertyType);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}