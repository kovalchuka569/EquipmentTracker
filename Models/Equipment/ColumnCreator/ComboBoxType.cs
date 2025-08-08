using System.Collections.ObjectModel;

namespace Models.Equipment.ColumnCreator;

public class ComboBoxType
{
    public string TypeTitle { get; set; }
    public string TypeDescription { get; set; }
    public ColumnDataType ColumnDataType { get; set; }

    public static ObservableCollection<ComboBoxType> GetComboBoxTypes()
    {
        return new ObservableCollection<ComboBoxType>
        {
            new ComboBoxType
            {
                TypeTitle = "Текст",
                TypeDescription = "Рядок символів — використовується для збереження слів, номерів з використанням букв, тощо",
                ColumnDataType = ColumnDataType.Text
            },
            new ComboBoxType
            {
                TypeTitle = "Число (ціле, дробове)",
                TypeDescription = "Числове значення напр., 3.14 або 5.",
                ColumnDataType = ColumnDataType.Number
            },
            new ComboBoxType
            {
                TypeTitle = "Дата", 
                TypeDescription = "Значення дати або дати й часу, напр., 2025-06-17.",
                ColumnDataType = ColumnDataType.Date
            },
            new ComboBoxType
            {
                TypeTitle = "Логічний тип", 
                TypeDescription = "Має лише два значення: Так / Ні.",
                ColumnDataType = ColumnDataType.Boolean
            },
            new ComboBoxType
            {
                TypeTitle = "Гіперпосилання",
                TypeDescription = "Відображає клікабельне посилання, яке відкривається в браузері.",
                ColumnDataType = ColumnDataType.Hyperlink
            },
            new ComboBoxType
            {
                TypeTitle = "Список", 
                TypeDescription = "Може зберігати в собі список, наприклад одиниць виміру.",
                ColumnDataType = ColumnDataType.List
            },
            new ComboBoxType
            {
                TypeTitle = "Валюта", 
                TypeDescription = "Число + валюта, з форматуванням.",
                ColumnDataType = ColumnDataType.Currency
            },
        };
    }
}