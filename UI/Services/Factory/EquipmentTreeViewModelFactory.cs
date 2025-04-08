using Core.Models.EquipmentTree;
using Core.Services.TabControlExt;
using Notification.Wpf;
using UI.Interfaces.Factory;
using UI.ViewModels.EquipmentTree;
using UI.ViewModels.Tabs;

namespace UI.Services.Factory;

/// <summary>
/// Factory responsible for creating instances of <see cref="EquipmentTreeViewModel"/>.
/// </summary>
public class EquipmentTreeViewModelFactory : IEquipmentTreeViewModelFactory
{
    private readonly IEventAggregator _eventAggregator;
    private readonly EquipmentTreeModel _model;
    private readonly NotificationManager _notificationManager;
    private readonly IRegionManager _regionManager;
    private readonly IRegionManagerService _regionManagerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentTreeViewModelFactory"/> class.
    /// </summary>
    /// <param name="eventAggregator">Event aggregator for pub-sub communication.</param>
    /// <param name="model">The model used for loading and managing equipment tree data.</param>
    /// <param name="notificationManager">Manager for showing notifications to the user.</param>
    /// <param name="regionManagerService">Service for managing scoped region managers.</param>
    public EquipmentTreeViewModelFactory(
        IEventAggregator eventAggregator,
        EquipmentTreeModel model,
        NotificationManager notificationManager,
        IRegionManagerService regionManagerService)
    {
        _eventAggregator = eventAggregator;
        _model = model;
        _notificationManager = notificationManager;
        _regionManagerService = regionManagerService;
    }

    /// <summary>
    /// Creates a new instance of <see cref="EquipmentTreeViewModel"/> configured for a specific menu type.
    /// </summary>
    /// <param name="menuType">The type of the menu to be set in the ViewModel.</param>
    /// <returns>A configured instance of <see cref="EquipmentTreeViewModel"/>.</returns>
    public EquipmentTreeViewModel Create(string menuType)
    {
        var vm = new EquipmentTreeViewModel(
            _eventAggregator,
            _model,
            _notificationManager,
            _regionManagerService);

        vm.MenuType = menuType;
        return vm;
    }
}
