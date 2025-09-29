using System;
using System.Collections.ObjectModel;
using Common.Enums;
using Common.Logging;
using Notification.Wpf;

namespace Presentation.ViewModels.Common.MarkedItemsCleaner;

public class MarkedForDeleteEquipmentSheetViewModel : BaseMarkedForDeleteItemViewModel
{
    public override MarkedForDeleteItemType MarkedForDeleteItemType => MarkedForDeleteItemType.EquipmentSheet;
    
    public MarkedForDeleteEquipmentSheetViewModel()
    {
        SetTitle("Equipment Sheet");
    }
}