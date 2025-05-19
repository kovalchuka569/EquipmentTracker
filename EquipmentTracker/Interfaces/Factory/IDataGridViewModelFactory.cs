using UI.ViewModels.DataGrid;

namespace UI.Interfaces.Factory;

public interface IDataGridViewModelFactory
{
    DataGridViewModel Create(string tableName);
}