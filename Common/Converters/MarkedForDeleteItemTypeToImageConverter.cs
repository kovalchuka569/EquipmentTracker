using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Common.Enums;

namespace Common.Converters;

public class MarkedForDeleteItemTypeToImageConverter : IMultiValueConverter
{
    public ImageSource? FolderIcon { get; set; }
    public ImageSource? OpenedFolderIcon { get; set; }
    public ImageSource?  EquipmentSheetIcon { get; set; }
    public ImageSource?  EquipmentSheetColumnIcon { get; set; }
    public ImageSource?  EquipmentSheetRowIcon { get; set; }
    public ImageSource?  PivotSheetIcon { get; set; }
        
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if(values.Length < 2)
            return null;
        
        if (values[0] is not MarkedForDeleteItemType itemType)
            return null;
        
        var isExpanded = values[1] as bool? ?? false;

        return itemType switch
        {
            MarkedForDeleteItemType.None => (object?)null,
            MarkedForDeleteItemType.Folder => isExpanded ? OpenedFolderIcon : FolderIcon,
            MarkedForDeleteItemType.EquipmentSheet => EquipmentSheetIcon,
            MarkedForDeleteItemType.EquipmentSheetColumn => EquipmentSheetColumnIcon,
            MarkedForDeleteItemType.EquipmentSheetRow => EquipmentSheetRowIcon,
            MarkedForDeleteItemType.PivotSheet => PivotSheetIcon,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}