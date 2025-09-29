using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Common.Converters;

public class BooleanToColorConverter : IValueConverter
{
    public SolidColorBrush? TrueBrush { get; set; }
    public SolidColorBrush? FalseBrush { get; set; }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is not bool isChecked)
            return null;
        
        return isChecked ? TrueBrush : FalseBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}