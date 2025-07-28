using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace Models.Equipment.ColumnCreator;

public class ComboBoxConfig
{ 
    public ObservableCollection<ComboBoxType> ColumnTypes { get; } = ComboBoxType.GetComboBoxTypes();
    
    public ObservableCollection<ComboBoxFontSize> FontSizes { get; } = ComboBoxFontSize.GetComboBoxFontSizes();
    public ObservableCollection<ComboBoxFontFamily> FontFamilies { get; } = ComboBoxFontFamily.GetComboBoxFontFamilies();
    public ObservableCollection<ComboBoxFontWeight> FontWeights { get; } = ComboBoxFontWeight.GetComboBoxFontWeights();
    public ObservableCollection<ComboBoxVerticalAlignment> VerticalAlignments { get; } = ComboBoxVerticalAlignment.GetComboBoxVerticalAlignments();
    public ObservableCollection<ComboBoxHorizontalAlignment> HorizontalAlignments { get; } = ComboBoxHorizontalAlignment.GetComboBoxHorizontalAlignments();
    public ObservableCollection<ComboBoxDateFormat> DateFormats { get; } = ComboBoxDateFormat.GetComboBoxDateFormats();
    public ObservableCollection<ComboBoxCurrency> Currencies { get; } = ComboBoxCurrency.GetComboBoxCurrencies();
    public ObservableCollection<ComboBoxBorders> Borders { get; } = ComboBoxBorders.GetComboBoxBorders();
}