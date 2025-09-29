using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Shared;

using Models.Equipment;
using Models.Common.Table.ColumnProperties;

using Presentation.Interfaces;

namespace Presentation.UIManagers;

public class SyncfusionGridColumnManager(IGridInteractionManager interactionManager) : ISyncfusionGridColumnManager
{

    private const string DeleteColumnToolTip = "Цей стовпець помічений для видалення";
    
    public GridColumn CreateColumn(BaseColumnProperties columnProps, Style baseGridHeaderStyle)
    {
         switch (columnProps.ColumnDataType)
         {
            case ColumnDataType.Number:
                if(columnProps is not NumberColumnProperties numberProps)
                    return new GridNumericColumn();
                
                return new GridNumericColumn
                {
                    HeaderStyle = CreateMarkedForDeleteHeaderStyle(baseGridHeaderStyle, columnProps.MarkedForDelete),
                    MappingName = columnProps.MappingName,
                    HeaderText = columnProps.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    AllowFiltering = true,
                    AllowSorting = true,
                    AllowGrouping = true,
                    AllowEditing = true,
                    AllowDragging = true,
                    UseBindingValue = true,
                    Width = columnProps.HeaderWidth,
                    NumberDecimalSeparator = ",",
                    NumberDecimalDigits = numberProps.NumberDecimalDigits,
                    AllowNullValue = true,
                    NullText = string.Empty
                };
            case ColumnDataType.Currency:
                if(columnProps is not CurrencyColumnProperties currencyProps)
                    return new GridCurrencyColumn();
                
                return new GridCurrencyColumn
                {
                    HeaderStyle = CreateMarkedForDeleteHeaderStyle(baseGridHeaderStyle, columnProps.MarkedForDelete),
                    MappingName = columnProps.MappingName,
                    HeaderText = columnProps.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    AllowFiltering = true,
                    AllowSorting = true,
                    AllowGrouping = true,
                    AllowEditing = true,
                    CurrencyDecimalSeparator = ".",
                    CurrencyGroupSeparator = " ",
                    CurrencyGroupSizes = [3],
                    CurrencyDecimalDigits = 2,  
                    CurrencySymbol = currencyProps.Symbol,
                    AllowDragging = true,
                    UseBindingValue = true,
                    Width = columnProps.HeaderWidth,
                    AllowNullValue = true,
                    NullText = string.Empty,
                    ColumnMemberType = typeof(decimal?)
                };
            case ColumnDataType.Date:
                if(columnProps is not DateColumnProperties dateColumnProps)
                    return new GridDateTimeColumn();
                
                return new GridDateTimeColumn
                {
                    HeaderStyle = CreateMarkedForDeleteHeaderStyle(baseGridHeaderStyle, columnProps.MarkedForDelete),
                    HeaderText = columnProps.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = columnProps.MappingName,
                    AllowFiltering = true,
                    AllowSorting = true,
                    AllowGrouping = true,
                    AllowEditing = true,
                    AllowDragging = true,
                    UseBindingValue = true,
                    Width = columnProps.HeaderWidth,
                    Pattern = DateTimePattern.CustomPattern,
                    CustomPattern = dateColumnProps.Pattern,
                    AllowNullValue = true,
                    NullText = string.Empty,
                };
            case ColumnDataType.List:
                if (columnProps is not ListColumnProperties listColumnProps)
                    return new GridComboBoxColumn();
                
                return new GridComboBoxColumn
                {
                    HeaderStyle = CreateMarkedForDeleteHeaderStyle(baseGridHeaderStyle, columnProps.MarkedForDelete),
                    HeaderText = columnProps.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = columnProps.MappingName,
                    AllowFiltering = true,
                    AllowSorting = true,
                    AllowGrouping = true,
                    AllowEditing = true,
                    AllowDragging = true,
                    UseBindingValue = true,
                    Width = columnProps.HeaderWidth,
                    ItemsSource = listColumnProps.ListValues,
                };
            case ColumnDataType.Hyperlink:
                return new GridTemplateColumn
                {
                    HeaderStyle = CreateMarkedForDeleteHeaderStyle(baseGridHeaderStyle, columnProps.MarkedForDelete),
                    CellTemplate = CreateHyperlinkCellTemplate(columnProps.MappingName),
                    EditTemplate = CreateHyperlinkEditTemplate(columnProps.MappingName),
                    HeaderText = columnProps.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = columnProps.MappingName,
                    AllowFiltering = true,
                    AllowSorting = true,
                    AllowGrouping = true,
                    AllowEditing = true,
                    AllowDragging = true,
                    UseBindingValue = true,
                    Width = columnProps.HeaderWidth,
                };
            case ColumnDataType.Text:
            default: 
                return new GridTextColumn
                {
                    HeaderStyle = CreateMarkedForDeleteHeaderStyle(baseGridHeaderStyle, columnProps.MarkedForDelete),
                    HeaderText = columnProps.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = columnProps.MappingName,
                    AllowFiltering = true,
                    AllowSorting = true,
                    AllowGrouping = true,
                    AllowEditing = true,
                    AllowDragging = true,
                    UseBindingValue = true,
                    Width = columnProps.HeaderWidth,  
                };
        }
    }

    private static Style CreateMarkedForDeleteHeaderStyle(Style baseGridHeaderStyle, bool markedForDelete)
    {
        var style = new Style(typeof(GridHeaderCellControl), baseGridHeaderStyle);
    
        if(!markedForDelete)
            return style;
    
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, Brushes.Red));
        style.Setters.Add(new Setter(FrameworkElement.ToolTipProperty, DeleteColumnToolTip));
        
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