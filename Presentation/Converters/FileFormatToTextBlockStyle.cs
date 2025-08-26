using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using Common.Enums;

namespace Presentation.Converters;

public class FileFormatToTextBlockStyle : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not FileFormat fileFormat) 
            return null;
            
        if (parameter is not FrameworkElement element) 
            return null;
        
        var resourceKey = fileFormat switch
        {
            FileFormat.EquipmentSheet or FileFormat.PivotSheet => "FileTextBlockStyle",
            FileFormat.None or FileFormat.Folder => string.Empty,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        if (string.IsNullOrEmpty(resourceKey))
            return null;
        
        if (element.Resources.Contains(resourceKey))
        {
            return element.Resources[resourceKey] as Style;
        }
        
        return element.TryFindResource(resourceKey) as Style;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}