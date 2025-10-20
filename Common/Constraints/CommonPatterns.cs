namespace Common.Constraints;

public static class CommonPatterns
{
    /// <summary>
    /// Lowercase letters (Latin + Cyrillic)
    /// </summary>
    public const string Lowercase = "[a-zа-яёіїєґ]";  
    
    /// <summary>
    /// Uppercase letters (Latin + Cyrillic)
    /// </summary>
    public const string Uppercase = "[A-ZА-ЯЁІЇЄҐ]";    
    
    /// <summary>
    /// Digits from 0 to 9
    /// </summary>
    public const string Digits = "[0-9]"; 
}