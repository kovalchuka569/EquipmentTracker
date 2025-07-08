using System.Collections.ObjectModel;
using System.ComponentModel;
using Models.EquipmentTree;

namespace UI.ViewModels.EquipmentTree;

public class FileSystemItemViewModel : IFileSystemItem, INotifyPropertyChanged
{
    public string Name { get; set; }
    public ObservableCollection<IFileSystemItem> Children { get; set; } = new();

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set { _isExpanded = value; OnPropertyChanged(nameof(IsExpanded)); }
    }

    private bool _isVisible = true;
    public bool IsVisible
    {
        get => _isVisible;
        set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public bool Filter(string filter)
    {
        bool matches = string.IsNullOrEmpty(filter) || Name.Contains(filter, StringComparison.OrdinalIgnoreCase);

        bool anyChildMatches = false;
        foreach (var child in Children.OfType<FileSystemItemViewModel>())
        {
            if (child.Filter(filter))
                anyChildMatches = true;
        }

        IsVisible = matches || anyChildMatches;
        if (IsVisible && anyChildMatches)
            IsExpanded = true;
        else if (!anyChildMatches)
            IsExpanded = false;

        OnPropertyChanged(nameof(IsVisible));
        OnPropertyChanged(nameof(IsExpanded));

        return IsVisible;
    }
}