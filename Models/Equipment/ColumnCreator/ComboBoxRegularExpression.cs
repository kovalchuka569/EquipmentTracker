using System.Collections.ObjectModel;

namespace Models.Equipment.ColumnCreator;

public class ComboBoxRegularExpression
{
    public string RegularExpressionTitle { get; set; }
    public string RegularExpressionPattern { get; set; }
    public string Example { get; set; }

    public static ObservableCollection<ComboBoxRegularExpression> GetComboBoxRegularExpressions()
    {
        return new ObservableCollection<ComboBoxRegularExpression>
        {
            new ComboBoxRegularExpression
            {
                RegularExpressionTitle = "Тільки літери (латиниця та кирилиця, без цифр та спецсимволів)",
                RegularExpressionPattern = @"^(|[\s]*|[a-zA-Zа-яА-ЯёЁїЇіІєЄґҐ\s]+)$",
                Example = "текст, Текст, Text, Привіт"
            },
            new ComboBoxRegularExpression
            {
                RegularExpressionTitle = "Тільки буквено-цифрові символи (латиниця, кирилиця, без спецсимволів)",
                RegularExpressionPattern = @"^(|[\s]*|[a-zA-Zа-яА-ЯёЁїЇіІєЄґҐ0-9\s]+)$",
                Example = "Текст123, text123, Привіт2023"
            },
            new ComboBoxRegularExpression
            {
                RegularExpressionTitle = "Дозволені символи (літери, цифри, пробіли, деякі спецсимволи, латиниця та кирилиця)",
                RegularExpressionPattern = @"^(|[\s]*|[a-zA-Zа-яА-ЯёЁїЇіІєЄґҐ0-9\s.,!?-]+)$",
                Example = "Привіт, світ! Hello world! 123"
            }
        };
    }
}