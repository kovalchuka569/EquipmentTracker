using System.Windows;
using System.Windows.Controls;
using Common.Enums;

namespace Presentation.TemplateSelectors;

public class MarkedItemsRemovingTemplateSelector : DataTemplateSelector
{
    public DataTemplate? RemovingTypeSelectorTemplate { get; set; }
    
    public DataTemplate? RemovingItemSelectionTemplate { get; set; }
    
    public DataTemplate? RemovingProcessTemplate { get; set; }
    
    public DataTemplate? RemovingItemsFinishTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is MarkedItemsRemovingStep stepType)
        {
            return stepType switch
            {
                MarkedItemsRemovingStep.RemovingTypeSelection => RemovingTypeSelectorTemplate,
                MarkedItemsRemovingStep.RemovingItemsSelection => RemovingItemSelectionTemplate,
                MarkedItemsRemovingStep.RemovingProcess => RemovingProcessTemplate,
                MarkedItemsRemovingStep.RemovingFinish => RemovingItemsFinishTemplate,
                _ => null
            };
        }
        return base.SelectTemplate(item, container);
    }
}