namespace Presentation.Models;

public class DragDropInfo(object droppedItem, object? targetItem)
{
    public object DroppedItem { get; } = droppedItem;
    public object? TargetItem { get; } = targetItem;
}