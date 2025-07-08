using System.Collections.ObjectModel;
using System.Windows;

namespace Models.Equipment.ColumnCreator;

public class ComboBoxFontWeight
{
    public string Title { get; set; }
    public FontWeight FontWeight { get; set; }

    public static ObservableCollection<ComboBoxFontWeight> GetComboBoxFontWeights()
    {
        return new ObservableCollection<ComboBoxFontWeight>
        {
            new ComboBoxFontWeight { Title = "Тонкий", FontWeight = FontWeights.Thin },
            new ComboBoxFontWeight { Title = "Жирний", FontWeight = FontWeights.Bold },
        };
    }
}