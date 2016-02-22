using System;

namespace SexyDb.TypeConverters
{
    [TypeConverter(typeof(DateTime), typeof(string))]
    public class DateTimeToStringConverter : ITypeConverter
    {
        public object Convert(object o, Type targetType)
        {
            return ((DateTime)o).ToString("o");
        }
    }
}