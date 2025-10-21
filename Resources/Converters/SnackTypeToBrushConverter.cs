using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Common.Enums;

namespace Resources.Converters;

public class SnackTypeToBrushConverter : IValueConverter
{
    public Brush? NoneBrush { get; set; }
    public Brush? PrimaryBrush { get; set; }
    public Brush? SuccessBrush { get; set; }
    public Brush? WarningBrush { get; set; }
    public Brush? CautionBrush { get; set; }
    public Brush? InfoBrush { get; set; }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is not SnackType snackType)
            return null;

        return snackType switch
        {
            SnackType.None => NoneBrush,
            SnackType.Primary => PrimaryBrush,
            SnackType.Success => SuccessBrush,
            SnackType.Warning => WarningBrush,
            SnackType.Info => InfoBrush,
            SnackType.Caution => CautionBrush,
            _ => (object?)null
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}