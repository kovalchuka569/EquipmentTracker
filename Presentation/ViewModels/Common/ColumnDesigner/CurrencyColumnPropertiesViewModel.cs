using System;
using System.Collections.Generic;
using System.Linq;

using Models.Common.Formatting;
using Models.Common.Table.ColumnProperties;
using Models.Equipment;

using Presentation.Attributes;
using Presentation.Enums;

namespace Presentation.ViewModels.Common.ColumnDesigner;

public class CurrencyColumnPropertiesViewModel : BaseColumnPropertiesViewModel
{
    
    #region Private fields
    
    private decimal? _defaultValue;
    private bool _hasDefaultValue;
    private List<CurrencySymbol> _currencySymbols = Data.Shared.CurrencySymbols.GetCurrencySymbols();
    private CurrencySymbol _selectedCurrencySymbol;
    
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
            if(value is not (decimal or null))
                return;
            
            SetProperty(ref _defaultValue, (decimal?)value);
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
                ? 0
                : null;
        }
    }

    [DisplayName(ColumnDesignerConstants.CurrencySymbolUiName)]
    [Description(ColumnDesignerConstants.CurrencySymbolUiDescription)]
    [GroupName(ColumnDesignerConstants.GeneralPropertiesGroupName)]
    [Editor(EditorType.CurrencySymbolEditor)]
    [Order(100)]
    public List<CurrencySymbol> CurrencySymbols
    {
        get => _currencySymbols;
        set => SetProperty(ref _currencySymbols, value);
    }

    public CurrencySymbol SelectedCurrencySymbol
    {
        get => _selectedCurrencySymbol;
        set => SetProperty(ref _selectedCurrencySymbol, value);
    }
    
    #endregion
    
    #region Constructor
    

    public CurrencyColumnPropertiesViewModel()
    {
        ColumnDataType = ColumnDataType.Currency;

        _selectedCurrencySymbol = _currencySymbols.First();
    }
    
    #endregion
    
    #region Private methods
    
    protected override void ValidateDefaultValue() { }

    #endregion

    #region Public methods

    public override void FromDomain(BaseColumnProperties domain)
    {
        if (domain is not CurrencyColumnProperties currencyProperties)
            throw new InvalidCastException();
        
        CopyBaseFromDomain(domain);
        _hasDefaultValue = currencyProperties.HasDefaultValue;
        _defaultValue = Convert.ToDecimal(currencyProperties.DefaultValue);
        _selectedCurrencySymbol = _currencySymbols
            .First(s => 
                s.Symbol == currencyProperties.Symbol);
    }

    public override BaseColumnProperties ToDomain()
    {
        var domain = new CurrencyColumnProperties();
        CopyBaseToDomain(domain);
        domain.HasDefaultValue = HasDefaultValue;
        domain.DefaultValue = DefaultValue;
        domain.Symbol = SelectedCurrencySymbol.Symbol;
        return domain;
    }

    #endregion
}