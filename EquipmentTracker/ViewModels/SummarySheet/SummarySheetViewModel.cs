using Common.Logging;
using Core.Services.Summary;
using EquipmentTracker.Common;
using EquipmentTracker.Constants.Common;
using Models.EquipmentTree;
using EquipmentTracker.Constants.Summary;
using EquipmentTracker.Events.Summary;

namespace EquipmentTracker.ViewModels.SummarySheet;

public class SummarySheetViewModel : BindableBase, INavigationAware, IDestructible
{
    private bool _isInitialized;
    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(_isInitialized) return;
        
        GetNavigationParameters(navigationContext.Parameters);
        
        SubscribeToEvents();
        
        await GetSummaryFormatAsync();
        
        NavigateTreeAndGrid();
        
        _isInitialized = true;
    }
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) {}
    
    // DI
    private readonly ISummaryService _summaryService;
    private readonly IAppLogger<SummarySheetViewModel> _logger;
    
    // UI data
    private bool _progressbarVisibility;
    public bool ProgressbarVisibility
    {
        get => _progressbarVisibility;
        set => SetProperty(ref _progressbarVisibility, value);
    }

    private bool _isOverlayVisible;
    public bool IsOverlayVisible
    {
        get => _isOverlayVisible;
        set => SetProperty(ref _isOverlayVisible, value);
    }
    

    // Constructor
    public SummarySheetViewModel(ISummaryService summaryService, IAppLogger<SummarySheetViewModel> logger)
    {
        _summaryService = summaryService;
        _logger = logger;
    }
    
    private async Task GetSummaryFormatAsync()
    {
        try
        {
            _summaryFormat = await _summaryService.GetSummaryFormat(_summaryId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting summary format");
            throw;
        }
    }
    
    private void ShowOverlay(bool isVisible)
    {
        IsOverlayVisible = isVisible;
    }
    
    private void NavigateTreeAndGrid()
    {
        if (_scopedRegionManager is null || _scopedEventAggregator is null) return;
        
        var parameters = new NavigationParameters
        {
            { NavigationConstants.ScopedRegionManager, _scopedRegionManager },
            { NavigationConstants.ScopedEventAggregator, _scopedEventAggregator },
            { SummaryNavigationConstants.SummaryId, _summaryId },
            { SummaryNavigationConstants.SummaryFormat, _summaryFormat },
            { SummaryNavigationConstants.SummaryName, _summaryName },
        };
        
        _scopedRegionManager.RequestNavigate(SummaryRegionConstants.SummaryColumnTreeRegion, SummaryViewConstants.SummaryColumnTreeView, parameters);
        _scopedRegionManager.RequestNavigate(SummaryRegionConstants.SummaryDataGridRegion, SummaryViewConstants.SummaryDataGridView, parameters);
    }

    
    // Navigation context data
    private IRegionManager? _scopedRegionManager;
    private EventAggregator? _scopedEventAggregator;
    private int _summaryId;
    private string _summaryName;
    private SummaryFormat _summaryFormat;
    private void GetNavigationParameters(INavigationParameters parameters)
    {
        if (parameters[NavigationConstants.ScopedRegionManager] is IRegionManager scopedRegionManager)
        {
            _scopedRegionManager = scopedRegionManager;
        }
        if (parameters[NavigationConstants.ScopedEventAggregator] is EventAggregator scopedEventAggregator)
        {
            _scopedEventAggregator = scopedEventAggregator;
        }
        if (parameters[SummaryNavigationConstants.SummaryId] is int summaryId)
        {
            _summaryId = summaryId;
        }
        if (parameters[SummaryNavigationConstants.SummaryName] is string summaryName)
        {
            _summaryName = summaryName;
        }
    }

    private void SubscribeToEvents()
    {
        _scopedEventAggregator?.GetEvent<ShowSheetOverlayEvent>().Subscribe(ShowOverlay);
    }

    private void UnsubscribeFromEvents()
    {
        _scopedEventAggregator?.GetEvent<ShowSheetOverlayEvent>().Unsubscribe(ShowOverlay);
    }

    // Destroyer
    public void Destroy()
    {
        UnsubscribeFromEvents();
        RegionCleaner.CleanUpRegions(_scopedRegionManager);
        _scopedRegionManager = null;
        _scopedEventAggregator = null;
    }
    
}