using System.Collections.ObjectModel;
using System.Windows;

namespace Models.Equipment.ColumnCreator;

public class ComboBoxBorders
{
    public string Title { get; set; }
    public Thickness BorderThickness { get; set; }

    public static ObservableCollection<ComboBoxBorders> GetComboBoxBorders()
    {
        return new ObservableCollection<ComboBoxBorders>
        {
            new ComboBoxBorders
            {
                Title = "Зліва",
                BorderThickness = new Thickness(3, 0, 0, 0),
            },
            new ComboBoxBorders
            {
                Title = "Зправа",
                BorderThickness = new Thickness(0, 0, 3, 0),
            },
            new ComboBoxBorders
            {
                Title = "Зверху",
                BorderThickness = new Thickness(0, 3, 0, 0),
            },
            new ComboBoxBorders
            {
                Title = "Знизу",
                BorderThickness = new Thickness(0, 0, 0, 3),
            },
            new ComboBoxBorders
            {
                Title = "Скрізь",
                BorderThickness = new Thickness(3, 3, 3, 3),
            },
            new ComboBoxBorders
            {
                Title = "Відсутні",
                BorderThickness = new Thickness(0, 0, 0, 0),
            },
        };
    }
}