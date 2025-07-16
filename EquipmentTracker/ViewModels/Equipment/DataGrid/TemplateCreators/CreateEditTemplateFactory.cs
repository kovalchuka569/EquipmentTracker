using System.Collections.ObjectModel;
using System.Globalization;
using System.Security;
using System.Windows;
using System.Windows.Markup;
using Models.Equipment;
using Models.Equipment.ColumnSpecificSettings;

namespace EquipmentTracker.ViewModels.Equipment.DataGrid.TemplateCreators;

public class CreateEditTemplateFactory
{
    public DataTemplate CreateEditTemplate(ColumnSettings settings)
    {
        string mappingName = settings.MappingName;
        
        switch (settings.DataType)
        {
            case ColumnDataType.Text:
               var textColumnSettings = settings.SpecificSettings as TextColumnSettings;
               return CreateEditTextTemplate(textColumnSettings, mappingName);
            case ColumnDataType.MultilineText:
                var multilineTextColumnSettings = settings.SpecificSettings as MultilineTextColumnSettings;
                return CreateEditMultilineTemplate(multilineTextColumnSettings, mappingName);
            case ColumnDataType.List:
                var listColumnSettings = settings.SpecificSettings as ListColumnSettings;
                return CreateEditListTemplate(listColumnSettings, mappingName);
            case ColumnDataType.Number:
                var numberColumnSettings = settings.SpecificSettings as NumberColumnSettings;
                return CreateEditNumericTemplate(numberColumnSettings, mappingName);
            case ColumnDataType.Currency:
                var currencyColumnSettings = settings.SpecificSettings as CurrencyColumnSettings;
                return CreateEditCurrencyTemplate(currencyColumnSettings, mappingName);
            case ColumnDataType.Boolean:
                var booleanColumnSettings = settings.SpecificSettings as BooleanColumnSettings;
                return CreateEditBooleanTemplate(booleanColumnSettings, mappingName);
            case ColumnDataType.Hyperlink:
                return CreateEditHyperlinkTemplate(mappingName);
            case ColumnDataType.Date:
                var dateColumnSettings = settings.SpecificSettings as DateColumnSettings;
                return CreateEditDateTemplate(dateColumnSettings, mappingName);
        }
        return null;
    }
    
    private DataTemplate CreateEditTextTemplate(TextColumnSettings textColumnSettings, string mappingName)
    {
        string xamlTemplate =
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>

                <TextBox Text='{{Binding Path={mappingName}, UpdateSourceTrigger=PropertyChanged}}' VerticalAlignment='Center' HorizontalAlignment='Stretch' Margin='5,0,5,0' TextWrapping='Wrap'/>

            </DataTemplate>";
        
        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }

    private DataTemplate CreateEditNumericTemplate(NumberColumnSettings numberColumnSettings, string mappingName)
    {
        string min = numberColumnSettings.MinValue != 0 
            ? $" MinValue='{numberColumnSettings.MinValue.ToString(CultureInfo.InvariantCulture)}'" 
            : string.Empty;

        string max = numberColumnSettings.MaxValue != 0 
            ? $" MaxValue='{numberColumnSettings.MaxValue.ToString(CultureInfo.InvariantCulture)}'" 
            : string.Empty;
        
        string xamlTemplate =
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'
                xmlns:sf='http://schemas.syncfusion.com/wpf'>
                    
                <sf:DoubleTextBox Value='{{Binding {mappingName}, UpdateSourceTrigger=PropertyChanged}}' NumberDecimalDigits='{numberColumnSettings.CharactersAfterComma}' 
                                  {min}{max}
                                  VerticalAlignment='Center' HorizontalAlignment='Stretch' Culture='uk-UA' Margin='5,0,5,0'/>

            </DataTemplate>";
        
        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }

    private DataTemplate CreateEditListTemplate(ListColumnSettings listColumnSettings, string mappingName)
    {
        ObservableCollection<string> items = listColumnSettings.ListValues;
        string comboBoxItems = string.Join("", items.Select(item => 
            $"<ComboBoxItem Content=\"{SecurityElement.Escape(item)}\"/>"));
        
        string xamlTemplate =
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>

                <ComboBox SelectedValue='{{Binding {mappingName}, UpdateSourceTrigger=PropertyChanged}}' SelectedValuePath='Content' VerticalAlignment='Center' HorizontalAlignment='Stretch' Margin='5,0,5,0'>
                {comboBoxItems}
                </ComboBox>
            
            </DataTemplate>";
        
        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }

    private DataTemplate CreateEditHyperlinkTemplate(string mappingName)
    {
        string xamlTemplate =
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>
                    
                <TextBox Text='{{Binding {mappingName}, UpdateSourceTrigger=PropertyChanged}}' VerticalAlignment='Center' HorizontalAlignment='Stretch' Margin='5,0,5,0'/>

            </DataTemplate>";
        
        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }

   private DataTemplate CreateEditDateTemplate(DateColumnSettings dateColumnSettings, string mappingName)
    {
        string format = $"{dateColumnSettings.DateFormat}" ?? "MM/dd/yyyy";
        
        string xamlTemplate =
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sf='http://schemas.syncfusion.com/wpf'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>
                    
                <sf:DateTimeEdit DateTime='{{Binding {mappingName}, UpdateSourceTrigger=PropertyChanged}}'  Pattern='CustomPattern' CustomPattern='{format}' VerticalAlignment='Center' HorizontalAlignment='Stretch' Margin='5,0,5,0'/>

            </DataTemplate>";
        
        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }

   private DataTemplate CreateEditCurrencyTemplate(CurrencyColumnSettings currencyColumnSettings, string mappingName)
   {
       string symbol = currencyColumnSettings?.CurrencySymbol ?? "$";

       string before = "Collapsed";
       string after = "Collapsed";

       if (currencyColumnSettings.PositionBefore)
           before = "Visible";
       else if (currencyColumnSettings.PositionAfter)
           after = "Visible";
       
        
       string xamlTemplate = 
           $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sf='http://schemas.syncfusion.com/wpf'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>
                
                <Grid VerticalAlignment='Center' HorizontalAlignment='Stretch' Margin='5,0,5,0'>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width='Auto'/>
                        <ColumnDefinition Width='*'/>
                        <ColumnDefinition Width='Auto'/>
                    </Grid.ColumnDefinitions>

                <TextBlock Grid.Column='0' Text='{symbol}' Visibility='{before}' HorizontalAlignment='Right' VerticalAlignment='Center' />

                    <sf:CurrencyTextBox Grid.Column='1' Value='{{Binding Path={mappingName}, UpdateSourceTrigger=PropertyChanged}}' CurrencySymbol='' CurrencyDecimalDigits='2' 
                                        CurrencyDecimalSeparator='.' TextWrapping='Wrap' Margin='4,0,4,0'/>

                <TextBlock Grid.Column='2' Text='{symbol}' Visibility='{after}' HorizontalAlignment='Left' VerticalAlignment='Center' />

                </Grid>

            </DataTemplate>";
       return (DataTemplate)XamlReader.Parse(xamlTemplate);
   }

    private DataTemplate CreateEditBooleanTemplate(BooleanColumnSettings booleanColumnSettings, string mappingName)
    {
        string xamlTemplate =
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>

                <CheckBox IsChecked='{{Binding Path={mappingName}, UpdateSourceTrigger=PropertyChanged}}' VerticalAlignment='Center' HorizontalAlignment='Center'/>

            </DataTemplate>";
        
        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }

    private DataTemplate CreateEditMultilineTemplate(MultilineTextColumnSettings multilineTextColumnSettings, string mappingName)
    {
        string xamlTemplate =
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>

                <TextBox Text='{{Binding Path={mappingName}, UpdateSourceTrigger=PropertyChanged}}' AcceptsReturn='True' MaxLength='{multilineTextColumnSettings.MaxLength}' HorizontalContentAlignment='Left' VerticalContentAlignment='Top' VerticalAlignment='Stretch' HorizontalAlignment='Stretch' TextWrapping='Wrap' Margin='5,0,5,0'/>

            </DataTemplate>";
        
        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }
}