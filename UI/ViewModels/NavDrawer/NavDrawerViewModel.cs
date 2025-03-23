using System.Windows;
using System.Windows.Controls;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using UI.ViewModels.TabControl;
using UI.Views.TabControl;
using UI.Views.Tabs.ProductionEquipmentTree;

namespace UI.ViewModels.NavDrawer
{
    public class NavDrawerViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        public NavDrawerViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            ShowTabCommand = new DelegateCommand<string>(ShowTab);
        }

        public DelegateCommand<string> ShowTabCommand { get; }

        private void ShowTab(string header)
        {
            var parameters = new NavigationParameters
            {
                { "SelectedTab", header }
            };
            _regionManager.RequestNavigate("ContentRegion", "TabControlView", parameters);
        }
        

    }
}