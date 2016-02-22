using System;

namespace SexyDb.TypeConverters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TypeConverterAttribute : Attribute
    {
        public Type SourceType { get; }
        public Type TargetType { get; }

        public TypeConverterAttribute(Type sourceType, Type targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;
        }
    }
}