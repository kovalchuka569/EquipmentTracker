using System.Windows;
using System.Windows.Media;
using Models.Equipment;
using Models.Equipment.ColumnSettings;
using Models.Table;

namespace Core.Services.Common;

public class ColumnSettingsParser
{
    public ColumnSettingsDisplayModel ToColumnSettingsDisplayModel(ColumnSettings columnSettings)
    {
        return new ColumnSettingsDisplayModel
        {
            DataType = ParseColumnDataType(columnSettings.DataType),
            HeaderText = columnSettings.HeaderText,
            MappingName = columnSettings.MappingName,
            IsReadOnly = columnSettings.IsReadOnly,
            IsUnique = columnSettings.IsUnique,
            IsRequired = columnSettings.IsRequired,
            AllowSorting = columnSettings.AllowSorting,
            AllowFiltering = columnSettings.AllowFiltering,
            AllowGrouping = columnSettings.AllowGrouping,
            IsPinned = columnSettings.IsPinned,
            ColumnPosition = columnSettings.ColumnPosition,
            ColumnWidth = columnSettings.ColumnWidth,
            HeaderBackground = ParseColor(columnSettings.HeaderBackground),
            HeaderForeground = ParseColor(columnSettings.HeaderForeground),
            HeaderBorderColor = ParseColor(columnSettings.HeaderBorderColor),
            HeaderFontFamily = columnSettings.HeaderFontFamily,
            HeaderFontSize = columnSettings.HeaderFontSize,
            HeaderFontWeight = ParseFontWeight(columnSettings.HeaderFontWeight),
            HeaderVerticalAlignment = ParseVerticalAlignment(columnSettings.HeaderVerticalAlignment),
            HeaderHorizontalAlignment = ParseHorizontalAlignment(columnSettings.HeaderHorizontalAlignment),
            HeaderBorderThickness = new Thickness(
                columnSettings.LeftHeaderBorderThickness,
                columnSettings.TopHeaderBorderThickness,
                columnSettings.RightHeaderBorderThickness,
                columnSettings.BottomHeaderBorderThickness
            ),
            SpecificSettings = columnSettings.SpecificSettings
        };
    }

    public ColumnSettings ToColumnSettingsBase(ColumnSettingsDisplayModel columnSettings)
    {
        return new ColumnSettings
        {
            DataType = columnSettings.DataType.ToString(),
            HeaderText = columnSettings.HeaderText,
            MappingName = columnSettings.MappingName,
            IsRequired = columnSettings.IsReadOnly,
            IsUnique = columnSettings.IsUnique,
            IsReadOnly = columnSettings.IsReadOnly,
            AllowSorting = columnSettings.AllowSorting,
            AllowFiltering = columnSettings.AllowFiltering,
            AllowGrouping = columnSettings.AllowGrouping,
            IsPinned = columnSettings.IsPinned,
            ColumnPosition = columnSettings.ColumnPosition,
            ColumnWidth = columnSettings.ColumnWidth,
            HeaderBackground = columnSettings.HeaderBackground.ToString(),
            HeaderForeground = columnSettings.HeaderForeground.ToString(),
            HeaderBorderColor = columnSettings.HeaderBorderColor.ToString(),
            HeaderFontFamily = columnSettings.HeaderFontFamily,
            HeaderFontSize = columnSettings.HeaderFontSize,
            HeaderFontWeight = columnSettings.HeaderFontWeight.ToString(),
            HeaderVerticalAlignment = columnSettings.HeaderVerticalAlignment.ToString(),
            HeaderHorizontalAlignment = columnSettings.HeaderHorizontalAlignment.ToString(),
            LeftHeaderBorderThickness = columnSettings.HeaderBorderThickness.Left,
            TopHeaderBorderThickness = columnSettings.HeaderBorderThickness.Top,
            RightHeaderBorderThickness = columnSettings.HeaderBorderThickness.Right,
            BottomHeaderBorderThickness = columnSettings.HeaderBorderThickness.Bottom,
            SpecificSettings = columnSettings.SpecificSettings as ColumnSpecificSettingsBase 
                               ?? throw new InvalidOperationException("Error: Specific settings type not found")
        };
    }

    private ColumnDataType ParseColumnDataType(string dataTypeString)
    {
        return Enum.TryParse(dataTypeString, true, out ColumnDataType result) ? result : ColumnDataType.Text;
    }
    
    private Color ParseColor(string colorString)
    {
        if (string.IsNullOrEmpty(colorString)) return Colors.Transparent;

        try
        {
            return (Color)ColorConverter.ConvertFromString(colorString);
        }
        catch
        {
            return Colors.Transparent;
        }
    }
    
    private FontWeight ParseFontWeight(string fontWeightString)
    {
        return (FontWeight)(new FontWeightConverter().ConvertFromString(fontWeightString) ?? FontWeights.Normal);
    }
    
    private VerticalAlignment ParseVerticalAlignment(string alignmentString)
    {
        return Enum.TryParse(alignmentString, true, out VerticalAlignment result) ? result : VerticalAlignment.Stretch;
    }

    private HorizontalAlignment ParseHorizontalAlignment(string alignmentString)
    {
        return Enum.TryParse(alignmentString, true, out HorizontalAlignment result)
            ? result
            : HorizontalAlignment.Stretch;
    }
}