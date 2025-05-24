using Prism.Mvvm;
using Prism.Regions;

namespace EquipmentTracker.ViewModels.DataGrid;

public class EquipmentDataGridViewModel: BindableBase, INavigationAware
{
    public EquipmentDataGridViewModel()
    {
        
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
}