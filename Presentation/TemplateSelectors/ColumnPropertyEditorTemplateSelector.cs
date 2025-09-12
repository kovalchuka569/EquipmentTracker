using System.Windows;
using System.Windows.Controls;

using Presentation.Enums;
using Presentation.ViewModels.Common.ColumnDesigner;

namespace Presentation.TemplateSelectors;

public class ColumnPropertyEditorTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UndefinedEditorTemplate { get; set; }
    
    public DataTemplate? TextBoxEditorTemplate { get; set; }
    
    public DataTemplate? DefaultValueEditor { get; set; }
    
    public DataTemplate? IntegerTextBoxEditorTemplate { get; set; }
    
    public DataTemplate? MinMaxNumberEditorTemplate { get; set; }
    
    public DataTemplate? CheckBoxEditorTemplate { get; set; }
    
    public DataTemplate? DoubleTextBoxEditorTemplate { get; set; }
    
    public DataTemplate? DatePatternEditorTemplate { get; set; }
    
    public DataTemplate? CurrencySymbolEditorTemplate { get; set; }
    
    public DataTemplate? ListEditorTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is ColumnPropertyEntry entry)
        {
            return entry.EditorType switch
            {
                EditorType.None                 => UndefinedEditorTemplate,
                EditorType.TextBox              => TextBoxEditorTemplate,
                EditorType.DefaultValueEditor   => DefaultValueEditor,
                EditorType.MinMaxNumberEditor   => MinMaxNumberEditorTemplate,
                EditorType.IntegerTextBox       => IntegerTextBoxEditorTemplate,
                EditorType.CheckBox             => CheckBoxEditorTemplate,
                EditorType.DoubleTextBoxEditor  => DoubleTextBoxEditorTemplate,
                EditorType.DatePatternEditor    => DatePatternEditorTemplate,
                EditorType.CurrencySymbolEditor => CurrencySymbolEditorTemplate,
                EditorType.ListEditor           => ListEditorTemplate,
                _                               => UndefinedEditorTemplate
            };            
        }
        return base.SelectTemplate(item, container);
    }
}