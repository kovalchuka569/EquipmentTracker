﻿using System.Windows;
using System.Windows.Markup;
using Models.Equipment;
using Models.Equipment.ColumnSpecificSettings;

namespace Core.Services.EquipmentDataGrid;

public class CreateCellTemplateFactory
{
     public DataTemplate CreateCellTemplate(ColumnSettings settings)
    {
        DataTemplate template;
        switch (settings.DataType)
        {
            case ColumnDataType.Currency:
                template = CreateCurrencyCellTemplate(settings);
                break;
            case ColumnDataType.List:
                template = CreateListCellTemplate(settings);
                break;
            case ColumnDataType.Boolean:
                template = CreateBooleanCellTemplate(settings);
                break;
            case ColumnDataType.Date:
                template = CreateDateCellTemplate(settings);
                break;
            case ColumnDataType.Hyperlink:
                template = CreateHyperlinkCellTemplate(settings);
                break;
            case ColumnDataType.Number:
                template = CreateNumberCellTemplate(settings);
                break;
            case ColumnDataType.Text:
                template = CreateTextCellTemplate(settings);
                break;
            case ColumnDataType.MultilineText:
                template = CreateMultilineTextCellTemplate(settings);
                break;
            default:
                template = CreateDefaultCellTemplate(settings);
                break;
        }
        return template;
    }
    
    private DataTemplate CreateBooleanCellTemplate(ColumnSettings settings)
    {
        string xamlTemplate =
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>

                <CheckBox IsChecked='{{Binding Path={settings.MappingName}}}' VerticalAlignment='Center' HorizontalAlignment='Center'/>

            </DataTemplate>";
        
        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }
    
    private DataTemplate CreateCurrencyCellTemplate(ColumnSettings settings)
    {
        var currencyColumnSettings = settings.SpecificSettings as CurrencyColumnSettings;
        string symbol = currencyColumnSettings?.CurrencySymbol ?? "$";
        
        string stringFormat = currencyColumnSettings?.PositionBefore == true
            ? $"{symbol} {{0:N2}}"
            : $"{{}}{{0:N2}} {symbol}";
        
        string xamlTemplate = 
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:converters='clr-namespace:EquipmentTracker.Views.Equipment.DataGrid.Converters;assembly=Equipment Tracker'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>
            
            <DataTemplate.Resources>
                <converters:NumberFormatConverter x:Key='NumberFormatConverter'/>
            </DataTemplate.Resources> 

                <TextBlock Text=""{{Binding Path={settings.MappingName}, StringFormat='{stringFormat}', Converter={{StaticResource NumberFormatConverter}}}}"" VerticalAlignment='Center' HorizontalAlignment='Center' TextWrapping='Wrap'/>

            </DataTemplate>";
        
        return  (DataTemplate)XamlReader.Parse(xamlTemplate);
    }
    
    private DataTemplate CreateListCellTemplate(ColumnSettings settings)
    {
        string xamlTemplate =
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>

                <TextBlock Text='{{Binding {settings.MappingName}}}' VerticalAlignment='Center' HorizontalAlignment='Center' TextWrapping='Wrap'/>

            </DataTemplate>";
        
        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }
    
    private DataTemplate CreateDateCellTemplate(ColumnSettings settings)
    {
        var dateSettings = settings.SpecificSettings as DateColumnSettings;
        
        string format = $"{{}}{{0:{dateSettings.DateFormat}}}" ?? "MM/dd/yyyy";
        
        string xamlTemplate = 
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>

                <TextBlock Text=""{{Binding Path={settings.MappingName}, StringFormat='{format}'}}"" Language='uk-UA' VerticalAlignment='Center' HorizontalAlignment='Center'/>

            </DataTemplate>";

        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }
    
    
    private DataTemplate CreateHyperlinkCellTemplate(ColumnSettings settings)
    {
        string xamlTemplate = 
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>

                <TextBlock Text=""{{Binding Path={settings.MappingName}}}"" Cursor='Hand' TextDecorations='Underline' VerticalAlignment='Center' HorizontalAlignment='Center' TextWrapping='Wrap'/>

            </DataTemplate>";

        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }
    
    private DataTemplate CreateNumberCellTemplate(ColumnSettings settings)
    {
        var numberColumnSettings = settings.SpecificSettings as NumberColumnSettings;
        string stringFormat = $"{{0:F{numberColumnSettings.CharactersAfterComma}}}";
        string xamlTemplate = 
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>

                <TextBlock Text='{{Binding Path={settings.MappingName}, StringFormat={{}}{stringFormat}}}' VerticalAlignment='Center' HorizontalAlignment='Center' TextWrapping='Wrap'/>

            </DataTemplate>";

        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }
    
    private DataTemplate CreateTextCellTemplate(ColumnSettings settings)
    {
        string xamlTemplate = 
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>

                <TextBlock Text=""{{Binding Path={settings.MappingName}}}"" VerticalAlignment='Center' HorizontalAlignment='Center' TextWrapping='Wrap'/>

            </DataTemplate>";

        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }
    
    private DataTemplate CreateMultilineTextCellTemplate(ColumnSettings settings)
    {
        string xamlTemplate = 
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>

                <TextBlock Text=""{{Binding Path={settings.MappingName}}}"" VerticalAlignment='Top' HorizontalAlignment='Left' TextWrapping='Wrap'/>

            </DataTemplate>";

        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }
    
    private DataTemplate CreateDefaultCellTemplate(ColumnSettings settings)
    {
        string xamlTemplate = 
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>

                <TextBlock Text=""{{Binding Path={settings.MappingName}}}"" VerticalAlignment='Center' HorizontalAlignment='Center' TextWrapping='Wrap'/>

            </DataTemplate>";

        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }
}