using System.Windows;

using Microsoft.Xaml.Behaviors;

using Syncfusion.UI.Xaml.TreeView;
using Syncfusion.UI.Xaml.TreeView.Helpers;

using Presentation.ViewModels.Common.FileSystem;

namespace Presentation.Behaviors;

public class SfTreeViewEditBehavior : Behavior<SfTreeView>
{
    public static readonly DependencyProperty EditItemProperty =
        DependencyProperty.Register(nameof(EditItem), typeof(object), 
            typeof(SfTreeViewEditBehavior), new PropertyMetadata(null, OnEditItemChanged));
    
    public object EditItem
    {
        get => GetValue(EditItemProperty);
        set => SetValue(EditItemProperty, value);
    }

    private static void OnEditItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SfTreeViewEditBehavior behavior && e.NewValue != null)
        {
            
            if(e.NewValue is not FileSystemItemBaseViewModel)
                return;
            
            var treeView = behavior.AssociatedObject;
            
            if (treeView == null) 
                return;
            
            var node = treeView.GetItemInfo(e.NewValue).Node;
            if (node != null)
            {
                treeView.BeginEdit(node);
            }
        }
    }

}