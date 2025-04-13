using Core.Models.DataGrid;
using UI.Interfaces.Factory;
using UI.ViewModels.DataGrid;

namespace UI.Services.Factory;

public class DataGridViewModelModelFactory : IDataGridViewModelFactory
{
    private readonly IContainerProvider _containerProvider;
    private readonly Dictionary<string, DataGridViewModel> _viewModels = new Dictionary<string, DataGridViewModel>();

    public DataGridViewModelModelFactory(IContainerProvider containerProvider)
    {
        _containerProvider = containerProvider;
        Console.WriteLine($"Создан DataGridViewModelFactory: {GetHashCode()}");
    }

    public DataGridViewModel Create(string tableName)
    {
        Console.WriteLine("Создание нового DataGridViewModel");
        if (_viewModels.TryGetValue(tableName, out var existingViewModel))
        {
            return existingViewModel;
        }
        var newViewModel = _containerProvider.Resolve<DataGridViewModel>();
        _viewModels[tableName] = newViewModel;
        return newViewModel;
    }
}