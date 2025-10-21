using Common.Enums;

namespace Presentation.ViewModels.Common.MarkedItemsCleaner;

public class MarkedForDeleteEquipmentSheetViewModel : BaseMarkedForDeleteItemViewModel
{
    public override MarkedForDeleteItemType MarkedForDeleteItemType => MarkedForDeleteItemType.EquipmentSheet;
    
    public MarkedForDeleteEquipmentSheetViewModel()
    {
        SetTitle("Equipment Sheet");
    }
}