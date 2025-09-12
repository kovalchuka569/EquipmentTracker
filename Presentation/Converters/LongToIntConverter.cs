using System;
using System.Globalization;
using System.Windows.Data;

namespace Presentation.Converters;

public class LongToIntConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Из ViewModel (int) в View (long)
        if (value is int intValue)
        {
            return (long)intValue;
        }
        return 0L;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Из View (long) в ViewModel (int)
        if (value is long longValue)
        {
            return (int)longValue;
        }
        return 0;
    }
}