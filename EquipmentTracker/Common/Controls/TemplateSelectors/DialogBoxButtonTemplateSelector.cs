using System.Windows;
using System.Windows.Controls;

using Core.Common.Enums;

namespace EquipmentTracker.Common.Controls.TemplateSelectors;

public class DialogBoxButtonTemplateSelector : DataTemplateSelector
{
    public DataTemplate DeleteCancelButtonsTemplate { get; set; }

    public DataTemplate YesNoButtonsTemplate { get; set; }
    
    public DataTemplate OkCancelButtonsTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is DialogBoxButtons dialogBoxButton)
        {
            return dialogBoxButton switch
            {
                DialogBoxButtons.DeleteCancel => DeleteCancelButtonsTemplate,
                DialogBoxButtons.YesNo => YesNoButtonsTemplate,
                DialogBoxButtons.OkCancel => OkCancelButtonsTemplate,
                _ => null
            };
        }

        return null;
    }
}