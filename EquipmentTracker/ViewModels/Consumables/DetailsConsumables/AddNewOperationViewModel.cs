using System.IO;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;
using System.Windows.Documents;
using System.Windows.Media;
using Core.Events.DataGrid.Consumables;
using Microsoft.Win32;
using Prism.Events;
using Syncfusion.UI.Xaml.ImageEditor;
using Syncfusion.Windows.Tools.Controls;

namespace UI.ViewModels.Consumables.DetailsConsumables
{
    public class AddNewOperationViewModel : BindableBase, INavigationAware
    {
        #region Fields
        private static IEventAggregator _eventAggregator;
        private string _descriptionText;
        private ComboBoxItemAdv _selectedOperation;
        private string _quantityValue;
        private decimal _quantityValueDecimal;
        private string _receiptFileName = "файл квитанції";
        private bool _unpinButtonVisibility = false;
        private byte[] _receiptImageBytes;

        private string _quantityErrorText;
        private bool _quantityErrorVisibility;
        #endregion

        #region Properties
        public string DescriptionText
        {
            get => _descriptionText;
            set => SetProperty(ref _descriptionText, value);
        }
        public ComboBoxItemAdv SelectedOperation
        {
            get => _selectedOperation;
            set => SetProperty(ref _selectedOperation, value);
        }

        public string QuantityValue
        {
            get => _quantityValue;
            set => SetProperty(ref _quantityValue, value);
        }

        public decimal QuantityValueDecimal
        {
            get => _quantityValueDecimal;
            set => SetProperty(ref _quantityValueDecimal, value);
        }
        
        public string QuantityErrorText
        {
            get => _quantityErrorText;
            set => SetProperty(ref _quantityErrorText, value);
        }

        public bool QuantityErrorVisibility
        {
            get => _quantityErrorVisibility;
            set => SetProperty(ref _quantityErrorVisibility, value);
        }

        public string ReceiptFileName
        {
            get => _receiptFileName;
            set => SetProperty(ref _receiptFileName, value);
        }

        public bool UnpinButtonVisibility
        {
            get => _unpinButtonVisibility;
            set => SetProperty(ref _unpinButtonVisibility, value);
        }
        #endregion
        
        #region Commands
        public DelegateCommand CloseAddNewOperationCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand QuantityValueChangedCommand { get; set; }
        public DelegateCommand ShowFileDialogCommand { get; set; }
        public DelegateCommand UnpinFileCommand { get; set; }
        #endregion

        #region Constructor
        public AddNewOperationViewModel(IEventAggregator? eventAggregator)
        {
            _eventAggregator = eventAggregator;
            CloseAddNewOperationCommand = new DelegateCommand(OnCloseAddNew);
            SaveCommand = new DelegateCommand(OnSave);
            QuantityValueChangedCommand = new DelegateCommand(OnQuantityValueChanged);
            ShowFileDialogCommand = new DelegateCommand(OnShowFileDialog);
            UnpinFileCommand = new DelegateCommand(OnUnpinFile);

            QuantityValue = "0,00";
        }
        #endregion

        private void OnShowFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Зображення (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Всі файли (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName;
                string selectedFileName = Path.GetFileName(path);
                string extension = Path.GetExtension(selectedFileName);
                
                _receiptImageBytes = File.ReadAllBytes(path);
                
                if (selectedFileName.Length > 22)
                {
                    ReceiptFileName = selectedFileName.Substring(0, 22) + ".." + extension;
                }
                else
                {
                    ReceiptFileName = selectedFileName;
                }
                UnpinButtonVisibility = true;
            }
        }

        private void OnUnpinFile()
        {
            _receiptImageBytes = null;
            ReceiptFileName = "файл квитанції";
            UnpinButtonVisibility = false;
        }
        
        // Correct value checker
        private void OnQuantityValueChanged()
        {
            if (!decimal.TryParse(QuantityValue, out var result))
            {
                QuantityErrorText = "Введіть числове значення";
                QuantityErrorVisibility = true;
                return;
            }
            if (result < 0)
            {
                QuantityErrorVisibility = true;
                QuantityErrorText = "Кількість не може бути від'ємною!";
                return;
            }
            if (result > 99999999.99m)
            {
                QuantityErrorVisibility = true;
                QuantityErrorText = "Перевищено максимальне значення!";
                return;
            }
            if (decimal.Round(result, 2) != result)
            {
                QuantityErrorVisibility = true;
                QuantityErrorText = "Допускається 2 знаки після коми!";
                return;
            }
            else
            {
                QuantityErrorVisibility = false;
                QuantityValueDecimal = result;
            }
        }
        
        // Close this template (Remove view from region)
        private void OnCloseAddNew() => _eventAggregator.GetEvent<CloseAddNewTemplateEvent>().Publish();

        // Save (Publish event with args from fields)
        private void OnSave()
        {
            if(QuantityErrorVisibility || SelectedOperation == null) return;
            _eventAggregator.GetEvent<CloseAddNewTemplateEvent>().Publish();
            _eventAggregator.GetEvent<AddNewOperationEvent>().Publish(new AddNewOperationEventArgs
            {
                OperationType = SelectedOperation.Content.ToString(),
                Quantity = QuantityValue,
                Description = DescriptionText,
                User = 1,
                ReceiptImageBytes = _receiptImageBytes
            });
        }
        
        #region Navigation
        public void OnNavigatedTo(NavigationContext navigationContext) {}
        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        public void OnNavigatedFrom(NavigationContext navigationContext) {}
        #endregion
    }
}
