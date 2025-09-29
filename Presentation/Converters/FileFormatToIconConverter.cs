using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

using Common.Enums;

namespace Presentation.Converters;

public class FileFormatToIconConverter : IMultiValueConverter
{
    
    public ImageSource? FolderIcon { get; set; }
    public ImageSource? OpenedFolderIcon { get; set; }
    public ImageSource? EquipmentSheetIcon { get; set; }
    public ImageSource? PivotSheetIcon { get; set; }

    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if(values.Length < 2)
            return null;
        
        if (values[0] is not FileFormat fileFormat) 
            return null;
        
        var isExpanded = values[1] as bool? ?? false;

        return fileFormat switch
        {
            FileFormat.None => null,
            FileFormat.Folder => isExpanded ? OpenedFolderIcon : FolderIcon,
            FileFormat.EquipmentSheet => EquipmentSheetIcon,
            FileFormat.PivotSheet => PivotSheetIcon,
            FileFormat.RepairsSheet or FileFormat.ServicesSheet or FileFormat.WriteOffSheet => (object?)null,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}