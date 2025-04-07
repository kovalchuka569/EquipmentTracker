namespace UI.ViewModels.DataGrid;

public interface IDataGridViewModelFactory
{
    DataGridViewModel Create(string tableName);
}