using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Dynamic;
using System.Windows.Input;
using Core.Models.DataGrid;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using Syncfusion.UI.Xaml.Grid;
using System.Data;
using Npgsql;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Prism.Common;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Windows.Controls.Cells;
using Syncfusion.Windows.Controls.Grid;
using RowColumnIndex = Syncfusion.UI.Xaml.Grid.ScrollAxis.RowColumnIndex;

namespace UI.ViewModels.DataGrid;

public class DataGridViewModel : BindableBase, INavigationAware
{
    private readonly DataGridModel _model;
    private string _currentTableName;
    private ObservableCollection<ExpandoObject> _data;
    private ExpandoObject _selectedItem;
    private string _guid = Guid.NewGuid().ToString();
    private SfDataGrid _sfDataGrid;
    
    private DelegateCommand<object> _autoGeneratingColumnCommand;
    private DelegateCommand<CurrentCellBeginEditEventArgs> _cellBeginEditCommand;
    private AsyncDelegateCommand<CurrentCellEndEditEventArgs> _currentCellEndEditCommand;
    private DelegateCommand<RowValidatedEventArgs> _rowValidatedCommand;
    private DelegateCommand<SfDataGrid> _sfDataGridLoadedCommand;
    private DelegateCommand _deleteRecordCommand;


    

    /// <summary>
    /// Gets or sets the data items displayed in the DataGrid
    /// </summary>
    public ObservableCollection<ExpandoObject> Items
    {
        get => _data;
        set => SetProperty(ref _data, value);
    }

    public ExpandoObject SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }
    
    
    /// <summary>
    /// Command that handles the AutoGeneratingColumn event
    /// </summary>
    public DelegateCommand<object> AutoGeneratingColumnCommand =>
        _autoGeneratingColumnCommand ??= new Prism.Commands.DelegateCommand<object>(HandleAutoGeneratingColumn);
    public DelegateCommand<CurrentCellBeginEditEventArgs> CellBeginEditCommand =>
        _cellBeginEditCommand ??= new DelegateCommand<CurrentCellBeginEditEventArgs>(HandleCellBeginEdit);

    public AsyncDelegateCommand<CurrentCellEndEditEventArgs> CurrentCellEndEditCommand =>
        _currentCellEndEditCommand ??= new AsyncDelegateCommand<CurrentCellEndEditEventArgs>(HandleCurrentCellEndEdit);
    public DelegateCommand<RowValidatedEventArgs> RowValidatedCommand =>
    _rowValidatedCommand??= new DelegateCommand<RowValidatedEventArgs>(HandleRowValidated1);

    public DelegateCommand<SfDataGrid> SfDataGridLoadedCommand =>
        _sfDataGridLoadedCommand ??= new DelegateCommand<SfDataGrid>(OnDataGridLoaded);
    public DelegateCommand DeleteRecordCommand =>
        _deleteRecordCommand ??= new DelegateCommand(OnDeleteRecord);
    


    /// <summary>
    /// Initializes a new instance of DataGridViewModel
    /// </summary>
    /// <param name="model">The DataGridModel to use for data operations</param>
    public DataGridViewModel(DataGridModel model)
    {
        _model = model;
        _data = new ObservableCollection<ExpandoObject>();
        Console.WriteLine($"Создан новый ViewModel: {_guid}");
    }
    

    private async void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // Add new record in Data Base
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (var item in e.NewItems)
            {
                var record = item as IDictionary<string, object>;
                if (record != null)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in record)
                    {
                       dict[prop.Key] = prop.Value; 
                    }
                    var insertedId = await _model.InsertRecordAsync(_currentTableName, dict);
                    if (insertedId != null)
                    {
                        record["id"] = insertedId;
                        Console.WriteLine("Added record, new ID: " + insertedId);
                    }
                }
            }
        }
        
        // Delete record from Data Base
        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (var item in e.OldItems)
            {
                var record = item as IDictionary<string, object>;
                if (record != null && record.ContainsKey("id") && record["id"] != null)
                {
                    var id = record["id"];
                    await _model.DeleteRecordAsync(_currentTableName, id);
                    Console.WriteLine($"Removed record: {id}");
                }
            }
        }
    }

    private void OnDeleteRecord()
    {
        if (SelectedItem != null)
        {
            Items.Remove(SelectedItem);
        }
    }
    
    private void HandleCellBeginEdit(CurrentCellBeginEditEventArgs args)
    {
        Console.WriteLine("Начинаем редактирование или добавление " + _guid);
    }
    private async Task HandleCurrentCellEndEdit(CurrentCellEndEditEventArgs args)
    {
        var rowIndex = args.RowColumnIndex.RowIndex;
        var columnIndex = args.RowColumnIndex.ColumnIndex;
        var mappingName = _sfDataGrid.Columns[columnIndex].MappingName;
        var rowData = _sfDataGrid.GetRecordAtRowIndex(rowIndex) as IDictionary<string, object>;

        if (rowData == null || !rowData.ContainsKey("id"))
        {
            Console.WriteLine("Введено пустое значение или не сущесвует id");
            return;
        }
        var id = rowData["id"];
        var newValue = rowData[mappingName];
        
        Console.WriteLine($"Изменено поле: {mappingName}, новое значение: {newValue}, id: {id}");
        Console.WriteLine("Заканчиваем редактирование " + _guid);

        var updateDic = new Dictionary<string, object>
        {
            { mappingName, newValue }
        };
        await _model.UpdateRecordAsync(_currentTableName, id, updateDic);
    }

    private void HandleRowValidated1(RowValidatedEventArgs args)
    {
        Console.WriteLine("Строка свалидировалась из " + _guid);
    }

    private void OnDataGridLoaded(SfDataGrid sfDataGrid)
    {
        _sfDataGrid = sfDataGrid;
    }
    

    private void HandleAddNewRowInitiating(AddNewRowInitiatingEventArgs e)
    {
        Console.WriteLine("HandleAddNewRowInitiating");
    }
    
    /// <summary>
    /// Called when navigated to this view, loads data from the specified table
    /// </summary>
    /// <param name="navigationContext">The navigation context</param>
    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        string tableName = navigationContext.Parameters["parameter"] as string;
        _currentTableName = tableName;
        
        Console.WriteLine($"OnNavigatedTo для ViewModel {GetHashCode()} с таблицей {_currentTableName}");
        
        if (!string.IsNullOrEmpty(_currentTableName))
        {
            await LoadData();
        }
    }
    
    /// <summary>
    /// Loads data from the database for the current table
    /// </summary>
    private async Task LoadData()
    {
        // Unsubscribe from collection event during loading
        Items.CollectionChanged -= Items_CollectionChanged;
        
        Items.Clear();
        var data = await _model.GetDataAsync(_currentTableName);
        foreach (var item in data)
        {
            Items.Add(item); 
        }
        
        // Subscribe to collection events after loading data
        Items.CollectionChanged += Items_CollectionChanged;
    }
    
    /// <summary>
    /// Handles the AutoGeneratingColumn event to customize columns
    /// </summary>
    /// <param name="param">Event arguments</param>
    private void HandleAutoGeneratingColumn(object param)
    {
        // Get the column from event args using reflection
        var columnProperty = param?.GetType().GetProperty("Column");
        if (columnProperty != null)
        {
            var column = columnProperty.GetValue(param) as GridColumn;
            if (column != null)
            {
                // Customize column properties
                column.AllowEditing = true;
                column.AllowFiltering = true;
                column.AllowSorting = true;
                column.AllowFocus = true;
                
                // Hide Id column
                if (column.MappingName.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    // Set visibility to hide the column
                    column.IsHidden = true;
                    column.AllowEditing = false;
                }
                
                // Set appropriate header text (can be customized as needed)
                column.HeaderText = column.MappingName;
            }
        }
    }

    /// <summary>
    /// Determines if this instance is the navigation target
    /// </summary>
    /// <param name="navigationContext">The navigation context</param>
    /// <returns>True if this is the navigation target</returns>
    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return false;
    }
    
    /// <summary>
    /// Called when navigated from this view
    /// </summary>
    /// <param name="navigationContext">The navigation context</param>
    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
}
