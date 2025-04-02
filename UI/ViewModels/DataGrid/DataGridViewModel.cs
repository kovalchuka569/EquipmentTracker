using Core.Models.Tabs.ProductionEquipmentTree;

namespace UI.ViewModels.DataGrid;

public class DataGridViewModel : BindableBase
{
    public string FileName { get;}

    public DataGridViewModel(File file)
    {
        FileName = file?.FileName ?? "Нова таблиця";
    }
}