using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Core.Models.Tabs.ProductionEquipmentTree;
using Core.Services.TreeView;
using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.UI.Xaml.TreeView;

namespace UI.Views.Tabs.ProductionEquipmentTree;

public partial class EquipmentTreeView : UserControl
{
    private readonly ITreeViewService _treeViewService;
    public EquipmentTreeView(ITreeViewService treeViewService)
    {
        InitializeComponent();
        _treeViewService = treeViewService;
        _treeViewService.Initialize(treeView);
    }

    private void TreeView_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is UI.ViewModels.Tabs.EquipmentTreeViewModel equipmentTreeViewModel)
        {
            equipmentTreeViewModel.TreeView = (SfTreeView)sender;
        }
    }
}