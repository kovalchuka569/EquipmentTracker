using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Models.Common.Table.ColumnProperties;
using Prism.Mvvm;
using Models.Equipment;
using Presentation.Attributes;
using Presentation.Enums;
using Presentation.EventArgs;
using EditorAttribute = Presentation.Attributes.EditorAttribute;
using DescriptionAttribute = Presentation.Attributes.DescriptionAttribute;
using DisplayNameAttribute = Presentation.Attributes.DisplayNameAttribute;

namespace Presentation.ViewModels.Common.ColumnDesigner;

public abstract class BaseColumnPropertiesViewModel : BindableBase, INotifyDataErrorInfo
{
    
    #region Public contracts
    
    public abstract object? DefaultValue { get; set; }
    public abstract bool HasDefaultValue { get; set; }
    
    #endregion
    
    #region Сontracts
    
    protected abstract void ValidateDefaultValue();
    public abstract void FromDomain(BaseColumnProperties domain);
    public abstract BaseColumnProperties ToDomain();
    
    #endregion
    
    #region Private fields
    
    private string _headerText = ColumnDesignerConstants.DefaultHeaderText;
    private ColumnDataType _columnDataType = ColumnDesignerConstants.DefaultColumnDataType;
    private List<EnumDisplay<ColumnDataType>> _columnDataTypes =
    [
    new (ColumnDataType.Text,      ColumnDesignerConstants.ColumnDataTypeTextDisplay),
    new (ColumnDataType.Number,    ColumnDesignerConstants.ColumnDataTypeNumberDisplay),
    new (ColumnDataType.Date,      ColumnDesignerConstants.ColumnDataTypeDateDisplay),
    new (ColumnDataType.Boolean,   ColumnDesignerConstants.ColumnDataTypeBooleanDisplay),
    new (ColumnDataType.List,      ColumnDesignerConstants.ColumnDataTypeListDisplay),
    new (ColumnDataType.Hyperlink, ColumnDesignerConstants.ColumnDataTypeHyperlinkDisplay),
    new (ColumnDataType.Currency,  ColumnDesignerConstants.ColumnDataTypeCurrencyDisplay)
    ];
    
    private EnumDisplay<ColumnDataType> _selectedColumnDataType;
    private long _order;
    private double _headerWidth = ColumnDesignerConstants.DefaultColumnWidth;
    private bool _isFrozen = ColumnDesignerConstants.DefaultIsFrozen;
    private bool _isUnique = ColumnDesignerConstants.DefaultIsUnique;
    private bool _isRequired = ColumnDesignerConstants.DefaultIsRequired;
    private bool _markedForDelete;
    private readonly Dictionary<string, List<string>> _propertyErrors = new();
    private bool _isNew;
    private bool _isModified;
    
    #endregion
    
    #region Public fields
    
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string MappingName { get; set; } = Guid.NewGuid().ToString();
    
    [DisplayName(ColumnDesignerConstants.HeaderTextDisplayedUiName)]
    [Description(ColumnDesignerConstants.HeaderTextPropertyUiDescription)]
    [GroupName(ColumnDesignerConstants.GeneralPropertiesGroupName)]
    [Editor(EditorType.TextBox)]
    [Order(1)]
    public string HeaderText    
    {
        get => _headerText;
        set
        {
            if(!SetProperty(ref _headerText, value))
                return;

            ValidateHeaderText();
        }
    }
    
    
    public ColumnDataType ColumnDataType
    {
        get => _columnDataType;
        set
        {
            if (!SetProperty(ref _columnDataType, value))
                return;
                
            SelectedColumnDataType = ColumnDataTypes
                .First(e => e.Enum == value);
            
            HasDefaultValue = false;
            DefaultValue = null;
        }
    }

    public List<EnumDisplay<ColumnDataType>> ColumnDataTypes
    {
        get => _columnDataTypes;
        set => SetProperty(ref _columnDataTypes, value);
    }

    public EnumDisplay<ColumnDataType> SelectedColumnDataType
    {
        get => _selectedColumnDataType;
        set
        {
            if(!SetProperty(ref _selectedColumnDataType, value))
                return;
            
            ColumnDataType = _selectedColumnDataType.Enum;
            ColumnDataTypeChanged?.Invoke(this, new ColumnDataTypeChangedEventArgs(value.Enum));
        }
    }
    
    public long Order
    {
        get => _order;
        set => SetProperty(ref _order, value);
    }

    [DisplayName(ColumnDesignerConstants.WidthDisplayedUiName)]
    [Description(ColumnDesignerConstants.WidthPropertyUiDescription)]
    [GroupName(ColumnDesignerConstants.GeneralPropertiesGroupName)]
    [Editor(EditorType.DoubleTextBoxEditor)]
    [Order(3)]
    public double HeaderWidth
    {
        get => _headerWidth;
        set
        {
            if(!SetProperty(ref _headerWidth, value))
                return;

            ValidateHeaderWidth();
        }
    }

    [DisplayName(ColumnDesignerConstants.IsFrozenDisplayedUiName)]
    [Description(ColumnDesignerConstants.IsFrozenPropertyUiDescription)]
    [GroupName(ColumnDesignerConstants.GeneralPropertiesGroupName)]
    [Editor(EditorType.CheckBox)]
    [Order(4)]
    public bool IsFrozen
    {
        get => _isFrozen;
        set => SetProperty(ref _isFrozen, value);
    }

    [DisplayName(ColumnDesignerConstants.IsUniqueDisplayedUiName)]
    [Description(ColumnDesignerConstants.IsUniquePropertyUiDescription)]
    [GroupName(ColumnDesignerConstants.ValidatePropertiesGroupName)]
    [Editor(EditorType.CheckBox)]
    [Order(5)]
    public bool IsUnique
    {
        get => _isUnique;
        set => SetProperty(ref _isUnique, value);
    }
    
    [DisplayName(ColumnDesignerConstants.IsRequiredDisplayedUiName)]
    [Description(ColumnDesignerConstants.IsRequiredPropertyUiDescription)]
    [GroupName(ColumnDesignerConstants.ValidatePropertiesGroupName)]
    [Editor(EditorType.CheckBox)]
    [Order(6)]
    public bool IsRequired
    {
        get => _isRequired;
        set => SetProperty(ref _isRequired, value);
    }

    public bool MarkedForDelete
    {
        get => _markedForDelete;
        set => SetProperty(ref _markedForDelete, value);
    }
    
    public bool HasErrors => _propertyErrors.Count != 0;

    public bool IsNew
    {
        get => _isNew;
        set => SetProperty(ref _isNew, value);
    }

    public bool IsModified
    {
        get => _isModified;
        set => SetProperty(ref _isModified, value);
    }
    
    #endregion
    
    #region Events
    
    public event EventHandler<ColumnDataTypeChangedEventArgs>? ColumnDataTypeChanged;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    
    #endregion
    
    #region Constructor
    
    public BaseColumnPropertiesViewModel()
    {
        IsNew = true;
        
        _selectedColumnDataType = _columnDataTypes
            .First(e => e
                .Enum == _columnDataType);
    }
    
    #endregion
    
    #region Private methods
    
    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        RaisePropertyChanged(nameof(HasErrors));
    }

    private void ValidateHeaderText()
    {
        ClearErrors(nameof(HeaderText));
        
        if(_headerText.Length > ColumnDesignerConstants.MaxHeaderTextLength)
            AddError(nameof(HeaderText), ColumnDesignerConstants.HeaderTextLengthErrorMessageUi);
        
        if(_headerText.Length is 0)
            AddError(nameof(HeaderText), ColumnDesignerConstants.HeaderTextEmptyErrorMessageUi);
    }

    private void ValidateHeaderWidth()
    {
        ClearErrors(nameof(HeaderWidth));
        
        if(_headerWidth <= 0) 
            AddError(nameof(HeaderWidth), ColumnDesignerConstants.MinHeaderWidthErrorMessageUi);
    }

    protected void CopyBaseToDomain(BaseColumnProperties domain)
    {
        domain.Id = Id;
        domain.HeaderText = HeaderText;
        domain.MappingName = MappingName;
        domain.ColumnDataType = ColumnDataType;
        domain.Order = Order;
        domain.HeaderWidth = HeaderWidth;
        domain.IsFrozen = IsFrozen;
        domain.IsUnique = IsUnique;
        domain.IsRequired = IsRequired;
        domain.MarkedForDelete = MarkedForDelete;
    }
    
    protected void CopyBaseFromDomain(BaseColumnProperties domain)
    {
        Id = domain.Id;
        MappingName = domain.MappingName;
        ColumnDataType = domain.ColumnDataType;
        HeaderText = domain.HeaderText;
        Order = domain.Order;
        HeaderWidth = domain.HeaderWidth;
        IsFrozen = domain.IsFrozen;
        IsUnique = domain.IsUnique;
        IsRequired = domain.IsRequired;
        MarkedForDelete = domain.MarkedForDelete;
        IsNew = false;
        IsModified = false;
    }

    protected override bool SetProperty<T>(ref T storage, T value, string? propertyName = null)
    {
        if(Equals(storage, value))
            return false;
        
        _isModified = true;
        storage = value;
        RaisePropertyChanged(propertyName);
        RaisePropertyChanged(nameof(IsModified)); 
        return true;
    }

    #endregion

    #region Public methods

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return Enumerable.Empty<string>();
    
        if (_propertyErrors.TryGetValue(propertyName, out var errors))
            return errors;
        
        return Enumerable.Empty<string>();
    }

    public void AddError(string propertyName, string errorMessage)
    {
        if (!_propertyErrors.ContainsKey(propertyName))
        {
            _propertyErrors.Add(propertyName, []);
        }
        _propertyErrors[propertyName].Add(errorMessage);
        OnErrorsChanged(propertyName);
    }

    public void ClearErrors(string propertyName)
    {
        if (!_propertyErrors.ContainsKey(propertyName)) 
            return;
        
        _propertyErrors.Remove(propertyName);
        OnErrorsChanged(propertyName);
    }
    
    #endregion
    
}