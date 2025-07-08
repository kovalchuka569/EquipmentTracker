using System.Collections.ObjectModel;
using System.Windows;

namespace Models.Equipment.ColumnCreator;

public class ComboBoxCurrency
{
    public string Currency { get; set; }
    public string Description { get; set; }
    
    public Style Style { get; set; }
    
    public static ObservableCollection<ComboBoxCurrency> GetComboBoxCurrencies()
    {
        return new ObservableCollection<ComboBoxCurrency>
        {
            new ComboBoxCurrency{Currency = "", Description = "Без символу"},
            new ComboBoxCurrency{Currency = "\u20b4", Description = "Українська гривня (UAH)"},
            new ComboBoxCurrency{Currency = "\u0024", Description = "Долар США (USD)"},
            new ComboBoxCurrency{Currency = "\u20ac", Description = "Євро (EUR)"},
            new ComboBoxCurrency{Currency = "\u00a3", Description = "Фунт стерлінгів (GBP)"},
            new ComboBoxCurrency{Currency = "\u00a5", Description = "Японська єна (JPY)"},
            new ComboBoxCurrency{Currency = "\u20a3", Description = "Швейцарський франк (CHF)"},
            new ComboBoxCurrency{Currency = "C\u0024", Description = "Канадський долар (CAD)"},
            new ComboBoxCurrency{Currency = "A\u0024", Description = "Австралійський долар (AUD)"},
            new ComboBoxCurrency{Currency = "\u00a5", Description = "Китайський юань (CNY)"},
            new ComboBoxCurrency{Currency = "\u20a9", Description = "Південно-корейська вона (KRW)"},
            new ComboBoxCurrency{Currency = "\u20b9", Description = "Індійська рупія (INR)"},
            new ComboBoxCurrency{Currency = "S\u0024", Description = "Сінгапурський долар (SGD)"},
            new ComboBoxCurrency{Currency = "\u20bd", Description = "Російський рубль (RUB)"},
            new ComboBoxCurrency{Currency = "R\u0024", Description = "Бразильський реал (BRL)"},
            new ComboBoxCurrency{Currency = "\u0024", Description = "Мексиканське песо (MXN)"},
            new ComboBoxCurrency{Currency = "R", Description = "Південно-африканський ранд (ZAR)"},
            new ComboBoxCurrency{Currency = "z\u0142", Description = "Польський злотий (PLN)"},
            new ComboBoxCurrency{Currency = "K\u010d", Description = "Чеська крона (CZK)"},
            new ComboBoxCurrency{Currency = "kr", Description = "Шведська крона (SEK)"},
            new ComboBoxCurrency{Currency = "kr", Description = "Норвезька крона (NOK)"},
            new ComboBoxCurrency{Currency = "kr", Description = "Данська крона (DKK)"},
            new ComboBoxCurrency{Currency = "\u20ba", Description = "Турецька ліра (TRY)"},
            new ComboBoxCurrency{Currency = "\u20aa", Description = "Ізраїльський шекель (ILS)"},
            new ComboBoxCurrency{Currency = "\u0e3f", Description = "Тайський бат (THB)"},
            new ComboBoxCurrency{Currency = "\u20ab", Description = "В'єтнамський донг (VND)"},
            new ComboBoxCurrency{Currency = "\u20b1", Description = "Філіппінське песо (PHP)"},
            new ComboBoxCurrency{Currency = "\u20a8", Description = "Пакистанська рупія (PKR)"},
            new ComboBoxCurrency{Currency = "\u09f3", Description = "Бангладешська така (BDT)"},
            new ComboBoxCurrency{Currency = "\u20a6", Description = "Нігерійська найра (NGN)"},
            new ComboBoxCurrency{Currency = "\u20a1", Description = "Аргентинське песо (ARS)"},
            new ComboBoxCurrency{Currency = "\u0024", Description = "Чилійське песо (CLP)"},
            new ComboBoxCurrency{Currency = "\u0024", Description = "Колумбійське песо (COP)"},
            new ComboBoxCurrency{Currency = "\u20b5", Description = "Ганський седі (GHS)"},
            new ComboBoxCurrency{Currency = "\u20a9", Description = "Південнокорейська вона (KRW)"},
            new ComboBoxCurrency{Currency = "\u0024", Description = "Доларова валюта (інша)"},
            new ComboBoxCurrency{Currency = "\u20ac", Description = "Євро (країни ЄС)"},
        };
    }
}