using Core.Models.Tabs.ProductionEquipmentTree;
using Core.Services.TabControlExt;
using Notification.Wpf;

namespace UI.ViewModels.Tabs;

public class EquipmentTreeViewModelFactory : IEquipmentTreeViewModelFactory
{
    private readonly IEventAggregator _eventAggregator;
    private readonly EquipmentTreeModel _model;
    private readonly NotificationManager _notificationManager;
    private readonly IRegionManager _regionManager;
    private readonly IRegionManagerService _regionManagerService;
    
    public EquipmentTreeViewModelFactory(
        IEventAggregator eventAggregator,
        EquipmentTreeModel model,
        NotificationManager notificationManager,
        IRegionManager regionManager,
        IRegionManagerService regionManagerService)
    {
        _eventAggregator = eventAggregator;
        _model = model;
        _notificationManager = notificationManager;
        _regionManager = regionManager;
        _regionManagerService = regionManagerService;
    }
    
    public EquipmentTreeViewModel Create(string menuType)
    {
        var vm = new EquipmentTreeViewModel(
            _eventAggregator,
            _model,
            _notificationManager,
            _regionManager,
            _regionManagerService);
        
        vm.MenuType = menuType;
        return vm;
    }
}