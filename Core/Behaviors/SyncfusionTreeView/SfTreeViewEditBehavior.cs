using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.TreeView;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Core.Behaviors.SyncfusionTreeView;

public class SfTreeViewEditBehavior: Behavior<SfTreeView>
{
    public static readonly DependencyProperty ItemBeginEditCommandProperty =
        DependencyProperty.Register("ItemBeginEditCommand", typeof(ICommand), typeof(SfTreeViewEditBehavior));
        
    public static readonly DependencyProperty ItemEndEditCommandProperty =
        DependencyProperty.Register("ItemEndEditCommand", typeof(ICommand), typeof(SfTreeViewEditBehavior));
   
    public ICommand ItemBeginEditCommand
    {
        get { return (ICommand)GetValue(ItemBeginEditCommandProperty); }
        set { SetValue(ItemBeginEditCommandProperty, value); }
    }
    
    public ICommand ItemEndEditCommand
    {
        get { return (ICommand)GetValue(ItemEndEditCommandProperty); }
        set { SetValue(ItemEndEditCommandProperty, value); }
    }
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.ItemBeginEdit += OnItemBeginEdit;
        AssociatedObject.ItemEndEdit += OnItemEndEdit;
    }
    
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.ItemBeginEdit -= OnItemBeginEdit;
        AssociatedObject.ItemEndEdit -= OnItemEndEdit;
    }
    private void OnItemBeginEdit(object sender, TreeViewItemBeginEditEventArgs e)
    {
        if (ItemBeginEditCommand != null && ItemBeginEditCommand.CanExecute(e))
        {
            ItemBeginEditCommand.Execute(e);
        }
    }
    
    private void OnItemEndEdit(object sender, TreeViewItemEndEditEventArgs e)
    {
        if (ItemEndEditCommand != null && ItemEndEditCommand.CanExecute(e))
        {
            ItemEndEditCommand.Execute(e);
        }
    }
    
    
}