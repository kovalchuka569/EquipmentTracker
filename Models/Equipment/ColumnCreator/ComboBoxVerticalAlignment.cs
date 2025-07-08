using System.Collections.ObjectModel;
using System.Windows;

namespace Models.Equipment.ColumnCreator;

public class ComboBoxVerticalAlignment
{
    public string Title { get; set; }
    public VerticalAlignment Alignment { get; set; }

    public static ObservableCollection<ComboBoxVerticalAlignment> GetComboBoxVerticalAlignments()
    {
        return new ObservableCollection<ComboBoxVerticalAlignment>
        {
            new ComboBoxVerticalAlignment { Title = "Зверху", Alignment = VerticalAlignment.Top },
            new ComboBoxVerticalAlignment { Title = "По центру", Alignment = VerticalAlignment.Center },
            new ComboBoxVerticalAlignment { Title = "Знизу", Alignment = VerticalAlignment.Bottom },
        };
    }
}