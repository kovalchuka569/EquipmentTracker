namespace Core.Common.Helpers;

public class PluralizedHelper
{
    public static string GetPluralizedText(int count, string singular, string plural234, string pluralOther)
    {
        int lastDigit = count % 10;
        int lastTwoDigits = count % 100;

        if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
        {
            return $"{count} {pluralOther}";
        }

        return lastDigit switch
        {
            1 => $"{count} {singular}",
            2 or 3 or 4 => $"{count} {plural234}",
            _ => $"{count} {pluralOther}",
        };
    }
}