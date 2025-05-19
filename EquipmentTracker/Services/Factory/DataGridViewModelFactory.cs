using Core.Models.DataGrid;
using Prism.Ioc;
using UI.Interfaces.Factory;
using UI.ViewModels.DataGrid;

namespace UI.Services.Factory;

public class DataGridViewModelFactory : IDataGridViewModelFactory
{
    private readonly IContainerProvider _containerProvider;
    private readonly Dictionary<string, DataGridViewModel> _viewModels = new Dictionary<string, DataGridViewModel>();

    public DataGridViewModelFactory(IContainerProvider containerProvider)
    {
        _containerProvider = containerProvider;
    }

    public DataGridViewModel Create(string tableName)
    {
        if (_viewModels.TryGetValue(tableName, out var existingViewModel))
        {
            return existingViewModel;
        }
        var newViewModel = _containerProvider.Resolve<DataGridViewModel>();
        _viewModels[tableName] = newViewModel;
        return newViewModel;
    }
}