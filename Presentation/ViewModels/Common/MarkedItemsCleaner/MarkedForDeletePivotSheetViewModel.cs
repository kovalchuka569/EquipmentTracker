using Common.Enums;

namespace Presentation.ViewModels.Common.MarkedItemsCleaner;

public class MarkedForDeletePivotSheetViewModel : BaseMarkedForDeleteItemViewModel
{
    
    public override MarkedForDeleteItemType MarkedForDeleteItemType => MarkedForDeleteItemType.PivotSheet;
    
    public MarkedForDeletePivotSheetViewModel()
    {
        SetTitle("Pivot sheet Sheet");
    }
}