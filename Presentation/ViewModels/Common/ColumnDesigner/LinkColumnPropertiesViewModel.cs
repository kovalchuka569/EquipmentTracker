using System;
using Models.Common.Table.ColumnProperties;
using Models.Equipment;
using Presentation.Attributes;
using Presentation.Enums;

namespace Presentation.ViewModels.Common.ColumnDesigner;

public class LinkColumnPropertiesViewModel : BaseColumnPropertiesViewModel
{

    #region Private fields

    private string? _defaultValue;
    private bool _hasDefaultValue;
    
    #endregion

    #region Public fields
    
    [DisplayName(ColumnDesignerConstants.DefaultValueDisplayedUiName)]
    [Description(ColumnDesignerConstants.DefaultValuePropertyUiDescription)]
    [GroupName(ColumnDesignerConstants.GeneralPropertiesGroupName)]
    [Editor(EditorType.DefaultValueEditor)]
    [Order(2)]
    public override object? DefaultValue
    {
        get => _defaultValue;
        set
        {
            if(value is not (string or null))
                return;
            
            if(!SetProperty(ref _defaultValue, (string?)value))
                return;
            
            ValidateDefaultValue();
        }
    }

    public override bool HasDefaultValue
    {
        get => _hasDefaultValue;
        set
        {
            if(!SetProperty(ref _hasDefaultValue, value))
                return;
            
            DefaultValue = value 
                ? ColumnDesignerConstants.LinkDefaultValue
                : null;
        }
    }
    
    #endregion

    #region Constructor
    
    public LinkColumnPropertiesViewModel()
    {
        ColumnDataType = ColumnDataType.Hyperlink;
    }
    
    #endregion

    #region Private methods
    
    protected override void ValidateDefaultValue()
    {
        ClearErrors(nameof(DefaultValue));
        
        if(_defaultValue is null)
            return;
        
        if(_defaultValue.Length == 0)
            AddError(nameof(DefaultValue), ColumnDesignerConstants.EmptyDefaultValueErrorMessageUi);
    }
    
    #endregion

    #region Public methods
    
    public override void FromDomain(BaseColumnProperties domain)
    {
        if (domain is not LinkColumnProperties lcProperties)
            throw new InvalidCastException();
        
        CopyBaseFromDomain(domain);
        _hasDefaultValue = lcProperties.HasDefaultValue;
        _defaultValue = (string?)lcProperties.DefaultValue;
    }

    public override BaseColumnProperties ToDomain()
    {
        var domain = new LinkColumnProperties();
        CopyBaseToDomain(domain);
        domain.HasDefaultValue = HasDefaultValue;
        domain.DefaultValue = DefaultValue;
        return domain;
    }
    
    #endregion
}