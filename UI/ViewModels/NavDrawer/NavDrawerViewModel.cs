using System.Windows;
using System.Windows.Controls;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using UI.ViewModels.TabControl;
using UI.Views.TabControl;
using UI.Views.Tabs.EquipmentTree;

namespace UI.ViewModels.NavDrawer
{
    public class NavDrawerViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        
        public DelegateCommand<string> NavigateToTabControlExt { get; }

        public NavDrawerViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            NavigateToTabControlExt = new DelegateCommand<string>(OnNavigateToTabControlExt);
        }
        

        private void OnNavigateToTabControlExt(string parameter)
        {
            Console.WriteLine("ShowTab");
            var parameters = new NavigationParameters
            {
                { "Parameter", parameter }
            };
            _regionManager.RequestNavigate("ContentRegion", "TabControlView", parameters);
        }
        

    }
}