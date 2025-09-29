using System;
using System.Collections.ObjectModel;
using Common.Enums;
using Common.Logging;
using Notification.Wpf;

namespace Presentation.ViewModels.Common.MarkedItemsCleaner;

public class MarkedForDeleteFolderViewModel : BaseMarkedForDeleteItemViewModel
{
    
    public override MarkedForDeleteItemType MarkedForDeleteItemType => MarkedForDeleteItemType.Folder;
    
    public MarkedForDeleteFolderViewModel()
    {
        SetTitle("Folder");
    }
}