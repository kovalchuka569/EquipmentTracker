using Syncfusion.UI.Xaml.TreeView;

namespace Core.Services.TreeView;

public class TreeViewService : ITreeViewService
{
    private SfTreeView _treeView;

    public void Initialize(SfTreeView treeView)
    {
        _treeView = treeView;
    }

    public void BeginEdit(object item)
    {
        if (_treeView != null && item != null)
        {
            var treeNode = _treeView.Nodes.FirstOrDefault(n => n.Content == item);
            if (treeNode != null)
            {
                _treeView.BeginEdit(treeNode);
            }
        }
    }
}