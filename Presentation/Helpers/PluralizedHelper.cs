using Resources.Localization;

namespace Presentation.Helpers;

public static class PluralizedHelper
{
    public static string GetPluralizedText(int count, string singular, string plural234, string pluralOther)
    {
        var lastDigit = count % 10;
        var lastTwoDigits = count % 100;

        if (lastTwoDigits is >= 11 and <= 14)
            return $"{count} {pluralOther}";

        return lastDigit switch
        {
            1 => $"{count} {singular}",
            2 or 3 or 4 => $"{count} {plural234}",
            _ => $"{count} {pluralOther}",
        };
    }
    
    public static string GetSymbolPlural(int count) => GetPluralizedText(count, Plurals.Symbol_Singular, Plurals.Symbol_Plural234, Plurals.Symbol_PluralOther);
    
    public static string GetLetterPlural(int count) => GetPluralizedText(count, Plurals.Letter_Singular, Plurals.Letter_Plural234, Plurals.Letter_Other);
    
    public static string GetDigitPlural(int count) => GetPluralizedText(count, Plurals.Digit_Singular, Plurals.Digit_Plural234, Plurals.Digit_Other);

    public static string GetUppercasePlural(int count) => GetPluralizedText(count, Plurals.Uppercase_Singular, Plurals.Uppercase_Plural234, Plurals.Uppercase_Other);
    
    public static string GetLowercasePlural(int count) => GetPluralizedText(count, Plurals.Lowercase_Singular, Plurals.Lowercase_Plural234, Plurals.Lowercase_Other);
    
    public static string GetSpecialCharsFormat(int count) => GetPluralizedText(count, Plurals.SpecialChar_Singular, Plurals.SpecialChar_Plural234, Plurals.SpecialChar_Other);
    
    public static string GetSequentialFormat(int count) => GetPluralizedText(count, Plurals.SequentialChars_Singular, Plurals.SequentialChars_Plural234, Plurals.SequentialChars_Other);
    
    public static string GetConsecutiveFormat(int count) => GetPluralizedText(count, Plurals.Consecutive_Singular, Plurals.Consecutive_Plural234, Plurals.Consecutive_Other);
}