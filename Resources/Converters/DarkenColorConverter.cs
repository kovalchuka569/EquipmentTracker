using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace Resources.Converters;

public class DarkenColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Color color)
            return value;
        
        var factor = 0.1;
        if (parameter != null && double.TryParse(parameter.ToString(), out var p))
        {
            factor = p;
        }

        var r = (byte)Math.Max(0, color.R * (1 - factor));
        var g = (byte)Math.Max(0, color.G * (1 - factor));
        var b = (byte)Math.Max(0, color.B * (1 - factor));

        return Color.FromArgb(color.A, r, g, b);

    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}