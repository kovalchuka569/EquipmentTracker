using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Common.Enums;

namespace Resources.Converters;

public class SnackbarContainerToTemplateConverter : IValueConverter
{
    public DataTemplate? TextTemplate { get; set; }
    public DataTemplate? ProgressTemplate { get; set; }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is not SnackbarContainer snackbarContainer)
            return null;

        return snackbarContainer switch
        {
            SnackbarContainer.Text => TextTemplate,
            SnackbarContainer.Progress => ProgressTemplate,
            _ => (object?)null
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}