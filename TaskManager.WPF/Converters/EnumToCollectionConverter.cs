using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace TaskManager.WPF.Converters
{
    public class EnumToCollectionConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            Type enumType;
            
            // Check if value is a Type (when using x:Type)
            if (value is Type type)
            {
                enumType = type;
            }
            else
            {
                // Otherwise get the type of the enum instance
                enumType = value.GetType();
            }

            if (!enumType.IsEnum)
                return null;

            return Enum.GetValues(enumType).Cast<Enum>().ToList();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 