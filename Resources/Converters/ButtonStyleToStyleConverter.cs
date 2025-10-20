using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Common.Enums;

namespace Resources.Converters;

public class ButtonStyleToStyleConverter : IValueConverter
{
    public Style DefaultStyle { get; set; }
    public Style PrimaryStyle { get; set; }
    public Style WarningStyle { get; set; }
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ButtonStyle buttonStyle)
            return DefaultStyle;

        return buttonStyle switch
        {
            ButtonStyle.Default => DefaultStyle,
            ButtonStyle.Primary => PrimaryStyle,
            ButtonStyle.Warning => WarningStyle,
            _ => DefaultStyle
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}