using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Common.Enums;

namespace Resources.Converters;

public class SnackbarStyleToImageConverter : IValueConverter
{
    public ImageSource? NoneImage { get; set; }
    public ImageSource? PrimaryImage { get; set; }
    public ImageSource? SuccessImage { get; set; }
    public ImageSource? WarningImage { get; set; }
    public ImageSource? CautionImage { get; set; }
    public ImageSource? InfoImage { get; set; }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is not SnackbarStyle snackbarStyle)
            return null;

        return snackbarStyle switch
        {
            SnackbarStyle.None => NoneImage,
            SnackbarStyle.Primary => PrimaryImage,
            SnackbarStyle.Success => SuccessImage,
            SnackbarStyle.Warning => WarningImage,
            SnackbarStyle.Info => InfoImage,
            SnackbarStyle.Caution => CautionImage,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}