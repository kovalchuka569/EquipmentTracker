using System.Collections.ObjectModel;

namespace Models.Equipment.ColumnCreator;

public class ComboBoxDateFormat
{
    public string Title { get; set; }
    public string Format { get; set; }
    public string Example { get; set; }

    public static ObservableCollection<ComboBoxDateFormat> GetComboBoxDateFormats()
    {
        return new ObservableCollection<ComboBoxDateFormat>
        {
            new ComboBoxDateFormat
            {
                Title = "Класичний формат", 
                Format = "dd.MM.yyyy", 
                Example = "01.01.2001"
            },
            new ComboBoxDateFormat
            {
                Title = "Рік", 
                Format = "yyyy", 
                Example = "2001"
            },
            new ComboBoxDateFormat
            {
                Title = "Рік та місяць", 
                Format = "yyyy-MM", 
                Example = "2001-01"
            },
            new ComboBoxDateFormat
            {
                Title = "Текстовий рік та місяць", 
                Format = "MMMM yyyy", 
                Example = "січень 2001"
            },
            new ComboBoxDateFormat
            { 
                Title = "Повна дата з часом (24 години)", 
                Format = "dd.MM.yyyy HH:mm", 
                Example = "01.01.2001 00:01" 
            },
            new ComboBoxDateFormat
            { 
                Title = "Класичний текстовий формат", 
                Format = "dd MMMM yyyy", 
                Example = "1 січня 2001" 
            },
            new ComboBoxDateFormat
            {
                Title = "Текстовий формат з днем тижня", 
                Format = "dddd, dd MMMM yyyy",
                Example = "понеділок, 1 січня 2001"
            },
        };
    }
}