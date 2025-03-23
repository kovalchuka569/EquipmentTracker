using Syncfusion.UI.Xaml.TreeView;

namespace Core.Services.TreeView;

public interface ITreeViewService
{
    void Initialize(SfTreeView treeView);
    void BeginEdit(object item);
}