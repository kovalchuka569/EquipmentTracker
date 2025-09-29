using System.Globalization;
using System.Windows.Data;

namespace Common.Converters;

public class MultiplyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double d)
            return null;
                
        var factor = System.Convert.ToDouble(parameter);
        return d * factor;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double d)
            return null;
                
        var factor = System.Convert.ToDouble(parameter);
        return d / factor;
    }
}