using Common.Enums;
using Common.Logging;
using Notification.Wpf;

namespace Presentation.ViewModels.Common.MarkedItemsCleaner;

public class MarkedForDeleteEquipmentSheetColumnViewModel : BaseMarkedForDeleteItemViewModel
{
    public override MarkedForDeleteItemType MarkedForDeleteItemType => MarkedForDeleteItemType.EquipmentSheetColumn;

    public MarkedForDeleteEquipmentSheetColumnViewModel()
    {
        SetTitle("Equipment Sheet Column");
    }
    
}