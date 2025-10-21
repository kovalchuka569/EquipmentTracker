using Common.Enums;

namespace Presentation.ViewModels.Common.MarkedItemsCleaner;

public class MarkedForDeleteEquipmentSheetRowViewModel : BaseMarkedForDeleteItemViewModel
{
    public override MarkedForDeleteItemType MarkedForDeleteItemType => MarkedForDeleteItemType.EquipmentSheetRow;
    
    public MarkedForDeleteEquipmentSheetRowViewModel()
    {
        SetTitle("Equipment Sheet Row");
    }
}