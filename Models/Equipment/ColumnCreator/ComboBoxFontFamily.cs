using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Models.Equipment.ColumnCreator;

public class ComboBoxFontFamily
{
    public string FontFamily { get; set; }
    public static ObservableCollection<ComboBoxFontFamily> GetComboBoxFontFamilies()
    {
        return new ObservableCollection<ComboBoxFontFamily>
        {
            new ComboBoxFontFamily {FontFamily = "Segoe UI"},
            new ComboBoxFontFamily {FontFamily = "Times New Roman"},
            new ComboBoxFontFamily {FontFamily = "Comic Sans MS"},
            new ComboBoxFontFamily {FontFamily = "Roboto"},
            new ComboBoxFontFamily {FontFamily = "Arial"},
            new ComboBoxFontFamily {FontFamily = "Courier New"},
        };
    }
}