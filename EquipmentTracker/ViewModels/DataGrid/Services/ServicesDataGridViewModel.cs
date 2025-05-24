using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;
using Common.Logging;
using Core.Events.TabControl;
using Core.Services.ServicesDataGrid;
using Models.RepairsDataGrid.ServicesDataGrid;
using Notification.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Syncfusion.Pdf;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.XlsIO;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace UI.ViewModels.DataGrid.Services
{
    public class ServicesDataGridViewModel : BindableBase, INavigationAware
    {
        private readonly IAppLogger<ServicesDataGridViewModel> _logger;
        private readonly IEventAggregator _globalEventAggregator;
        private readonly NotificationManager _globalNotificationManager;
        private readonly IServicesDataGridService _service;
        
        private SfDataGrid _servicesDataGrid;
        
        private ObservableCollection<ServiceItem> _services = new();
        private ServiceItem _selectedService;

        private string _equipmentTableName;
        private string _servicesTableName;
        
        private bool _progressBarVisibility;
        private bool _editServiceContextMenuVisibility;

        public ObservableCollection<ServiceItem> Services
        {
            get => _services;
            set => SetProperty(ref _services, value);
        }
        
        public ServiceItem SelectedService
        {
            get => _selectedService;
            set => SetProperty(ref _selectedService, value);
        }
        
        public bool ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => SetProperty(ref _progressBarVisibility, value);
        }
        
        public bool EditServiceContextMenuVisibility
        {
            get => _editServiceContextMenuVisibility;
            set => SetProperty(ref _editServiceContextMenuVisibility, value);
        }
        
        public DelegateCommand<SfDataGrid> ServicesDataGridLoadedCommand { get; set; }
        public DelegateCommand ContextMenuLoadedCommand { get; set; }
        public DelegateCommand CreateNewServiceCommand { get; set; }
        public DelegateCommand EditServiceCommand { get; set; }
        public DelegateCommand RefreshCommand { get; set; }
        public DelegateCommand PrintCommand {get; set; }
        public DelegateCommand ExcelExportCommand { get; set; }
        public DelegateCommand PdfExportCommand { get; set; }
        
        public ServicesDataGridViewModel(
            IEventAggregator globalEventAggregator,
            IServicesDataGridService service,
            IAppLogger<ServicesDataGridViewModel> logger,
            NotificationManager globalNotificationManager)
        {
            _globalEventAggregator = globalEventAggregator;
            _service = service;
            _logger = logger;
            _globalNotificationManager = globalNotificationManager;

            ServicesDataGridLoadedCommand = new DelegateCommand<SfDataGrid>(OnServicesDataGridLoaded);
            ContextMenuLoadedCommand = new DelegateCommand(OnContextMenuLoaded);
            CreateNewServiceCommand = new DelegateCommand(OnCreateService);
            EditServiceCommand = new DelegateCommand(OnEditService);
            RefreshCommand = new DelegateCommand(LoadServicesDataAsync);
            PrintCommand = new DelegateCommand(OnPrint);
            ExcelExportCommand = new DelegateCommand(OnExcelExport);
            PdfExportCommand = new DelegateCommand(OnPdfExport);
        }

        private void OnCreateService()
        {
            _globalEventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
            {
                Header = _equipmentTableName + " створення обслуговування",
                Parameters = new Dictionary<string, object>
                {
                    {"ViewNameToShow", "AddServiceView"},
                    {"AddServiceView.EquipmentTableName", _equipmentTableName},
                }
            });
        }
        
        private void OnEditService()
        {
            if (SelectedService is ServiceItem service)
            {
                _globalEventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
                {
                    Header = $"{service.EquipmentInventoryNumber} | {service.EquipmentBrand} | {service.EquipmentModel} - редагування обслуговування",
                    Parameters = new Dictionary<string, object>
                    {
                        {"ViewNameToShow", "AddServiceView"},
                        {"AddServiceView.ServiceItems", service},
                        {"AddServiceView.EquipmentTableName", _equipmentTableName}
                    }
                });
            }
        }
        
        private void OnContextMenuLoaded()
        {
            if (SelectedService is ServiceItem)
            {
                EditServiceContextMenuVisibility = true;
            }
        }
        
        private void OnPrint()
        {
            _servicesDataGrid.ShowPrintPreview();
        }
        
        private void OnExcelExport()
        {
            try
            {
                var options = new ExcelExportingOptions()
                {
                    ExcelVersion = ExcelVersion.Excel2016
                };
            
                var excelEngine = _servicesDataGrid.ExportToExcel(_servicesDataGrid.View, options);
                var workbook = excelEngine.Excel.Workbooks[0];

                var saveDialog = new SaveFileDialog()
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    Title = $"Експорт в Excel: Обслуговування - {_equipmentTableName}, {DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}",
                    FileName = $"Обслуговування - {_equipmentTableName}, {DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    using (var stream = saveDialog.OpenFile())
                    {
                        workbook.SaveAs(stream);
                    }
                    _globalNotificationManager.Show("Таблицю успішно експортовано до Excel!", NotificationType.Success);
                }
            }
            catch (Exception e)
            {
                _globalNotificationManager.Show("Помилка експорту до Excel!", NotificationType.Error);
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
                var pdfGrid = _servicesDataGrid.ExportToPdfGrid(_servicesDataGrid.View, options);

                using (PdfDocument document = new PdfDocument())
                {
                    var page = document.Pages.Add();
                    pdfGrid.Draw(page, new PointF(0, 0));

                    var saveDialog = new SaveFileDialog()
                    {
                        Filter = "PDF files (*.pdf)|*.pdf",
                        Title = $"Експорт в PDF: Обслуговування - {_equipmentTableName}, {DateTime.Now:yyyy-MM-dd_HH-mm-ss}",
                        FileName = $"Обслуговування - {_equipmentTableName}, {DateTime.Now:yyyy-MM-dd_HH-mm-ss}.pdf"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        using (var stream = saveDialog.OpenFile())
                        {
                            document.Save(stream);
                        }

                        _globalNotificationManager.Show("Таблицю успішно експортовано в PDF!", NotificationType.Success);
                    }
                }

            }
            catch (Exception e)
            {
                _globalNotificationManager.Show("Помилка експорту в PDF!", NotificationType.Error);
                _logger.LogError(e.Message, "Error exporting PDF");
                throw;
            }
        }
        
        private async void LoadServicesDataAsync()
        {
            ProgressBarVisibility = true;
            await Task.Delay(1000);
            Services = await _service.GetServiceItems(_servicesTableName, _equipmentTableName);
            ProgressBarVisibility = false;
        }
        
        private void OnServicesDataGridLoaded(SfDataGrid servicesDataGrid)
        {
            _servicesDataGrid = servicesDataGrid;
        }
        
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _servicesTableName = navigationContext.Parameters["ServicesTableName"] as string;
            _equipmentTableName = _servicesTableName.EndsWith("О") ? _servicesTableName.Substring(0, _servicesTableName.Length - 1).TrimEnd() : _servicesTableName;
            
            LoadServicesDataAsync();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
