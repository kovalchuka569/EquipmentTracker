using System.Windows;
using System.Windows.Media;
using EquipmentTracker.Factories.Interfaces;
using EquipmentTracker.ViewModels.Equipment.DataGrid.TemplateCreators;
using Models.Equipment;
using Models.Equipment.ColumnSettings;
using Models.Equipment.ColumnSpecificSettings;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Shared;

namespace EquipmentTracker.Factories.Implementations.Syncfusion;

public class SyncfusionGridColumnFactory : IGridColumnFactory
{
    public GridColumn CreateColumn(ColumnSettingsDisplayModel settings)
    {
        
         switch (settings.DataType)
         {
            case ColumnDataType.Number:
                var numberSpecificSettings = settings.SpecificSettings as NumberColumnSettings;
                if (numberSpecificSettings == null) 
                {
                   
                }
                return new GridNumericColumn
                {
                    MappingName = settings.MappingName,
                    HeaderText = settings.HeaderText,
                    HeaderStyle = CreateHeaderStyle(settings),
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    AllowFiltering = settings.AllowFiltering,
                    AllowSorting = settings.AllowSorting,
                    AllowGrouping = settings.AllowGrouping,
                    AllowEditing = !settings.IsReadOnly,
                    Width = settings.ColumnWidth,
                    NumberDecimalSeparator = ",",
                    NumberDecimalDigits = numberSpecificSettings?.CharactersAfterComma ?? 0,
                    MaxValue = (decimal)numberSpecificSettings?.MaxValue!,
                };
            case ColumnDataType.Currency:
                var currencySpecificSettings = settings.SpecificSettings as CurrencyColumnSettings;
                if (currencySpecificSettings == null) { /* Handle error */ }
                return new GridCurrencyColumn
                {
                    HeaderStyle = CreateHeaderStyle(settings),
                    HeaderText = settings.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = settings.MappingName,
                    AllowFiltering = settings.AllowFiltering,
                    AllowSorting = settings.AllowSorting,
                    AllowGrouping = settings.AllowGrouping,
                    AllowEditing = !settings.IsReadOnly,
                    Width = settings.ColumnWidth,
                    CurrencyDecimalDigits = 2,
                    CurrencyDecimalSeparator = ",",
                    CurrencySymbol = currencySpecificSettings?.CurrencySymbol ?? "$",
                };
            case ColumnDataType.Date:
                var dateSpecificSettings = settings.SpecificSettings as DateColumnSettings;
                if (dateSpecificSettings == null) { /* Handle error */ }
                return new GridDateTimeColumn
                {
                    HeaderStyle = CreateHeaderStyle(settings),
                    HeaderText = settings.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = settings.MappingName,
                    AllowFiltering = settings.AllowFiltering,
                    AllowSorting = settings.AllowSorting,
                    AllowGrouping = settings.AllowGrouping,
                    AllowEditing = !settings.IsReadOnly,
                    Width = settings.ColumnWidth,
                    Pattern = DateTimePattern.CustomPattern,
                    CustomPattern = dateSpecificSettings?.DateFormat ?? "dd.MM.yyyy",
                    AllowNullValue = true,
                };
            case ColumnDataType.List:
                var listSpecificSettings = settings.SpecificSettings as ListColumnSettings;
                if (listSpecificSettings == null) { /* Handle error */ }
                return new GridComboBoxColumn
                {
                    HeaderStyle = CreateHeaderStyle(settings),
                    HeaderText = settings.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = settings.MappingName,
                    AllowFiltering = settings.AllowFiltering,
                    AllowSorting = settings.AllowSorting,
                    AllowGrouping = settings.AllowGrouping,
                    AllowEditing = !settings.IsReadOnly,
                    Width = settings.ColumnWidth,
                    ItemsSource = listSpecificSettings?.ListValues,
                };
            case ColumnDataType.Text:
            case ColumnDataType.MultilineText: 
            default: 
                return new GridTextColumn
                {
                    HeaderStyle = CreateHeaderStyle(settings),
                    HeaderText = settings.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = settings.MappingName,
                    AllowFiltering = settings.AllowFiltering,
                    AllowSorting = settings.AllowSorting,
                    AllowGrouping = settings.AllowGrouping,
                    AllowEditing = !settings.IsReadOnly,
                    Width = settings.ColumnWidth,
                };
        }
    }
    
    public Style CreateHeaderStyle(ColumnSettingsDisplayModel settings)
    {
        var style = new Style(typeof(GridHeaderCellControl));
        style.Setters.Add(new Setter(GridHeaderCellControl.BackgroundProperty, ConvertFromColor(settings.HeaderBackground)));
        style.Setters.Add(new Setter(GridHeaderCellControl.ForegroundProperty, ConvertFromColor(settings.HeaderForeground)));
        style.Setters.Add(new Setter(GridHeaderCellControl.FontFamilyProperty, new FontFamily(settings.HeaderFontFamily)));
        style.Setters.Add(new Setter(GridHeaderCellControl.FontSizeProperty, settings.HeaderFontSize));
        style.Setters.Add(new Setter(GridHeaderCellControl.FontWeightProperty, settings.HeaderFontWeight));
        style.Setters.Add(new Setter(GridHeaderCellControl.BorderThicknessProperty, settings.HeaderBorderThickness));
        style.Setters.Add(new Setter(GridHeaderCellControl.BorderBrushProperty, ConvertFromColor(settings.HeaderBorderColor)));
        style.Setters.Add(new Setter(GridHeaderCellControl.VerticalContentAlignmentProperty, settings.HeaderVerticalAlignment));
        style.Setters.Add(new Setter(GridHeaderCellControl.HorizontalContentAlignmentProperty, settings.HeaderHorizontalAlignment));
        style.Setters.Add(new Setter(GridHeaderCellControl.IsEnabledProperty, !settings.IsReadOnly));
        return style;
    }   
    
    private static SolidColorBrush ConvertFromColor(Color color)
    { 
        return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
    }
}