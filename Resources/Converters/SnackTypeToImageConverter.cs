using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Common.Enums;

namespace Resources.Converters;

public class SnackTypeToImageConverter : IValueConverter
{
    public ImageSource? NoneImage { get; set; }
    public ImageSource? PrimaryImage { get; set; }
    public ImageSource? SuccessImage { get; set; }
    public ImageSource? WarningImage { get; set; }
    public ImageSource? CautionImage { get; set; }
    public ImageSource? InfoImage { get; set; }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is not SnackType snackType)
            return null;

        return snackType switch
        {
            SnackType.None => NoneImage,
            SnackType.Primary => PrimaryImage,
            SnackType.Success => SuccessImage,
            SnackType.Warning => WarningImage,
            SnackType.Info => InfoImage,
            SnackType.Caution => CautionImage,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}