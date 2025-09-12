using System;
using System.Collections.Generic;
using System.Linq;
using Models.Common.Formatting;
using Models.Common.Table.ColumnProperties;
using Models.Equipment;
using Presentation.Attributes;
using Presentation.Enums;

namespace Presentation.ViewModels.Common.ColumnDesigner;

public class DateColumnPropertiesViewModel : BaseColumnPropertiesViewModel
{
    #region Private fields

    private DateTime? _defaultValue;
    private bool _hasDefaultValue;
    private List<DatePattern> _datePatterns = Data.Shared.DatePatterns.GetDatePatterns();
    private DatePattern _selectedDatePattern;
    
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
            if(value is not (DateTime or null))
                return;

            SetProperty(ref _defaultValue, (DateTime?)value);
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
                ? DateTime.MinValue
                : null;
        }
    }

    [DisplayName(ColumnDesignerConstants.DatePatternUiName)]
    [Description(ColumnDesignerConstants.DatePatternUiDescription)]
    [GroupName(ColumnDesignerConstants.GeneralPropertiesGroupName)]
    [Editor(EditorType.DatePatternEditor)]
    [Order(100)]
    public List<DatePattern> DatePatterns
    {
        get => _datePatterns;
        set => SetProperty(ref _datePatterns, value);
    }
    
    public DatePattern SelectedDatePattern
    {
        get => _selectedDatePattern;
        set => SetProperty(ref _selectedDatePattern, value);
    }

    #endregion

    #region Constructor
    
    public DateColumnPropertiesViewModel()
    {
        ColumnDataType = ColumnDataType.Date;
        
        _selectedDatePattern = _datePatterns.First();
    }
    
    #endregion
    
    #region Private methods
    
    protected override void ValidateDefaultValue() { }

    #endregion

    #region Public methods

    public override void FromDomain(BaseColumnProperties domain)
    {
        if(domain is not DateColumnProperties dateProperties)
            throw new InvalidCastException();
        
        CopyBaseFromDomain(domain);
        _hasDefaultValue = domain.HasDefaultValue;
        _defaultValue = (DateTime?)dateProperties.DefaultValue;
        _selectedDatePattern = _datePatterns
            .First(p => 
                p.Pattern == dateProperties.Pattern);
    }

    public override BaseColumnProperties ToDomain()
    {
        var domain = new DateColumnProperties();
        CopyBaseToDomain(domain);
        domain.HasDefaultValue = HasDefaultValue;
        domain.DefaultValue = DefaultValue;
        domain.Pattern = SelectedDatePattern.Pattern;
        return domain;
    }

    #endregion
}