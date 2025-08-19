using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Shared;

using Models.Common.Table;
using Models.Common.Table.ColumnSpecificSettings;
using Models.Common.Table.ColumnValidationRules;
using Models.Equipment;

using Presentation.Interfaces;

namespace Presentation.UIManagers;

public class SyncfusionGridColumnManager(IGridInteractionManager interactionManager) : ISyncfusionGridColumnManager
{
    public GridColumn CreateColumn(ColumnModel columnModel, Style baseGridHeaderStyle)
    {
         switch (columnModel.DataType)
         {
            case ColumnDataType.Number:
                var numericColumnSpecificSettingsSpecificSettings = columnModel.SpecificSettings as NumericColumnSpecificSettings;
                var validationRules = columnModel.ValidationRules as NumericColumnValidationRules;
                return new GridNumericColumn
                {
                    MappingName = columnModel.MappingName,
                    HeaderText = columnModel.HeaderText,
                    ValueBinding = new Binding
                    {
                        Path = new PropertyPath(columnModel.MappingName),
                        Converter = new NullableDoubleToStringConverter
                        {
                            DecimalPlaces = numericColumnSpecificSettingsSpecificSettings?.NumberDecimalDigits ?? 2
                        }
                    },
                    HeaderStyle = CreateHeaderStyle(columnModel, baseGridHeaderStyle),
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    AllowFiltering = columnModel.AllowFiltering,
                    AllowSorting = columnModel.AllowSorting,
                    AllowGrouping = columnModel.AllowGrouping,
                    AllowEditing = !columnModel.IsReadOnly,
                    AllowDragging = true,
                    Width = columnModel.Width,
                    NumberDecimalSeparator = ",",
                    NumberDecimalDigits = numericColumnSpecificSettingsSpecificSettings?.NumberDecimalDigits ?? 0,
                    MaxValue = (decimal)validationRules?.MaxValue!,
                    AllowNullValue = true,
                    NullValue = null,
                };
            case ColumnDataType.Currency:
                var currencyColumnSpecificSettings = columnModel.SpecificSettings as CurrencyColumnSpecificSettings;
                return new GridNumericColumn
                {
                    HeaderStyle = CreateHeaderStyle(columnModel, baseGridHeaderStyle),
                    ValueBinding = new Binding
                    {
                        Path = new PropertyPath(columnModel.MappingName),
                        Converter = new CurrencyValueConverter()
                    },
                    
                    DisplayBinding = new Binding
                    {
                        Path = new PropertyPath(columnModel.MappingName),
                        Converter = new CurrencyDisplayConverter
                        {
                            CurrencySymbol = currencyColumnSpecificSettings?.CurrencySymbol ?? "$",
                            SymbolPosition = currencyColumnSpecificSettings?.CurrencyPosition ?? CurrencyPosition.After,
                        }
                        
                    },
                    MappingName = columnModel.MappingName,
                    HeaderText = columnModel.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    AllowFiltering = columnModel.AllowFiltering,
                    AllowSorting = columnModel.AllowSorting,
                    AllowGrouping = columnModel.AllowGrouping,
                    AllowEditing = !columnModel.IsReadOnly,
                    NumberDecimalSeparator = " ",
                    AllowDragging = true,
                    Width = columnModel.Width,
                    AllowNullValue = true,
                    NullValue = null,
                    ColumnMemberType = typeof(decimal?)
                };
            case ColumnDataType.Date:
                var dateColumnSpecificSettings = columnModel.SpecificSettings as DateColumnSpecificSettings;
                return new GridDateTimeColumn
                {
                    HeaderStyle = CreateHeaderStyle(columnModel, baseGridHeaderStyle),
                    ValueBinding = new Binding
                    {
                        Path = new PropertyPath(columnModel.MappingName),
                        Converter = new NullableDateTimeToStringConverter
                        {
                            Format = dateColumnSpecificSettings?.DateFormat ?? "d",
                        },
                    },
                    HeaderText = columnModel.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = columnModel.MappingName,
                    AllowFiltering = columnModel.AllowFiltering,
                    AllowSorting = columnModel.AllowSorting,
                    AllowGrouping = columnModel.AllowGrouping,
                    AllowEditing = !columnModel.IsReadOnly,
                    AllowDragging = true,
                    Width = columnModel.Width,
                    Pattern = DateTimePattern.CustomPattern,
                    CustomPattern = dateColumnSpecificSettings?.DateFormat ?? "d",
                    AllowNullValue = true,
                    NullValue = null,
                };
            case ColumnDataType.List:
                var comboBoxSpecificSettings = columnModel.SpecificSettings as ComboBoxColumnSpecificSettings;
                return new GridComboBoxColumn
                {
                    HeaderStyle = CreateHeaderStyle(columnModel, baseGridHeaderStyle),
                    ValueBinding = new Binding
                    {
                        Path = new PropertyPath(columnModel.MappingName),
                        TargetNullValue = ""
                    },
                    HeaderText = columnModel.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = columnModel.MappingName,
                    AllowFiltering = columnModel.AllowFiltering,
                    AllowSorting = columnModel.AllowSorting,
                    AllowGrouping = columnModel.AllowGrouping,
                    AllowEditing = !columnModel.IsReadOnly,
                    AllowDragging = true,
                    Width = columnModel.Width,
                    ItemsSource = comboBoxSpecificSettings?.ListValues,
                };
            case ColumnDataType.Boolean:
                return new GridCheckBoxColumn
                {
                    HeaderStyle = CreateHeaderStyle(columnModel, baseGridHeaderStyle),
                    ValueBinding = new Binding
                    {
                        Path = new PropertyPath(columnModel.MappingName),
                        TargetNullValue = ""
                    },
                    HeaderText = columnModel.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    MappingName = columnModel.MappingName,
                    AllowFiltering = columnModel.AllowFiltering,
                    AllowSorting = columnModel.AllowSorting,
                    AllowGrouping = columnModel.AllowGrouping,
                    AllowEditing = !columnModel.IsReadOnly,
                    AllowDragging = true,
                    Width = columnModel.Width,
                    UpdateTrigger = UpdateSourceTrigger.PropertyChanged,
                };
            case ColumnDataType.Hyperlink:
                return new GridTemplateColumn
                {
                    HeaderStyle = CreateHeaderStyle(columnModel, baseGridHeaderStyle),
                    CellTemplate = CreateHyperlinkCellTemplate(columnModel.MappingName),
                    EditTemplate = CreateHyperlinkEditTemplate(columnModel.MappingName),
                    ValueBinding = new Binding
                    {
                        Path = new PropertyPath(columnModel.MappingName),
                        TargetNullValue = ""
                    },
                    HeaderText = columnModel.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = columnModel.MappingName,
                    AllowFiltering = columnModel.AllowFiltering,
                    AllowSorting = columnModel.AllowSorting,
                    AllowGrouping = columnModel.AllowGrouping,
                    AllowEditing = !columnModel.IsReadOnly,
                    AllowDragging = true,
                    Width = columnModel.Width,
                };
            case ColumnDataType.Text:
            default: 
                return new GridTextColumn
                {
                    HeaderStyle = CreateHeaderStyle(columnModel, baseGridHeaderStyle),
                    ValueBinding = new Binding
                    {
                        Path = new PropertyPath(columnModel.MappingName),
                        TargetNullValue = ""
                    },
                    HeaderText = columnModel.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = columnModel.MappingName,
                    AllowFiltering = columnModel.AllowFiltering,
                    AllowSorting = columnModel.AllowSorting,
                    AllowGrouping = columnModel.AllowGrouping,
                    AllowEditing = !columnModel.IsReadOnly,
                    AllowDragging = true,
                    Width = columnModel.Width,  
                };
        }
    }
    
    

    private Style CreateHeaderStyle(ColumnModel columnModel, Style baseGridHeaderStyle)
    {
        var style = new Style(typeof(GridHeaderCellControl), baseGridHeaderStyle);
        style.Setters.Add(new Setter(GridHeaderCellControl.BackgroundProperty, ConvertFromColor(columnModel.HeaderBackground)));
        style.Setters.Add(new Setter(GridHeaderCellControl.ForegroundProperty, ConvertFromColor(columnModel.HeaderForeground)));
        style.Setters.Add(new Setter(GridHeaderCellControl.FontFamilyProperty, columnModel.HeaderFontFamily));
        style.Setters.Add(new Setter(GridHeaderCellControl.FontSizeProperty, columnModel.HeaderFontSize));
        style.Setters.Add(new Setter(GridHeaderCellControl.FontWeightProperty, columnModel.HeaderFontWeight));
        style.Setters.Add(new Setter(GridHeaderCellControl.BorderThicknessProperty, columnModel.HeaderBorderThickness));
        style.Setters.Add(new Setter(GridHeaderCellControl.BorderBrushProperty, ConvertFromColor(columnModel.HeaderBorderColor)));
        style.Setters.Add(new Setter(GridHeaderCellControl.VerticalContentAlignmentProperty, columnModel.HeaderVerticalAlignment));
        style.Setters.Add(new Setter(GridHeaderCellControl.HorizontalContentAlignmentProperty, columnModel.HeaderHorizontalAlignment));
        style.Setters.Add(new Setter(GridHeaderCellControl.IsEnabledProperty, !columnModel.IsReadOnly));
        return style;
    }
    
    private DataTemplate CreateHyperlinkCellTemplate(string mappingName)
    {
        var dataTemplate = new DataTemplate();

        var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
        textBlockFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        textBlockFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        textBlockFactory.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
        
        var hyperlinkFactory  = new FrameworkElementFactory(typeof(Hyperlink));
        var runFactory = new FrameworkElementFactory(typeof(Run));
        
        runFactory.SetBinding(Run.TextProperty, new Binding(mappingName));

        hyperlinkFactory.AppendChild(runFactory);

        hyperlinkFactory.AddHandler(Hyperlink.ClickEvent, new RoutedEventHandler(interactionManager.OnHyperlinkCellClick));

        textBlockFactory.AppendChild(hyperlinkFactory);
        dataTemplate.VisualTree = textBlockFactory;
        
        return dataTemplate;
    }
    
    private class NullableDoubleToStringConverter : IValueConverter
    {
        public int DecimalPlaces { get; set; } = 2;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
                return string.Empty;

            if (value is double d)
            {
                string format = $"F{DecimalPlaces}";
                return d.ToString(format, culture);
            }

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
                return null;

            if (double.TryParse(value.ToString(), NumberStyles.Any, culture, out var d))
                return d;

            return 0;
        }
    }
    
    private class CurrencyValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            try
            {
                var amount = System.Convert.ToDouble(value);
                return amount;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
        
            if (value == null)
            {
                return null;
            }

            try
            {
                var result = System.Convert.ToDouble(value);
                result = Math.Round(result, 2, MidpointRounding.AwayFromZero);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    
    private class CurrencyDisplayConverter : IValueConverter
    {
        public string CurrencySymbol { get; set; } = "$";

        public CurrencyPosition SymbolPosition { get; set; } = CurrencyPosition.After;
        
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            try
            {
                var amount = System.Convert.ToDouble(value);
                var formatted = SymbolPosition == CurrencyPosition.After 
                    ? $"{amount:N2} {CurrencySymbol}" 
                    : $"{CurrencySymbol} {amount:N2}"; 
                
                return formatted;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
        
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return null;
            }

            var strValue = value.ToString();
            var cleanValue = strValue?.Replace(CurrencySymbol, "").Replace(" ", "").Replace(",", ".");
            
            if (double.TryParse(cleanValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            return null;
        }
    }


    
    private class NullableDateTimeToStringConverter : IValueConverter
    {
        public string Format { get; set; } = "d";
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                null => string.Empty,
                DateTime dt => dt == default ? string.Empty : dt.ToString(Format, culture), 
                _ => value
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
                return null;

            if (DateTime.TryParse(value.ToString(), culture, DateTimeStyles.None, out var dt))
                return dt;

            return null;
        }
    }
    
    
    

    private static DataTemplate CreateHyperlinkEditTemplate(string mappingName)
    {
        var dataTemplate = new DataTemplate();
        var textBoxFactory = new FrameworkElementFactory(typeof(TextBox));
        textBoxFactory.SetBinding(TextBox.TextProperty, new Binding(mappingName)
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });
        dataTemplate.VisualTree = textBoxFactory;
        return dataTemplate;
    }
    
    private static SolidColorBrush ConvertFromColor(Color color)
    { 
        return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
    }
}