using System.Globalization;
using System.Windows;
using System.Windows.Data;

using Common.Enums;

namespace Common.Converters;

public class FileFormatToTextBlockStyleConverter : IValueConverter
{
    
    private const string InvalidFileFormatValue = "Invalid FileFormat value";
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not FileFormat fileFormat) 
            return null;
            
        if (parameter is not Style styleParameter) 
            return null;
        
        return fileFormat switch
        {
            FileFormat.EquipmentSheet or FileFormat.PivotSheet => styleParameter,
            FileFormat.None or FileFormat.Folder => null,
            _ => throw new ArgumentOutOfRangeException(nameof(fileFormat), InvalidFileFormatValue)
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}