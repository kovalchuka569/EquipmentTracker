using Prism.Mvvm;
using Prism.Regions;

namespace UI.ViewModels.DataGrid
{
    public class ServicesDataGridViewModel : BindableBase, INavigationAware
    {
        public ServicesDataGridViewModel()
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
}
