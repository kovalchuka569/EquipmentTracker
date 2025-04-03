using System.Collections.ObjectModel;
using Core.Models.DataGrid;
using Core.Models.Tabs.ProductionEquipmentTree;
using Data.Entities;
using Syncfusion.DocIO.DLS;
using Syncfusion.UI.Xaml.Grid;

namespace UI.ViewModels.DataGrid;

public class DataGridViewModel : BindableBase, INavigationAware
{
    private readonly DataGridModel _model;
    private ObservableCollection<dynamic> _tableData;
    private Columns _columns;
    private string _tableName;

    public ObservableCollection<dynamic> TableData
    {
        get => _tableData;
        set => SetProperty(ref _tableData, value);
    }

    public Columns Columns
    {
        get => _columns;
        set => SetProperty(ref _columns, value);
    }
    

    public DataGridViewModel(DataGridModel model)
    {
        Console.WriteLine("DataGridViewModel constructor");
        _model = model;
        Columns = new Columns();
    }

    private async Task LoadDataAsync()
    {
        if (string.IsNullOrWhiteSpace(_tableName))
        {
            Console.WriteLine("Table name is empty");
            return;
        }
        try
        {
            var data = await _model.GetTableDataAsync();
            TableData = new ObservableCollection<dynamic>(data);

            var columns = await _model.GetColumnNamesAsync();
            Columns.Clear();

            foreach (var column in columns)
            {
                Columns.Add(new GridTextColumn
                {
                    MappingName = column.ColumnName,
                    HeaderText = column.ColumnName,
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
  

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters.ContainsKey("TableName"))
        {
            var tableName = navigationContext.Parameters["TableName"] as string;
            _model.SetTableName(tableName);
            _tableName = tableName;
            Console.WriteLine("TableName: " + tableName);
            LoadDataAsync();
        }
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;


    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
}