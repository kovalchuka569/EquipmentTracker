using System;
using System.Collections.ObjectModel;
using Common.Enums;
using Common.Logging;
using Notification.Wpf;

namespace Presentation.ViewModels.Common.MarkedItemsCleaner;

public class MarkedForDeleteEquipmentSheetRowViewModel : BaseMarkedForDeleteItemViewModel
{
    public override MarkedForDeleteItemType MarkedForDeleteItemType => MarkedForDeleteItemType.EquipmentSheetRow;
    
    public MarkedForDeleteEquipmentSheetRowViewModel()
    {
        SetTitle("Equipment Sheet Row");
    }
}