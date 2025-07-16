using System.Windows;
using System.Windows.Markup;
using Models.Equipment;

namespace EquipmentTracker.ViewModels.Equipment.DataGrid.TemplateCreators;

public class CreateHeaderTemplateFactory
{
    public DataTemplate CreateHeaderTemplate(ColumnSettings settings)
    {
        string xamlTemplate =
            $@"<DataTemplate 
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:sys='clr-namespace:System;assembly=mscorlib'>

                <TextBlock Text='{settings.HeaderText}' TextWrapping='Wrap'
                           FontSize='{settings.HeaderFontSize}' FontWeight='{settings.HeaderFontWeight}' FontFamily='{settings.HeaderFontFamily}' 
                           Foreground='{settings.HeaderForeground}'
                           VerticalAlignment='{settings.HeaderVerticalAlignment}' HorizontalAlignment='{settings.HeaderHorizontalAlignment}'/>
            
                </DataTemplate>";
        
        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }
}