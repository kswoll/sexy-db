using System;

namespace SexyDb.TypeConverters
{
    public interface ITypeConverter
    {
        object Convert(object o, Type targetType); 
    }
}