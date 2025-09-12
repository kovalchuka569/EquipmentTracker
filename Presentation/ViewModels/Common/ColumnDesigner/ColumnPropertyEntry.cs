using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Presentation.Attributes;
using Presentation.Enums;
using Prism.Mvvm;

using DescriptionAttribute = Presentation.Attributes.DescriptionAttribute;
using DisplayNameAttribute = Presentation.Attributes.DisplayNameAttribute;
using EditorAttribute = Presentation.Attributes.EditorAttribute;

namespace Presentation.ViewModels.Common.ColumnDesigner;

public class ColumnPropertyEntry : BindableBase, INotifyDataErrorInfo
{
    #region Dependencies
    
    private readonly BaseColumnPropertiesViewModel _owner;
    
    private readonly PropertyInfo _pi;
    
    #endregion
    
    #region Private fields

    private string _category = string.Empty;

    private string _displayName = string.Empty;
    
    private string _description = string.Empty;
    
    private EditorType _editorType = EditorType.None;
    
    #endregion
    
    #region Public fields

    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }

    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public EditorType EditorType
    {
        get => _editorType;
        set => SetProperty(ref _editorType, value);
    }
    
    public BaseColumnPropertiesViewModel Owner => _owner;
    
    public string PropertyName => _pi.Name;
    
    public object? Value
    {
        get => _pi.GetValue(_owner);
        set
        {
            if(Equals(_pi.GetValue(_owner), value)) 
                return;
            
            _pi.SetValue(_owner, value);
            
            RaisePropertyChanged();
            
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Value)));
        }
    }
    
    #endregion
    
    #region Constructor

    public ColumnPropertyEntry(BaseColumnPropertiesViewModel owner, PropertyInfo pi)
    {
        _owner = owner;
        _pi = pi;
        
        Category = pi.GetCustomAttribute<GroupNameAttribute>()?.GroupName ?? string.Empty;
        
        DisplayName = pi.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? string.Empty;
        
        Description = pi.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
        
        EditorType = pi.GetCustomAttribute<EditorAttribute>()?.EditorType ?? EditorType.None;

        _owner.ErrorsChanged += OnOwnerErrorsChanged;
    }
    
    #endregion

    public IEnumerable GetErrors(string? propertyName)
    {
        return propertyName == nameof(Value) ? _owner.GetErrors(PropertyName) : Enumerable.Empty<string>();
    }

    private void OnOwnerErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        if (e.PropertyName == PropertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Value)));
        }
    }

    public bool HasErrors => GetErrors(nameof(Value))
                            .Cast<string>()
                            .Any();
    
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    ~ColumnPropertyEntry()
    {
        _owner.ErrorsChanged -= OnOwnerErrorsChanged;
    }
}