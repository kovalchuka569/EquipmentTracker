using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;
using Presentation.Models;

namespace Presentation.Behaviors;

public class ListViewDragDropBehavior : Behavior<ListView>
{
    private Point _listItemDragStartPoint;
    private object? _draggedItem;
    
    public static readonly DependencyProperty DropCommandProperty = DependencyProperty.Register(nameof(DropCommand), typeof(ICommand), typeof(ListViewDragDropBehavior), new PropertyMetadata(null));
    
    public ICommand? DropCommand
    {
        get => (ICommand)GetValue(DropCommandProperty);
        set => SetValue(DropCommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
        AssociatedObject.MouseMove += AssociatedObject_MouseMove;
        AssociatedObject.Drop += AssociatedObject_Drop;
        AssociatedObject.AllowDrop = true;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
        AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
        AssociatedObject.Drop -= AssociatedObject_Drop;
    }
    
    private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _listItemDragStartPoint = e.GetPosition(null);
        _draggedItem = null;

        if (e.OriginalSource is not DependencyObject depObj)
            return;

        var listViewItem = FindAncestor<ListViewItem>(depObj);
        if (listViewItem != null)
        {
            _draggedItem = listViewItem.DataContext;
        }
    }
    
    private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _draggedItem == null)
            return;

        var currentPosition = e.GetPosition(null);
        var diff = currentPosition - _listItemDragStartPoint;

        if (!(Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance) &&
            !(Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            return;

        DataObject data = new DataObject(typeof(object), _draggedItem);
        DragDrop.DoDragDrop(AssociatedObject, data, DragDropEffects.Move);
        _draggedItem = null;
    }
    
    private void AssociatedObject_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(object)) || DropCommand == null)
            return;

        var droppedItem = e.Data.GetData(typeof(object));
        var targetItem = GetItemFromPoint(e.GetPosition(AssociatedObject));

        if (droppedItem == null) return;
        
        var commandArgs = new DragDropInfo(droppedItem, targetItem);

        if (DropCommand.CanExecute(commandArgs))
        {
            DropCommand.Execute(commandArgs);
        }
    }
    
    private object? GetItemFromPoint(Point point)
    {
        var hit = VisualTreeHelper.HitTest(AssociatedObject, point);
        var element = hit?.VisualHit;
        while (element != null && !(element is ListViewItem))
        {
            element = VisualTreeHelper.GetParent(element);
        }
        return (element as ListViewItem)?.DataContext;
    }
    
    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current != null && current is not T)
            current = VisualTreeHelper.GetParent(current);
        return current as T;
    }
}