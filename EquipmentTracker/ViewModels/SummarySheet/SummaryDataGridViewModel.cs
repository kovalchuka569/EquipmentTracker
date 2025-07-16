using System.Collections.ObjectModel;
using System.Dynamic;
using System.Windows;
using Common.Logging;
using Core.Services.Summary;
using EquipmentTracker.Constants.Common;
using EquipmentTracker.Constants.Summary;
using EquipmentTracker.Events.Summary;
using Models.EquipmentTree;
using Models.Summary.DataGrid;
using Syncfusion.UI.Xaml.Grid;

namespace EquipmentTracker.ViewModels.SummarySheet;

public class SummaryDataGridViewModel : BindableBase, INavigationAware, IDestructible
{
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        GetNavigationParameters(navigationContext.Parameters);
        
        SubscribeToEvents();
        
        Reload();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) {}
    
    // DI
    private readonly ISummaryService _summaryService;
    private readonly IAppLogger<SummaryDataGridViewModel> _logger;
    
    // UI data
    private List<int> _columnIds = new();
    private SfDataGrid _sfDataGrid;
    private Columns _columns = new();
    public Columns Columns
    {
        get => _columns;
        set => SetProperty(ref _columns, value);
    }

    private ObservableCollection<ExpandoObject> _items = new();
    public ObservableCollection<ExpandoObject> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    private bool _progressbarVisibility;
    public bool ProgressbarVisibility
    {
        get => _progressbarVisibility;
        set => SetProperty(ref _progressbarVisibility, value);
    }
    
    // Commands
    public DelegateCommand<SfDataGrid> SfDataGridLoadedCommand { get; }
    
    // Constructor
    public SummaryDataGridViewModel(ISummaryService summaryService, IAppLogger<SummaryDataGridViewModel> logger)
    {
        _summaryService = summaryService;
        _logger = logger;
        
        SfDataGridLoadedCommand = new DelegateCommand<SfDataGrid>(OnSfDataGridLoaded);
    }

    private void OnSfDataGridLoaded(SfDataGrid sfDataGrid)
    {
        _sfDataGrid = sfDataGrid;
    }

    private async void Reload()
    {
        ProgressbarVisibility = true;
        try
        {
            await LoadColumns();
        }
        finally
        {
            await Task.Delay(300);
            ProgressbarVisibility = false;
        }
    }
    
    private async Task LoadColumns()
    {
        try
        {
            Columns = await _summaryService.GetMergedColumnsAsync(_summaryId, _summaryFormat);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading columns");
            throw;
        }
    }
    
    
    private async Task LoadRowsAsync()
    {
        var tableIds = Columns
            .OfType<CustomGridTemplateColumn>()
            .Select(c => c.TableId)
            .Distinct()
            .ToList();

        var rows = await _summaryService.GetDataAsync(tableIds, _summaryFormat);
        foreach (var row in rows)
            Items.Add(row);
    }
    
    private void ApplyMergesToItems(Dictionary<string, string> mergeMap)
    {
        foreach (dynamic dyn in Items)
        {
            var dict = (IDictionary<string, object>)dyn;
            foreach (var (src, dst) in mergeMap)
            {
                if (dict.TryGetValue(src, out var val) && val is not null)
                    dict[dst] = val;
            }
        }
    }

    // Navigation context data
    private IRegionManager _scopedRegionManager;
    private EventAggregator _scopedEventAggregator;
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
        if (parameters[SummaryNavigationConstants.SummaryFormat] is SummaryFormat summaryFormat)
        {
            _summaryFormat = summaryFormat;
        }
    }

    public void Destroy()
    {
        Columns = new Columns();
        Items = new ObservableCollection<ExpandoObject>();
        UnsubscribeFromEvents();
    }
    
    private void SubscribeToEvents()
    {
        _scopedEventAggregator.GetEvent<RefreshSummaryTrigger>().Subscribe(Reload);
    }
    
    private void UnsubscribeFromEvents()
    {
        _scopedEventAggregator.GetEvent<RefreshSummaryTrigger>().Unsubscribe(Reload);
    }
}