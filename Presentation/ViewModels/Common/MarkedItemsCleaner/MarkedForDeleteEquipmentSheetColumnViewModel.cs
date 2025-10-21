using Common.Enums;

namespace Presentation.ViewModels.Common.MarkedItemsCleaner;

public class MarkedForDeleteEquipmentSheetColumnViewModel : BaseMarkedForDeleteItemViewModel
{
    public override MarkedForDeleteItemType MarkedForDeleteItemType => MarkedForDeleteItemType.EquipmentSheetColumn;

    public MarkedForDeleteEquipmentSheetColumnViewModel()
    {
        SetTitle("Equipment Sheet Column");
    }
    
}