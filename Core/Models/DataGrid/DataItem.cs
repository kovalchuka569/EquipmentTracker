using System.Collections.Generic;
using System.Dynamic;
using System.ComponentModel;

namespace Core.Models.DataGrid;

public class DataItem : DynamicObject, INotifyPropertyChanged, ICustomTypeDescriptor
{
    private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();
    
    public event PropertyChangedEventHandler PropertyChanged;

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        string name = binder.Name;
        return TryGetValue(name, out result);
    }

    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        string name = binder.Name;
        _properties[name] = value;
        OnPropertyChanged(name);
        return true;
    }

    public object this[string name]
    {
        get
        {
            if (TryGetValue(name, out var result))
                return result;
            
            return null;
        }
        set
        {
            _properties[name] = value;
            OnPropertyChanged(name);
        }
    }

    public bool TryGetValue(string name, out object result)
    {
        if (_properties.TryGetValue(name, out result))
            return true;
        
        result = null;
        return false;
    }

    public IEnumerable<string> GetPropertyNames()
    {
        return _properties.Keys;
    }
    
    public override IEnumerable<string> GetDynamicMemberNames()
    {
        return _properties.Keys;
    }
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    #region ICustomTypeDescriptor implementation
    public AttributeCollection GetAttributes() => AttributeCollection.Empty;
    public string GetClassName() => GetType().Name;
    public string GetComponentName() => null;
    public TypeConverter GetConverter() => null;
    public EventDescriptor GetDefaultEvent() => null;
    public PropertyDescriptor GetDefaultProperty() => null;
    public object GetEditor(Type editorBaseType) => null;
    public EventDescriptorCollection GetEvents() => EventDescriptorCollection.Empty;
    public EventDescriptorCollection GetEvents(Attribute[] attributes) => EventDescriptorCollection.Empty;
    
    public PropertyDescriptorCollection GetProperties()
    {
        var properties = new List<PropertyDescriptor>();
        foreach (var key in _properties.Keys)
        {
            properties.Add(new DynamicPropertyDescriptor(key, _properties[key]?.GetType() ?? typeof(object)));
        }
        return new PropertyDescriptorCollection(properties.ToArray());
    }
    
    public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
    {
        return GetProperties();
    }
    
    public object GetPropertyOwner(PropertyDescriptor pd)
    {
        return this;
    }
    #endregion
    
    private class DynamicPropertyDescriptor : PropertyDescriptor
    {
        private readonly string _name;
        private readonly Type _type;
        
        public DynamicPropertyDescriptor(string name, Type type) 
            : base(name, null)
        {
            _name = name;
            _type = type;
        }
        
        public override Type ComponentType => typeof(DataItem);
        public override bool IsReadOnly => false;
        public override Type PropertyType => _type;
        public override bool CanResetValue(object component) => false;
        public override object GetValue(object component) => ((DataItem)component)[_name];
        public override void ResetValue(object component) { }
        public override void SetValue(object component, object value) => ((DataItem)component)[_name] = value;
        public override bool ShouldSerializeValue(object component) => true;
    }
}