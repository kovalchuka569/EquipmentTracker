using Core.Models.DataGrid;
using UI.Interfaces.Factory;
using UI.ViewModels.DataGrid;

namespace UI.Services.Factory;

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
        return vm;
    }
}