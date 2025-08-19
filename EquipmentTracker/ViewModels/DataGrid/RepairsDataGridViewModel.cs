using System.Collections.ObjectModel;
using System.Drawing;
using Common.Logging;
using Core.Events.TabControl;
using Core.Services.RepairsDataGrid;
using Microsoft.Win32;
using Prism.Mvvm;
using Prism.Commands;
using Models.RepairsDataGrid;
using Models.RepairsDataGrid.AddRepair;
using Notification.Wpf;
using Prism.Events;
using Syncfusion.Pdf;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.XlsIO;

namespace UI.ViewModels.DataGrid
{
    public class RepairsDataGridViewModel : BindableBase, INavigationAware
    {
        private readonly IAppLogger<RepairsDataGridViewModel> _logger;
        private readonly IRepairsDataGridService _service;
        private readonly NotificationManager _notificationManager;
        private readonly IEventAggregator _eventAggregator;
        
        private SfDataGrid _repairsDataGrid;

        private string _repairsTableName;
        private string _equipmentTableName;
        
        private ObservableCollection<RepairItem> _repairs = new();
        private RepairItem _selectedRepair;
        
        private bool _progressBarVisibility;
        private bool _editRepairContextMenuVisibility;

        public ObservableCollection<RepairItem> Repairs
        {
            get => _repairs;
            set => SetProperty(ref _repairs, value);
        }
        public RepairItem SelectedRepair
        {
            get => _selectedRepair;
            set => SetProperty(ref _selectedRepair, value);
        }
        public bool ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => SetProperty(ref _progressBarVisibility, value);
        }

        public bool EditRepairContextMenuVisibility
        {
            get => _editRepairContextMenuVisibility;
            set => SetProperty(ref _editRepairContextMenuVisibility, value);
        }
        
        public DelegateCommand<SfDataGrid> RepairsDataGridLoadedCommand { get; set; }
        public DelegateCommand ContextMenuLoadedCommand { get; set; }
        public DelegateCommand CreateNewRepairCommand { get; set; }
        public DelegateCommand EditRepairCommand { get; set; }
        public DelegateCommand RefreshCommand { get; set; }
        public DelegateCommand PrintCommand {get; set; }
        public DelegateCommand ExcelExportCommand { get; set; }
        public DelegateCommand PdfExportCommand { get; set; }
        
        public RepairsDataGridViewModel(IAppLogger<RepairsDataGridViewModel> logger, 
            IRepairsDataGridService service,
            IEventAggregator eventAggregator,
            NotificationManager notificationManager)
        {
            _logger = logger;
            _service = service;
            _eventAggregator = eventAggregator;
            _notificationManager = notificationManager;

            RepairsDataGridLoadedCommand = new DelegateCommand<SfDataGrid>(OnRepairsDataGridLoaded);
            ContextMenuLoadedCommand = new DelegateCommand(OnContextMenuLoaded);
            CreateNewRepairCommand = new DelegateCommand(OnCreateNewRepair);
            EditRepairCommand = new DelegateCommand(OnEditRepair);
            RefreshCommand = new DelegateCommand(LoadRepairsDataAsync);
            PrintCommand = new DelegateCommand(OnPrint);
            ExcelExportCommand = new DelegateCommand(OnExcelExport);
            PdfExportCommand = new DelegateCommand(OnPdfExport);
        }

        private void OnEditRepair()
        {
            if (SelectedRepair is RepairItem repair)
            {
                _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
                {
                    Header = $"{repair.EquipmentInventoryNumber} | {repair.EquipmentBrand} | {repair.EquipmentModel} - редагування ремонту",
                    Parameters = new Dictionary<string, object>
                    {
                        {"ViewNameToShow", "AddRepairView"},
                        {"AddRepairView.RepairItem", repair},
                        {"AddRepairView.EquipmentTableName", _equipmentTableName}
                    }
                });
            }
        }

        private void OnContextMenuLoaded()
        {
            if (SelectedRepair is RepairItem)
            {
                EditRepairContextMenuVisibility = true;
            }
        }

        private void OnCreateNewRepair()
        {
            _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
            {
                Header = _equipmentTableName + " створення ремонту",
                Parameters = new Dictionary<string, object>
                {
                    {"ViewNameToShow", "AddRepairView"},
                    {"AddRepairView.EquipmentTableName", _equipmentTableName}
                }
            });
        }
        
        private void OnExcelExport()
        {
            try
            {
                var options = new ExcelExportingOptions()
                {
                    ExcelVersion = ExcelVersion.Excel2016
                };
            
                var excelEngine = _repairsDataGrid.ExportToExcel(_repairsDataGrid.View, options);
                var workbook = excelEngine.Excel.Workbooks[0];

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    Title = $"Експорт в Excel: Ремонти - {_equipmentTableName}, {DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}",
                    FileName = $"Ремонти - {_equipmentTableName}, {DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    using (var stream = saveDialog.OpenFile())
                    {
                        workbook.SaveAs(stream);
                    }
                    _notificationManager.Show("Таблицю успішно експортовано до Excel!", NotificationType.Success);
                }
            }
            catch (Exception e)
            {
                _notificationManager.Show("Помилка експорту до Excel!", NotificationType.Error);
                _logger.LogError(e.Message, "Error exporting Excel");
                throw;
            }
        }
        
        private void OnPdfExport()
        {
            try
            {
                PdfExportingOptions options = new PdfExportingOptions();
                // Delete notes column
                options.ExcludeColumns.Add("Notes");
                options.FitAllColumnsInOnePage = true;
                var pdfGrid = _repairsDataGrid.ExportToPdfGrid(_repairsDataGrid.View, options);

                using (PdfDocument document = new PdfDocument())
                {
                    var page = document.Pages.Add();
                    pdfGrid.Draw(page, new PointF(0, 0));

                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "PDF files (*.pdf)|*.pdf",
                        Title = $"Експорт в PDF: Ремонти - {_equipmentTableName}, {DateTime.Now:yyyy-MM-dd_HH-mm-ss}",
                        FileName = $"Ремонти - {_equipmentTableName}, {DateTime.Now:yyyy-MM-dd_HH-mm-ss}.pdf"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        using (var stream = saveDialog.OpenFile())
                        {
                            document.Save(stream);
                        }

                        _notificationManager.Show("Таблицю успішно експортовано в PDF!", NotificationType.Success);
                    }
                }

            }
            catch (Exception e)
            {
                _notificationManager.Show("Помилка експорту в PDF!", NotificationType.Error);
                _logger.LogError(e.Message, "Error exporting PDF");
                throw;
            }
        }
        
        private void OnPrint()
        {
            _repairsDataGrid.ShowPrintPreview();
        }

        private async void LoadRepairsDataAsync()
        {
            ProgressBarVisibility = true;
            await Task.Delay(1000);
            Repairs = await _service.GetRepairItems(_repairsTableName, _equipmentTableName);
            ProgressBarVisibility = false;
        }

        private void OnRepairsDataGridLoaded(SfDataGrid repairsDataGrid)
        {
            _repairsDataGrid = repairsDataGrid;
        }
        
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _repairsTableName = navigationContext.Parameters["RepairsTableName"] as string;
            _equipmentTableName = _repairsTableName.EndsWith("Р") ? _repairsTableName.Substring(0, _repairsTableName.Length - 1).TrimEnd() : _repairsTableName;

            LoadRepairsDataAsync();
        }
        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}

