namespace UI.ViewModels.Tabs;

public interface IEquipmentTreeViewModelFactory
{
    EquipmentTreeViewModel Create(string menuType);
}