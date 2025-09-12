using Models.Common.Formatting;

namespace Data.Shared;

public static class DatePatterns
{

    #region Date patterns

    private static readonly List<DatePattern> DatePatternList =
    [
        new DatePattern
        {
            Pattern = "dd.MM.yyyy",
            Label = "Класичний формат",
            Example = "01.01.2001"
        },
        new DatePattern
        {
            Pattern = "yyyy",
            Label = "Рік",
            Example = "2001"
        },
        new DatePattern
        {
            Pattern = "MMMM yyyy",
            Label = "Текстовий рік та місяць",
            Example = "січень 2001"
        },
        new DatePattern
        {
            Pattern = "dd.MM.yyyy HH:mm",
            Label = "Повна дата з часом (24 години)",
            Example = "01.01.2001 00:01"
        },
        new DatePattern
        {
            Pattern = "dd MMMM yyyy",
            Label = "Звичайний текстовий формат",
            Example = "1 січня 2001"
        },
        new DatePattern
        {
            Pattern = "dddd, dd MMMM yyyy",
            Label = "Текстовий формат з днем тижня",
            Example = "понеділок, 1 січня 2001"
        }
    ];

    #endregion

    public static List<DatePattern> GetDatePatterns() => DatePatternList;
}