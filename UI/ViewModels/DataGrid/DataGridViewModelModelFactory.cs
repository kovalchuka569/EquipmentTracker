using Core.Models.DataGrid;

namespace UI.ViewModels.DataGrid;

public class DataGridViewModelModelFactory : IDataGridViewModelFactory
{
    private readonly DataGridModel _model;

    public DataGridViewModelModelFactory(DataGridModel model)
    {
        _model = model;
    }

    public DataGridViewModel Create(string tableName)
    {
        var vm = new DataGridViewModel(_model);
        vm.TableName = tableName;
        return vm;
    }
}