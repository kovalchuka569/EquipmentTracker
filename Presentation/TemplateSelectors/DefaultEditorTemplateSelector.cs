using System.Windows;
using System.Windows.Controls;

using Models.Equipment;

using Presentation.ViewModels.Common.ColumnDesigner;

namespace Presentation.TemplateSelectors;

public class DefaultEditorTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UndefinedEditorTemplate { get; set; }
    
    public DataTemplate? TextDefaultEditorTemplate { get; set; }
    
    public DataTemplate? BooleanDefaultEditorTemplate { get; set; }
    
    public DataTemplate? NumberDefaultEditorTemplate { get; set; }
    
    public DataTemplate? DateDefaultEditorTemplate { get; set; }
    
    public DataTemplate? CurrencyDefaultEditorTemplate { get; set; }
    
    public DataTemplate? ListDefaultEditorTemplate { get; set; }
    
    public DataTemplate? LinkDefaultEditorTemplate { get; set; }
    
    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is ColumnPropertyEntry entry)
        {
            return entry.Owner.ColumnDataType switch
            {
                ColumnDataType.None      => UndefinedEditorTemplate,
                ColumnDataType.Text      => TextDefaultEditorTemplate,
                ColumnDataType.Number    => NumberDefaultEditorTemplate,
                ColumnDataType.Date      => DateDefaultEditorTemplate,
                ColumnDataType.Currency  => CurrencyDefaultEditorTemplate,
                ColumnDataType.List      => ListDefaultEditorTemplate,
                ColumnDataType.Hyperlink => LinkDefaultEditorTemplate,
                _                        => UndefinedEditorTemplate
            };            
        }
        return base.SelectTemplate(item, container);
    }
}