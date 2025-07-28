using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Models.Equipment;
using Models.Equipment.ColumnSettings;
using Syncfusion.UI.Xaml.Grid;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace EquipmentTracker.ViewModels.Equipment.DataGrid.TemplateCreators;

public class CreateHeaderStyleFactory
{
    public Style CreateHeaderStyle(ColumnSettingsDisplayModel settings, Style baseGridHeaderStyle)
    {
        var style = new Style(typeof(GridHeaderCellControl), baseGridHeaderStyle);
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