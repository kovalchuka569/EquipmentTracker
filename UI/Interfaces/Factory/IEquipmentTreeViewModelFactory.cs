using UI.ViewModels.EquipmentTree;
using UI.ViewModels.Tabs;

namespace UI.Interfaces.Factory;

public interface IEquipmentTreeViewModelFactory
{
    EquipmentTreeViewModel Create(string menuType);
}