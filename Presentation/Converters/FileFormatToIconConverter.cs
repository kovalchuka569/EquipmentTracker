using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

using Common.Enums;

namespace Presentation.Converters;

public class FileFormatToIconConverter : IMultiValueConverter, IValueConverter
{
    
    // Multi value converter
    
    public object? Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2) 
            return null;
            
        if (values[0] is not FileFormat fileFormat) 
            return null;
            
        if (parameter is not FrameworkElement element) 
            return null;

        var isExpanded = values[1] is bool expanded && expanded;

        var resourceKey = fileFormat switch
        {
            FileFormat.None => string.Empty,
            FileFormat.Folder when isExpanded => "FolderOpenedColoredLine36",  
            FileFormat.Folder => "FolderClosedColoredLine36",                
            FileFormat.EquipmentSheet => "TableSheetDarkLine36",
            FileFormat.PivotSheet => "PivotSheetDarkLine36",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        if (string.IsNullOrEmpty(resourceKey))
            return null;
        
        if (element.Resources.Contains(resourceKey))
        {
            return element.Resources[resourceKey] as ImageSource;
        }
        
        return element.TryFindResource(resourceKey) as ImageSource;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
    
    // Value converter

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not FileFormat fileFormat) 
            return null;
            
        if (parameter is not FrameworkElement element) 
            return null;
        
        var resourceKey = fileFormat switch
        {
            FileFormat.None => string.Empty,
            FileFormat.Folder => "FolderClosedColoredLine36",                
            FileFormat.EquipmentSheet => "TableSheetDarkLine36",
            FileFormat.PivotSheet => "PivotSheetDarkLine36",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        if (string.IsNullOrEmpty(resourceKey))
            return null;
        
        if (element.Resources.Contains(resourceKey))
        {
            return element.Resources[resourceKey] as ImageSource;
        }
        
        return element.TryFindResource(resourceKey) as ImageSource;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}