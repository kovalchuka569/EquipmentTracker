using System.Collections.ObjectModel;

namespace Models.Equipment.ColumnCreator;

public class ComboBoxFontSize
{
    public string Example { get; set; } = "Приклад";
    public double FontSize { get; set; }

    public static ObservableCollection<ComboBoxFontSize> GetComboBoxFontSizes()
    {
        return new ObservableCollection<ComboBoxFontSize>
        {
            new ComboBoxFontSize { FontSize = 6 },
            new ComboBoxFontSize { FontSize = 8 },
            new ComboBoxFontSize { FontSize = 10 },
            new ComboBoxFontSize { FontSize = 12 },
            new ComboBoxFontSize { FontSize = 14 },
            new ComboBoxFontSize { FontSize = 16 },
            new ComboBoxFontSize { FontSize = 18 },
        };
    }

}