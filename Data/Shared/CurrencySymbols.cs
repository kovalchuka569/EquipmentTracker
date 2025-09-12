using Models.Common.Formatting;

namespace Data.Shared;

public class CurrencySymbols
{

    #region Currency symbols
    
    public static readonly List<CurrencySymbol> SymbolsList =
    [
        new CurrencySymbol
        {
            Symbol = string.Empty,
            Name = "Без символу"
        },
        new CurrencySymbol
        {
            Symbol = "\u20b4",
            Name = "Українська гривня (UAH)"
        },
        new CurrencySymbol
        {
            Symbol = "\u0024",
            Name = "Долар США (USD)"
        },
        new CurrencySymbol
        {
            Symbol = "\u20ac",
            Name = "Євро (EUR)"
        },
        new CurrencySymbol
        {
            Symbol = "\u00a3",
            Name = "Фунт стерлінгів (GBP)"
        },
        new CurrencySymbol
        {
            Symbol = "\u00a5",
            Name = "Японська єна (JPY)"
        },
        new CurrencySymbol
        {
            Symbol = "\u20a3",
            Name = "Швейцарський франк (CHF)"
        },
        new CurrencySymbol
        {
            Symbol = "C\u0024",
            Name = "Канадський долар (CAD)"
        },
        new CurrencySymbol
        {
            Symbol = "A\u0024",
            Name = "Австралійський долар (AUD)"
        },
        new CurrencySymbol
        {
            Symbol = "\u00a5",
            Name = "Китайський юань (CNY)"
        },
        new CurrencySymbol
        {
            Symbol = "\u20a9",
            Name = "Південно-корейська вона (KRW)"
        },
        new CurrencySymbol
        {
            Symbol = "\u20b9",
            Name = "Індійська рупія (INR)"
        },
        new CurrencySymbol
        {
            Symbol = "S\u0024",
            Name = "Сінгапурський долар (SGD)"
        },
        new CurrencySymbol
        {
            Symbol = "\u20bd",
            Name = "Російський рубль (RUB)"
        },
        new CurrencySymbol
        {
            Symbol = "R\u0024",
            Name = "Бразильський реал (BRL)"
        },
        new CurrencySymbol
        {
            Symbol = "\u0024",
            Name = "Мексиканське песо (MXN)"
        },
        new CurrencySymbol
        {
            Symbol = "R",
            Name = "Південно-африканський ранд (ZAR)"
        },
        new CurrencySymbol
        {
            Symbol = "z\u0142",
            Name = "Польський злотий (PLN)"
        },
        new CurrencySymbol
        {
            Symbol = "K\u010d",
            Name = "Чеська крона (CZK)"
        },
        new CurrencySymbol
        {
            Symbol = "kr",
            Name = "Шведська крона (SEK)"
        },
        new CurrencySymbol
        {
            Symbol = "kr",
            Name = "Норвезька крона (NOK)"
        },
        new CurrencySymbol
        {
            Symbol = "kr",
            Name = "Данська крона (DKK)"
        },
        new CurrencySymbol
        {
            Symbol = "\u20ba",
            Name = "Турецька ліра (TRY)"
        },
        new CurrencySymbol
        {
            Symbol = "\u20aa",
            Name = "Ізраїльський шекель (ILS)"
        },
        new CurrencySymbol
        {
            Symbol = "\u0e3f",
            Name = "Тайський бат (THB)"
        },
        new CurrencySymbol
        {
            Symbol = "\u20ab",
            Name = "В'єтнамський донг (VND)"
        },
        new CurrencySymbol
        {
            Symbol = "\u20b1",
            Name = "Філіппінське песо (PHP)"
        },
        new CurrencySymbol
        {
            Symbol = "\u20a8",
            Name = "Пакистанська рупія (PKR)"
        },
        new CurrencySymbol
        {
            Symbol = "\u09f3",
            Name = "Бангладешська така (BDT)"
        },
        new CurrencySymbol
        {
            Symbol = "\u20a6",
            Name = "Нігерійська найра (NGN)"
        },
        new CurrencySymbol
        {
            Symbol = "\u20a1",
            Name = "Аргентинське песо (ARS)"
        },
        new CurrencySymbol
        {
            Symbol = "\u0024",
            Name = "Чилійське песо (CLP)"
        },
        new CurrencySymbol
        {
            Symbol = "\u0024",
            Name = "Колумбійське песо (COP)"
        },
        new CurrencySymbol
        {
            Symbol = "\u20b5",
            Name = "Ганський седі (GHS)"
        },
        new CurrencySymbol
        {
            Symbol = "\u20a9",
            Name = "Південнокорейська вона (KRW)"
        },
        new CurrencySymbol
        {
            Symbol = "\u0024",
            Name = "Доларова валюта (інша)"
        },
        new CurrencySymbol
        {
            Symbol = "\u20ac",
            Name = "Євро (країни ЄС)"
        }
    ];
    
    #endregion

    public static List<CurrencySymbol> GetCurrencySymbols() => SymbolsList;

}