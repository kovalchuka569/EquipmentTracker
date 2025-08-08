using System.Collections;
using System.ComponentModel;
using System.Dynamic;
using EquipmentTracker.ViewModels.Common.Table;

namespace EquipmentTracker.ViewModels.Common;

public class NotifyingExpandoObject : DynamicObject, INotifyPropertyChanged, IDictionary<string, object?>
{
    private readonly Dictionary<string, object?> _properties = new();
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        return _properties.TryGetValue(binder.Name, out result);
    }
    
    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        _properties[binder.Name] = value;
        OnPropertyChanged(binder.Name);
        return true;
    }
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    public object? this[string key]
    {
        get => _properties.GetValueOrDefault(key);
        set
        {
            _properties[key] = value;
            OnPropertyChanged(key);
        }
    }
    
    public ICollection<string> Keys => _properties.Keys;
    public ICollection<object?> Values => _properties.Values;
    public int Count => _properties.Count;
    public bool IsReadOnly => false;
    
    public void Add(string key, object? value)
    {
        _properties.Add(key, value);
        OnPropertyChanged(key);
    }

    public void Add(KeyValuePair<string, object?> item) => Add(item.Key, item.Value);

    public void Clear()
    {
        var keys = _properties.Keys.ToList();
        _properties.Clear();
        foreach (var key in keys)
        {
            OnPropertyChanged(key);
        }
    }
    
    public bool Contains(KeyValuePair<string, object?> item) => _properties.Contains(item);
    public bool ContainsKey(string key) => _properties.ContainsKey(key);

    public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
    {
        ((IDictionary<string, object?>)_properties).CopyTo(array, arrayIndex);
    }
    
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _properties.GetEnumerator();

    public bool Remove(string key)
    {
        var removed = _properties.Remove(key);
        if (removed) OnPropertyChanged(key);
        return removed;
    }
    
    public bool Remove(KeyValuePair<string, object?> item)
    {
        var removed = _properties.Remove(item.Key);
        if (removed) OnPropertyChanged(item.Key);
        return removed;
    }

    public bool TryGetValue(string key, out object? value) => _properties.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}