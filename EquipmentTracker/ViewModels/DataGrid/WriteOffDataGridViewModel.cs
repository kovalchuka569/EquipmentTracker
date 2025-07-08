

using UI.ViewModels.TabControl;

namespace UI.ViewModels.DataGrid
{
    public class WriteOffDataGridViewModel : BindableBase, INavigationAware, IRegionCleanup
    {
        public WriteOffDataGridViewModel()
        {
            Console.WriteLine("WriteOffDataGridViewModel constructor");
        }
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Console.WriteLine("OnNavigatedTo WriteOffDataGridViewModel");
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => false;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void CleanupRegions()
        {
        }
    }
}
