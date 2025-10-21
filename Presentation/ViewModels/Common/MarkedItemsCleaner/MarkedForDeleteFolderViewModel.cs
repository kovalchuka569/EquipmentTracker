using Common.Enums;

namespace Presentation.ViewModels.Common.MarkedItemsCleaner;

public class MarkedForDeleteFolderViewModel : BaseMarkedForDeleteItemViewModel
{
    
    public override MarkedForDeleteItemType MarkedForDeleteItemType => MarkedForDeleteItemType.Folder;
    
    public MarkedForDeleteFolderViewModel()
    {
        SetTitle("Folder");
    }
}