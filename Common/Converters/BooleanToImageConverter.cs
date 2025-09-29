using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Common.Converters;

public class BooleanToImageConverter : IValueConverter
{
    public ImageSource? TrueImage { get; set; }
    public ImageSource? FalseImage { get; set; }
    
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            true => TrueImage,
            false => FalseImage,
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}