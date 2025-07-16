using Common.Logging;
using Core.Services.Summary;
using EquipmentTracker.Common;
using EquipmentTracker.Constants.Common;
using Models.EquipmentTree;
using EquipmentTracker.Constants.Summary;

namespace EquipmentTracker.ViewModels.SummarySheet;

public class SummarySheetViewModel : BindableBase, INavigationAware, IDestructible
{
    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        GetNavigationParameters(navigationContext.Parameters);
        
        await GetSummaryFormatAsync();
        
        NavigateTreeAndGrid();
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
    
    private void NavigateTreeAndGrid()
    {
        if (_scopedRegionManager is null || _scopedEventAggregator is null) return;
        
        var parameters = new NavigationParameters
        {
            { NavigationConstants.ScopedRegionManager, _scopedRegionManager },
            { NavigationConstants.ScopedEventAggregator, _scopedEventAggregator },
            { SummaryNavigationConstants.SummaryId, _summaryId },
            { SummaryNavigationConstants.SummaryFormat, _summaryFormat }
        };
        
        _scopedRegionManager.RequestNavigate(SummaryRegionConstants.SummaryColumnTreeRegion, SummaryViewConstants.SummaryColumnTreeView, parameters);
        _scopedRegionManager.RequestNavigate(SummaryRegionConstants.SummaryDataGridRegion, SummaryViewConstants.SummaryDataGridView, parameters);
    }

    
    // Navigation context data
    private IRegionManager? _scopedRegionManager;
    private EventAggregator? _scopedEventAggregator;
    private int _summaryId;
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
    }

    // Destroyer
    public void Destroy()
    {
        RegionCleaner.CleanUpRegions(_scopedRegionManager);
        _scopedRegionManager = null;
        _scopedEventAggregator = null;
    }
}