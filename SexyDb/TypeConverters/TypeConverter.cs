using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SexyDb.TypeConverters
{
    public static class TypeConverter
    {
        private static Dictionary<Tuple<Type, Type>, ITypeConverter> typeConverters = new Dictionary<Tuple<Type, Type>, ITypeConverter>();

        static TypeConverter()
        {
            foreach (var type in typeof(ITypeConverter).Assembly.GetTypes().Where(x => Attribute.IsDefined(x, typeof(TypeConverterAttribute))))
            {
                foreach (var attribute in type.GetCustomAttributes<TypeConverterAttribute>())
                {
                    var converter = (ITypeConverter)Activator.CreateInstance(type);
                    typeConverters[Tuple.Create(attribute.SourceType, attribute.TargetType)] = converter;                    
                }
            }
        }

        public static object Convert(object o, Type targetType)
        {
            ITypeConverter converter;
            if (typeConverters.TryGetValue(Tuple.Create(o.GetType(), targetType), out converter))
                return converter.Convert(o, targetType);

            if (o is Enum)
                return o.ToString();
            if (targetType.IsEnum)
                return Enum.Parse(targetType, (string)o);

            return System.Convert.ChangeType(o, targetType);
        }
    }
}