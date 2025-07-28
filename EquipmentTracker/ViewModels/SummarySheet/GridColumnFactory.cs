using System.Windows;
using Models.Equipment;
using Models.Equipment.ColumnSettings;
using Models.Equipment.ColumnSpecificSettings;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Shared;

namespace EquipmentTracker.ViewModels.SummarySheet;

public class GridColumnFactory
{
    public GridColumn GetColumn (ColumnSettingsDisplayModel settings, string headerText, string mappingName)
    {
        var dataType = settings.DataType;
        var parameters = new InternalParams
        {
            Settings = settings,
            HeaderText = headerText,
            MappingName = mappingName,
        };
        
        switch (dataType)
        {
            case ColumnDataType.Text:
                return GetTextColumn(parameters);
            case ColumnDataType.Number:
                return GetNumericColumn(parameters);
            case ColumnDataType.Boolean:
                return GetCheckBoxColumn(parameters);
            case ColumnDataType.Currency:
                return GetCurrencyColumn(parameters);
            case ColumnDataType.Hyperlink:
                return GetHyperlinkColumn(parameters);
            case ColumnDataType.Date:
                return GetDateTimeColumn(parameters);
            case ColumnDataType.MultilineText:
                return GetTextColumn(parameters);
            case ColumnDataType.List:
                return GetTextColumn(parameters);
            default: return GetTextColumn(parameters);
        }
    }

    private GridTextColumn GetTextColumn(InternalParams parameters)
    {
        return new GridTextColumn
        {
            HeaderText = parameters.HeaderText,
            MappingName = parameters.MappingName,
            AllowFiltering = true,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalHeaderContentAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
        };
    }

    private GridNumericColumn GetNumericColumn(InternalParams parameters)
    {
        var specificSettings = parameters.Settings.SpecificSettings as NumberColumnSettings;
        return new GridNumericColumn
        {
            HeaderText = parameters.HeaderText,
            MappingName = parameters.MappingName,
            NumberDecimalSeparator = ".",
            NumberDecimalDigits = specificSettings.CharactersAfterComma,
            AllowFiltering = true,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalHeaderContentAlignment = HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
        };
    }

    private GridCheckBoxColumn GetCheckBoxColumn(InternalParams parameters)
    {
        return new GridCheckBoxColumn
        {
            HeaderText = parameters.HeaderText,
            MappingName = parameters.MappingName,
            AllowFiltering = true,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalHeaderContentAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
        };
    }

    private GridCurrencyColumn GetCurrencyColumn(InternalParams parameters)
    {
        var specificSettings = parameters.Settings.SpecificSettings as CurrencyColumnSettings;
        return new GridCurrencyColumn
        {
            HeaderText = parameters.HeaderText,
            MappingName = parameters.MappingName,
            CurrencyDecimalSeparator = ",",
            CurrencySymbol = specificSettings.CurrencySymbol,
            AllowFiltering = true,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalHeaderContentAlignment = HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
        };
    }

    private GridHyperlinkColumn GetHyperlinkColumn(InternalParams parameters)
    {
        return new GridHyperlinkColumn
        {
            HeaderText = parameters.HeaderText,
            MappingName = parameters.MappingName,
            AllowFiltering = true,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalHeaderContentAlignment = HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
        };
    }

    private GridDateTimeColumn GetDateTimeColumn(InternalParams parameters)
    {
        var specificSettings = parameters.Settings.SpecificSettings as DateColumnSettings;
        return new GridDateTimeColumn
        {
            HeaderText = parameters.HeaderText,
            MappingName = parameters.MappingName,
            Pattern = DateTimePattern.CustomPattern,
            CustomPattern = specificSettings.DateFormat,
            AllowFiltering = true,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalHeaderContentAlignment = HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
        };
    }
    
    private class InternalParams 
    {
        public ColumnSettingsDisplayModel Settings { get; init; }
        public string HeaderText { get; init; }
        public string MappingName { get; init; }
    }
}
