using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;

using Syncfusion.XlsIO;

using Models.Dialogs;
using Core.Interfaces;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;


namespace Presentation.ViewModels;

public class ExcelImportConfiguratorViewModel : BindableBase, IDialogAware, IClosableDialog, IDataErrorInfo
{
    #region UI properties
    
    private string _filePath = "(не вибрано)";
    public string FilePath
    {
        get => _filePath;
        set => SetProperty(ref _filePath, value);
    }

    private ObservableCollection<string> _sheets = new();
    public ObservableCollection<string> Sheets
    {
        get => _sheets;
        set => SetProperty(ref _sheets, value);
    }

    private string _selectedSheet = string.Empty;
    public string SelectedSheet
    {
        get => _selectedSheet;
        set
        {
            if (SetProperty(ref _selectedSheet, value))
            {
                RaisePropertyChanged(nameof(ImportButtonIsEnabled));
            }
        }
    }

    private bool _filePathBusyIndicatorVisibility;
    public bool FilePathBusyIndicatorVisibility
    {
        get => _filePathBusyIndicatorVisibility;
        set
        {
            if (SetProperty(ref _filePathBusyIndicatorVisibility, value))
            {
                RaisePropertyChanged(nameof(ImportButtonIsEnabled));
            }
        }
    }
    
    private string _headerRangeString = "A1:B1";
    public string HeaderRangeString
    {
        get => _headerRangeString;
        set => SetProperty(ref _headerRangeString, value);
    }

    private int? _rowRangeStart = 1;
    public int? RowRangeStart
    {
        get => _rowRangeStart;
        set => SetProperty(ref _rowRangeStart, value);
    }
    
    private int? _rowRangeEnd = 10;
    public int? RowRangeEnd
    {
        get => _rowRangeEnd;
        set => SetProperty(ref _rowRangeEnd, value);
    }
    
    public string Error => string.Empty;
    
    private readonly Dictionary<string, string> _errors = new();

    public string this[string columnName]
    {
        get
        {
            var error = OnValidate(columnName);
            
            if (string.IsNullOrEmpty(error))
                _errors.Remove(columnName);
            
            else
                _errors[columnName] = error;
            
            RaisePropertyChanged(nameof(ImportButtonIsEnabled));
            RaisePropertyChanged(nameof(Error));

            return error;
        }
    }
    
    public bool ImportButtonIsEnabled => !FilePathBusyIndicatorVisibility && 
                                         _errors.Count == 0 && 
                                         !string.IsNullOrEmpty(SelectedSheet);
    
    public bool NoSheetsTipVisibility => !Sheets.Any();
    
    #endregion
    
    #region Constructor
    public ExcelImportConfiguratorViewModel()
    {
        InitializeCommands();
    }
    #endregion

    #region Commands management
    
    public DelegateCommand? CancelDialogCommand { get; private set; }
    
    public DelegateCommand<KeyEventArgs>? UserControlKeyDownCommand { get; private set; }
    
    public DelegateCommand? BrowseFileCommand { get; private set; }
    
    public DelegateCommand? ImportCommand { get; private set; }
    
    private void InitializeCommands()
    {
        CancelDialogCommand = new DelegateCommand(OnCancelDialog);
        
        UserControlKeyDownCommand = new DelegateCommand<KeyEventArgs>(OnUserControlKeyDown);
        
        BrowseFileCommand = new DelegateCommand(OnBrowseFile);

        ImportCommand = new DelegateCommand(OnImport);
    }
    
    #endregion

    #region Commands implementation

    private void OnImport()
    {
        var result = new DialogResult
        {
            Result = ButtonResult.OK,
            Parameters = new DialogParameters
            {
                {
                    "ExcelImportConfigurationResult", new ExcelImportConfigurationResult
                    {
                        SelectedFilePath = FilePath,
                        SelectedSheetName = SelectedSheet,
                        HeadersRange = HeaderRangeString,
                        RowRangeStart = RowRangeStart!.Value,
                        RowRangeEnd = RowRangeEnd!.Value
                    }
                }
            }
        };
        
        _closeDialogFromHostCommand?.Execute(result);
    }

    private async void OnBrowseFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Вибір Excel-файлу",
            Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls",
            DefaultExt = ".xlsx",
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog() != true) return;
        
        FilePath = dialog.FileName;

        FilePathBusyIndicatorVisibility = true;
        try
        {
            await GetExcelSheetsAsync(FilePath);
        }
        finally
        {
            FilePathBusyIndicatorVisibility = false;
        }
    }

    private void OnUserControlKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            OnCancelDialog();
        }
    }
    
    private void OnCancelDialog()
    {
        var result = new DialogResult
        {
            Result = ButtonResult.Cancel
        };
        _closeDialogFromHostCommand?.Execute(result);
    }
    #endregion

    #region Private methods
    
    private async Task GetExcelSheetsAsync(string path)
    {
        Sheets.Clear();
        RaisePropertyChanged(nameof(NoSheetsTipVisibility));
        
        var sheetNames = await Task.Run(() =>
        {
            using var excelEngine = new ExcelEngine();
            var application = excelEngine.Excel;
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var ms = new MemoryStream();
            fs.CopyTo(ms);
            ms.Position = 0;

            var workbook = application.Workbooks.Open(ms);
            try
            {
                return workbook.Worksheets.Select(ws => ws.Name).ToList();
            }
            finally
            {
                workbook.Close();
            }
        });

        foreach (var sheet in sheetNames)
            Sheets.Add(sheet);

        SelectedSheet = Sheets.First();

        RaisePropertyChanged(nameof(NoSheetsTipVisibility));
    }
    
     private string OnValidate(string columnName)
    {
        var result = string.Empty;
        if (columnName == "HeaderRangeString")
        {
            if (string.IsNullOrEmpty(HeaderRangeString))
            {
                result = "Діапазон заголовків не може бути пустим";
            }
            else
            {
                var parts = HeaderRangeString.Split(":");
                if (parts.Length != 2)
                {
                    result = "Діапазон повинен містити дві комірки через двокрапку, наприклад: A1:G1";
                }
                else
                {
                    var start = parts[0].Trim();
                    var end = parts[1].Trim();
                    
                    var cellRegex = new Regex(@"^([A-Z]+)(\d+)$", RegexOptions.IgnoreCase);
                    var matchStart = cellRegex.Match(start);
                    var matchEnd = cellRegex.Match(end);
                    
                    if (!matchStart.Success || !matchEnd.Success)
                    {
                        result = "Неправильний формат комірок, приклад: A1:G1";
                    }
                    
                    else
                    {
                        var startCol = matchStart.Groups[1].Value.ToUpper();
                        var startRow = int.Parse(matchStart.Groups[2].Value);
                        var endCol = matchEnd.Groups[1].Value.ToUpper();
                        var endRow = int.Parse(matchEnd.Groups[2].Value);

                        if (startRow != endRow)
                        {
                            result = "Обидві комірки повинні бути в одному рядку";
                        }
                        else if (ColumnToIndex(startCol) > ColumnToIndex(endCol))
                        {
                            result = "Початкова колонка повинна бути лівіше або дорівнювати кінцевій";
                        }
                    }
                    
                }
            }
        }

        if (columnName is "RowRangeStart")
        {
            if (RowRangeStart > RowRangeEnd)
            {
                result = "Початковий рядок не може бути більшим за кінцевий";
            }

            if (RowRangeStart is null)
            {
                result = "Початковий рядок не може бути пустим";
            }
        }
        
        if (columnName is "RowRangeEnd")
        {
            if (RowRangeEnd < RowRangeStart)
            {
                result = "Кінцевий рядок не може бути меншим за початковий";
            }
            
            if (RowRangeStart is null)
            {
                result = "Кінцевий рядок не може бути пустим";
            }
        }
        return result;
    }
    
    private int ColumnToIndex(string column)
    {
        return column.Aggregate(0, (current, c) => current * 26 + c - 'A' + 1);
    }

    #endregion

    #region Interface implementations

    private DelegateCommand<IDialogResult>? _closeDialogFromHostCommand;

    public bool CanCloseDialog() => true;

    public void OnDialogClosed() { }

    public void OnDialogOpened(IDialogParameters parameters) { }

    public DialogCloseListener RequestClose { get; } = new();
    
    public void SetCloseCommand(DelegateCommand<IDialogResult>? closeCommand)
    {
        _closeDialogFromHostCommand = closeCommand;
    }

    #endregion
}