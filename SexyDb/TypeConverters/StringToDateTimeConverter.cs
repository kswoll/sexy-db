using System;

namespace SexyDb.TypeConverters
{
    [TypeConverter(typeof(string), typeof(DateTime))]
    public class StringToDateTimeConverter : ITypeConverter
    {
        public object Convert(object o, Type targetType)
        {
            return DateTime.ParseExact((string)o, "o", null);
        }
    }
}