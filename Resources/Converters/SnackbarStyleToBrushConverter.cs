using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Common.Enums;

namespace Resources.Converters;

public class SnackbarStyleToBrushConverter : IValueConverter
{
    public Brush? NoneBrush { get; set; }
    public Brush? PrimaryBrush { get; set; }
    public Brush? SuccessBrush { get; set; }
    public Brush? WarningBrush { get; set; }
    public Brush? CautionBrush { get; set; }
    public Brush? InfoBrush { get; set; }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is not SnackbarStyle snackbarStyle)
            return null;

        return snackbarStyle switch
        {
            SnackbarStyle.None => NoneBrush,
            SnackbarStyle.Primary => PrimaryBrush,
            SnackbarStyle.Success => SuccessBrush,
            SnackbarStyle.Warning => WarningBrush,
            SnackbarStyle.Info => InfoBrush,
            SnackbarStyle.Caution => CautionBrush,
            _ => (object?)null
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}