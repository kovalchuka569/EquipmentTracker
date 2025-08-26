using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using Common.Logging;
using Core.Services.Summary;
using Data.Repositories.Summary;
using EquipmentTracker.Constants.Common;
using EquipmentTracker.Constants.Summary;
using EquipmentTracker.Events.Summary;
using Models.Summary.DataGrid;
using Notification.Wpf;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;

namespace EquipmentTracker.ViewModels.SummarySheet;

public class SummaryDataGridViewModel : BindableBase, INavigationAware, IDestructible
{
    private bool _isInitialised;
    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(_isInitialised) return;
        
        GetNavigationParameters(navigationContext.Parameters);
        
        SubscribeToEvents();
        
        await LoadSummaryData();
        
        _isInitialised = true;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) {}
    
    // DI
    private readonly ISummaryService _summaryService;
    private readonly IAppLogger<SummaryDataGridViewModel> _logger;
    private readonly NotificationManager _notificationManager;
    private readonly ISummaryRepository _summaryRepository;
   // private readonly IDialogManager _dialogManager;
    
    // UI data
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

    private ObservableCollection<ExpandoObject> _selectedItems = new();
    public ObservableCollection<ExpandoObject> SelectedItems
    {
        get => _selectedItems;
        set
        {
            if (SetProperty(ref _selectedItems, value))
            {
                Console.WriteLine("Selected items changed: " + SelectedItems.Count);
            }
        }
    }

    private StackedHeaderRows _stackedHeaderRows = new();
    public StackedHeaderRows StackedHeaderRows
    {
        get => _stackedHeaderRows;
        set => SetProperty(ref _stackedHeaderRows, value);
    }

    private bool _progressbarVisibility;
    public bool ProgressbarVisibility
    {
        get => _progressbarVisibility;
        set => SetProperty(ref _progressbarVisibility, value);
    }
    
    // Commands
    public DelegateCommand<SfDataGrid> SfDataGridLoadedCommand { get; set; }
    public DelegateCommand<CurrentCellRequestNavigateEventArgs> HyperlinkNavigateCommand { get; set; }
    public DelegateCommand ExportToExcelCommand { get; set; }
    public DelegateCommand ExportToPdfCommand { get; set; }
    public DelegateCommand PrintCommand { get; set; }
    
    // Constructor
    public SummaryDataGridViewModel(ISummaryService summaryService, 
        IAppLogger<SummaryDataGridViewModel> logger, 
        NotificationManager notificationManager, 
       // IDialogManager dialogManager,
        ISummaryRepository summaryRepository)
    {
        _summaryService = summaryService;
        _logger = logger;
        _notificationManager = notificationManager;
       // _dialogManager = dialogManager;
        _summaryRepository = summaryRepository;
        
        SfDataGridLoadedCommand = new DelegateCommand<SfDataGrid>(OnSfDataGridLoaded);
        HyperlinkNavigateCommand = new DelegateCommand<CurrentCellRequestNavigateEventArgs>(OnHyperlinkNavigate);
        ExportToExcelCommand = new DelegateCommand(OnExportToExcel);
        ExportToPdfCommand = new DelegateCommand(OnExportToPdf);
        PrintCommand = new DelegateCommand(OnPrint);
    }

    private void OnPrint()
    {
      //  _sfDataGrid.PrintSettings.PrintManagerBase = new PrintManager(_sfDataGrid);
        _sfDataGrid.ShowPrintPreview();
    }

    private void OnExportToExcel()
    {
     //   ExcelExportManager.ExportToExcel(_sfDataGrid, _summaryName, _notificationManager);
    }

    private void OnExportToPdf()
    {
      //  PdfExportManager.ExportToPdf(_sfDataGrid, _summaryName, _notificationManager);
    }

    private void OnSfDataGridLoaded(SfDataGrid sfDataGrid)
    {
        _sfDataGrid = sfDataGrid;
    }
    

    private void OnHyperlinkNavigate(CurrentCellRequestNavigateEventArgs e)
    {
        e.Handled = true;
        try
        {
            string url = e.NavigateText;
            
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri validUri))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = validUri.ToString(),
                    UseShellExecute = true
                });
            }
            else
            {
                _notificationManager.Show("Невідомий формат посилання: " + url, NotificationType.Information);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate to hyperlink");
        }
    }

    private async void Reload()
    {
        ProgressbarVisibility = true;
        try
        {
            await LoadSummaryData();
        }
        finally
        {
            await Task.Delay(300);
            ProgressbarVisibility = false;
        }
    }

    private async Task LoadSummaryData()
    {
        ProgressbarVisibility = true;
        try
        {
            var duplicatesToResolve = await _summaryService.GetPotentialDuplicateColumnInfosAsync(_summaryId); 

            if (duplicatesToResolve.Any())
            {
                await ResolveDuplicateAndNotifyUserAsync(duplicatesToResolve);
            }
            
            var columnDefinitions = await _summaryService.GetReportGridColumnsAsync(_summaryId);
            Columns.Clear();
            var columnFactory = new GridColumnFactory();
            foreach (var col in columnDefinitions)
            {
                Columns.Add(columnFactory.GetColumn(col.ColumnSettings, col.HeaderText, col.MappingName));
            }
    
            Items = await _summaryService.GetReportItemsAsync(_summaryId);
            
            
            var stackedHeaderRowDefinitions = await _summaryService.GetStackedHeaderRowsDefinitionsAsync(_summaryId);
            StackedHeaderRows.Clear();
            foreach (var rowDef in stackedHeaderRowDefinitions)
            {
                var newStackedHeaderRow = new StackedHeaderRow();
                foreach (var colDef in rowDef.StackedColumns)
                {
                    newStackedHeaderRow.StackedColumns.Add(new StackedColumn
                    {
                        HeaderText = colDef.HeaderText,
                        MappingName = colDef.MappingName,
                        ChildColumns = string.Join(",", colDef.ChildColumnMappingNames),
                    });
                }
                StackedHeaderRows.Add(newStackedHeaderRow);
            }
            _sfDataGrid.RefreshColumns();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load summary data");
            throw;
        }
        finally
        {
            await Task.Delay(300);
            ProgressbarVisibility = false;
        }
    }
    
    private async Task<bool> ResolveDuplicateAndNotifyUserAsync(List<DuplicateColumnInfo> duplicatesToResolve)
    {
        bool changesMade = false;
        string title = "У звіті виявлено дві характеристики з однаковим заголовком";
        foreach (var duplicateInfo in duplicatesToResolve)
        {
            string message = $"- Вихідна характеристика: '{duplicateInfo.ExistingColumn.HeaderText}' (з листа '{duplicateInfo.ExistingColumn.EquipmentSheetName}')\n" +
                             $"- Дублікат: '{duplicateInfo.DuplicateColumn.HeaderText}' (з листа '{duplicateInfo.DuplicateColumn.EquipmentSheetName}')\n\n" +
                             $"Об'єднати в одну характеристику?";

            _scopedEventAggregator.GetEvent<ShowSheetOverlayEvent>().Publish(true);
            bool result = false; //await _dialogManager.ShowInformationAgreementAsync(title, message, "Об'єднати", "Залишити окремо");
            _scopedEventAggregator.GetEvent<ShowSheetOverlayEvent>().Publish(false);

            if (result)
            {
                await _summaryRepository.UpdateEquipmentSummaryMergedStatus(duplicateInfo.DuplicateColumn.EquipmentSummaryEntryId, duplicateInfo.ExistingColumn.CustomColumnId, true, true);
                changesMade = true;
            }
            else
            {
                await _summaryRepository.UpdateEquipmentSummaryMergedStatus(
                    duplicateInfo.DuplicateColumn.EquipmentSummaryEntryId,
                    null, 
                    false, 
                    true); 
                changesMade = true;
            }
        }

        return changesMade;
    }
    
    // Navigation context data
    private IRegionManager _scopedRegionManager;
    private EventAggregator _scopedEventAggregator;
    private int _summaryId;
   // private SummaryFormat _summaryFormat;
    private string _summaryName;
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
        /*if (parameters[SummaryNavigationConstants.SummaryFormat] is SummaryFormat summaryFormat)
        {
            _summaryFormat = summaryFormat;
        }*/
        if (parameters[SummaryNavigationConstants.SummaryName] is string summaryName)
        {
            _summaryName = summaryName;
        }
    }

    public void Destroy()
    {
        if (StackedHeaderRows != null)
            StackedHeaderRows.Clear(); 
        else
            StackedHeaderRows = new StackedHeaderRows();  
        
        Items.Clear(); 
        Items = new ObservableCollection<ExpandoObject>(); 

        Columns.Clear(); 
        Columns = new Columns(); 

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