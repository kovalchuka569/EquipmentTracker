using System.Collections.ObjectModel;
using System.Windows;

namespace Models.Equipment.ColumnCreator;

public class ComboBoxHorizontalAlignment
{
    public string Title { get; set; }
    public HorizontalAlignment Alignment { get; set; }

    public static ObservableCollection<ComboBoxHorizontalAlignment> GetComboBoxHorizontalAlignments()
    {
        return new ObservableCollection<ComboBoxHorizontalAlignment>
        {
            new ComboBoxHorizontalAlignment { Title = "Зліва", Alignment = HorizontalAlignment.Left },
            new ComboBoxHorizontalAlignment { Title = "По центру", Alignment = HorizontalAlignment.Center },
            new ComboBoxHorizontalAlignment { Title = "Зправа", Alignment = HorizontalAlignment.Right },
        };
    }
}