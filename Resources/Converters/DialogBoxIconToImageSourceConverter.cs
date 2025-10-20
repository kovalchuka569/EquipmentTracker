using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Common.Enums;

namespace Resources.Converters;

public class DialogBoxIconToImageSourceConverter : IValueConverter
{
    public ImageSource AlertImageSource { get; set; }
    public ImageSource TrashImageSource { get; set; }
    public ImageSource InfoImageSource { get; set; }
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DialogBoxIcon icon)
            return null;

        return icon switch
        {
            DialogBoxIcon.None => null,
            DialogBoxIcon.Alert => AlertImageSource,
            DialogBoxIcon.Trash => TrashImageSource,
            DialogBoxIcon.Info => InfoImageSource,
            _ => null
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}