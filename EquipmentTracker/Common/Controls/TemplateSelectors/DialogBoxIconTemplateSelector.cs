using System.Windows;
using System.Windows.Controls;

using Core.Common.Enums;

namespace EquipmentTracker.Common.Controls.TemplateSelectors;

public class DialogBoxIconTemplateSelector : DataTemplateSelector
{
    public DataTemplate TrashIconTemplate { get; set; }
    
    public DataTemplate InfoIconTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is DialogBoxIcon dialogBoxIcon)
        {
            return dialogBoxIcon switch
            {
                DialogBoxIcon.Trash => TrashIconTemplate,
                DialogBoxIcon.Info => InfoIconTemplate,
                _ => null
            };
        }
        
        return null;
    }
}