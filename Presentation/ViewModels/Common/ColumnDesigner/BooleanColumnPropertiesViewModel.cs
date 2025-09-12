using System;
using Models.Common.Table.ColumnProperties;
using Models.Equipment;
using Presentation.Attributes;
using Presentation.Enums;

namespace Presentation.ViewModels.Common.ColumnDesigner;

public class BooleanColumnPropertiesViewModel : BaseColumnPropertiesViewModel
{
    #region Private fields
    
    private bool? _defaultValue;
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
            if(value is not (bool or null))
                return;
            
            SetProperty(ref _defaultValue, (bool?)value);
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
                ? false
                : null;
        }
    }
    
    #endregion
    
    #region Constructor

    public BooleanColumnPropertiesViewModel()
    {
        ColumnDataType = ColumnDataType.Boolean;
    }
    
    #endregion

    #region Public methods

    public override void FromDomain(BaseColumnProperties domain)
    {
        if (domain is not BooleanColumnProperties booleanDomain)
            throw new InvalidCastException();
        
        CopyBaseFromDomain(booleanDomain);
        _hasDefaultValue = booleanDomain.HasDefaultValue;
        _defaultValue = (bool?)booleanDomain.DefaultValue;
    }

    public override BaseColumnProperties ToDomain()
    {
        var domain = new BooleanColumnProperties();
        CopyBaseToDomain(domain);
        domain.HasDefaultValue = HasDefaultValue;
        domain.DefaultValue = DefaultValue;
        return domain;
    }

    #endregion
    
    #region Private methods
    
    protected override void ValidateDefaultValue() { }
    
    #endregion
}