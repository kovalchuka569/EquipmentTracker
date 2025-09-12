using System;
using Models.Common.Table.ColumnProperties;
using Models.Equipment;

using Presentation.Attributes;
using Presentation.Enums;

namespace Presentation.ViewModels.Common.ColumnDesigner;

public class NumberColumnPropertiesViewModel : BaseColumnPropertiesViewModel
{
    
    #region Private fields
    
    private double? _defaultValue;
    private bool _hasDefaultValue;
    private int _numberDecimalDigits = ColumnDesignerConstants.DefaultNumberDecimalDigits;
    private double _minNumberValue = ColumnDesignerConstants.DefaultMinNumberValue;
    private double _maxNumberValue = ColumnDesignerConstants.DefaultMaxNumberValue;
    
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
            if (value is not (double or null))
                return;
            
            if(!SetProperty(ref _defaultValue, (double?)value))
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
            
            Console.WriteLine(value);
        
            DefaultValue = value 
                ? MinNumberValue 
                : null;
            
            Console.WriteLine(DefaultValue);
        }
    }

    [DisplayName(ColumnDesignerConstants.NumberDecimalDigitsUiName)]
    [GroupName(ColumnDesignerConstants.GeneralPropertiesGroupName)]
    [Description(ColumnDesignerConstants.NumberDecimalDigitsPropertyUiDescription)]
    [Editor(EditorType.IntegerTextBox)]
    [Order(100)]
    public int NumberDecimalDigits
    {
        get => _numberDecimalDigits;
        set => SetProperty(ref _numberDecimalDigits, value);
    }

    [DisplayName(ColumnDesignerConstants.MinNumberValueUiName)]
    [GroupName(ColumnDesignerConstants.ValidatePropertiesGroupName)]
    [Description(ColumnDesignerConstants.MinNumberValuePropertyUiDescription)]
    [Editor(EditorType.MinMaxNumberEditor)]
    [Order(101)]
    public double MinNumberValue
    {
        get => _minNumberValue;
        set
        {
            if(!SetProperty(ref _minNumberValue, value))
                return;
            
            ValidateMinNumberValue();
        }
    }

    [DisplayName(ColumnDesignerConstants.MaxNumberValueUiName)]
    [GroupName(ColumnDesignerConstants.ValidatePropertiesGroupName)]
    [Description(ColumnDesignerConstants.MaxNumberValuePropertyUiDescription)]
    [Editor(EditorType.MinMaxNumberEditor)]
    [Order(102)]
    public double MaxNumberValue
    {
        get => _maxNumberValue;
        set
        {
            if(!SetProperty(ref _maxNumberValue, value))
                return;

            ValidateMaxNumberValue();
        }
    }
    
    #endregion
    
    #region Constructor

    public NumberColumnPropertiesViewModel()
    {
        ColumnDataType = ColumnDataType.Number;
    }
    
    #endregion

    #region Private methods
    private void ValidateMaxNumberValue() => ValidateRange(fromMax: true);

    private void ValidateMinNumberValue() => ValidateRange(fromMax: false);
    
    private void ValidateRange(bool fromMax)
    {
        ClearErrors(nameof(MaxNumberValue));
        ClearErrors(nameof(MinNumberValue));

        if (_maxNumberValue < _minNumberValue)
        {
            AddError(nameof(MaxNumberValue), ColumnDesignerConstants.MaxLessMinErrorMessageUi);
            AddError(nameof(MinNumberValue), ColumnDesignerConstants.MinGreaterMaxErrorMessageUi);
        }

        RaisePropertyChanged(fromMax 
            ? nameof(MinNumberValue) 
            : nameof(MaxNumberValue));

        if (fromMax)
            ValidateDefaultValue();
        else if (HasDefaultValue)
            DefaultValue = _minNumberValue;
    }
    
    protected override void ValidateDefaultValue()
    {
        ClearErrors(nameof(DefaultValue));
        
        if(!HasDefaultValue)
            return;
        
        if (_defaultValue is null)
            return;
        
        if (_defaultValue.Value > _maxNumberValue)
            AddError(nameof(DefaultValue), ColumnDesignerConstants.DefaultNumberValueGreaterMaxValueErrorMessageUi(MaxNumberValue, NumberDecimalDigits));
        else if (_defaultValue.Value < _minNumberValue)
            AddError(nameof(DefaultValue), ColumnDesignerConstants.DefaultNumberValueLessMinValueErrorMessageUi(MinNumberValue, NumberDecimalDigits));
    }
    
    #endregion

    #region Public methods

    public override void FromDomain(BaseColumnProperties domain)
    {
        if (domain is not NumberColumnProperties numbProperties)
            throw new InvalidCastException();
        
        CopyBaseFromDomain(domain);
        _hasDefaultValue = numbProperties.HasDefaultValue;
        _defaultValue = (double?)numbProperties.DefaultValue;
        _numberDecimalDigits = numbProperties.NumberDecimalDigits;
        _minNumberValue = numbProperties.MinNumberValue;
        _maxNumberValue = numbProperties.MaxNumberValue;
    }

    public override BaseColumnProperties ToDomain()
    {
        var domain = new NumberColumnProperties();
        CopyBaseToDomain(domain);
        domain.HasDefaultValue = HasDefaultValue;
        domain.DefaultValue = DefaultValue;
        domain.NumberDecimalDigits = NumberDecimalDigits;
        domain.MinNumberValue = MinNumberValue;
        domain.MaxNumberValue = MaxNumberValue;
        return domain;
    }

    #endregion
    
}