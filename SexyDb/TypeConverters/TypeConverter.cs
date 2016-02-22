using System;

namespace SexyDb.TypeConverters
{
    public static class TypeConverter
    {
        public static object Convert(object o, Type targetType)
        {
            return System.Convert.ChangeType(o, targetType);
        }
    }
}