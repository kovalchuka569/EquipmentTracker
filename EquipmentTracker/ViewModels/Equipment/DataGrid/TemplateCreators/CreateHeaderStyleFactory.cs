using System.Windows;
using System.Windows.Media;
using Models.Equipment;
using Syncfusion.UI.Xaml.Grid;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace UI.ViewModels.Equipment.DataGrid.TemplateCreators;

public class CreateHeaderStyleFactory
{
    public Style CreateHeaderStyle(ColumnSettings settings, Style baseGridHeaderStyle)
    {
        var style = new Style(typeof(GridHeaderCellControl), baseGridHeaderStyle);
        style.Setters.Add(new Setter(GridHeaderCellControl.BackgroundProperty, ConvertFromColor(settings.HeaderBackground)));
        style.Setters.Add(new Setter(GridHeaderCellControl.BorderThicknessProperty, settings.HeaderBorderThickness));
        style.Setters.Add(new Setter(GridHeaderCellControl.BorderBrushProperty, ConvertFromColor(settings.HeaderBorderColor)));
        style.Setters.Add(new Setter(GridHeaderCellControl.VerticalContentAlignmentProperty, VerticalAlignment.Stretch));
        style.Setters.Add(new Setter(GridHeaderCellControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
        return style;
    }   
    
    private static SolidColorBrush ConvertFromColor(Color color)
    { 
        return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
    }
}