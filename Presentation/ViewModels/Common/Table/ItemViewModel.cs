using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Presentation.ViewModels.Common.Table;

public class ItemViewModel : DynamicObject, INotifyPropertyChanged
{
    
    public RowViewModel RowViewModel{ get;}

    public ItemViewModel(RowViewModel rowViewModel)
    {
        RowViewModel = rowViewModel;

        foreach (var cell in rowViewModel.CellsByMappingName)
        {
            Properties[cell.Key] = cell.Value.Value;
        }
    }

    public ItemViewModel() : this(new RowViewModel()) { }
    
    // Indexator
    public object? this[string mappingName] 
    { 
        get 
        { 
            if (RowViewModel.TryGetCellByMappingName(mappingName, out var cell)) 
                return cell?.Value; 
            return null; 
        } 
        set 
        { 
            Properties[mappingName] = value; 
            RowViewModel.TrySetCellValueByMappingName(mappingName, value); 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(mappingName)); 
        } 
    }
    
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        return Properties.TryGetValue(binder.Name, out result);
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        Properties[binder.Name] = value;
        RowViewModel.TrySetCellValueByMappingName(binder.Name, value);
        OnPropertyChanged(binder.Name);
        return true;
    }
    
    public override IEnumerable<string> GetDynamicMemberNames()
    {
        return Properties.Keys;
    }
    
    private Dictionary<string, object?> Properties { get; } = new();
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}