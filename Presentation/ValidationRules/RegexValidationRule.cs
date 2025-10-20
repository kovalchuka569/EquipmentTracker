using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Common.Constraints;
using Presentation.Helpers;

namespace Presentation.ValidationRules;

public partial class RegexValidationRule : ValidationRule
{
    // ==================== COMMON ERROR MESSAGES ==============
    
    /// <summary>
    /// Error message for minimum (example: "Minimum {0}.")
    /// </summary>
    public string MinimumErrorMessageFormat { get; init; } = string.Empty;
    
    /// <summary>
    /// Error message for maximum (example: "Maximum {0}.")
    /// </summary>
    public string MaximumErrorMessageFormat { get; init; } = string.Empty;
    
    
    
    // ==================== UPPERCASE ====================
    
    /// <summary>
    /// Require uppercase letters.
    /// </summary>
    public bool RequireUppercase { get; init; }
    
    /// <summary>
    /// Minimum number of capital letters
    /// </summary>
    public int MinUppercase { get; init; } = 1;

    /// <summary>
    /// Maximum number of capital letters (0 = no limit).
    /// </summary>
    public int MaxUppercase { get; init; }
    
    
    
    // ==================== LOWERCASE ====================
    
    /// <summary>
    /// Require lowercase letters.
    /// </summary>
    public bool RequireLowercase { get; init; }
    
    /// <summary>
    /// Minimum number of lowercase letters.
    /// </summary>
    public int MinLowercase { get; init; } = 1;
    
    /// <summary>
    /// Maximum number of lowercase letters (0 = no limit).
    /// </summary>
    public int MaxLowercase { get; init; }

    
    
    // ==================== DIGITS ====================
    
    /// <summary>
    /// Require the presence of digits.
    /// </summary>
    public bool RequireDigits { get; init; }
    
    /// <summary>
    /// Minimum number of digits.
    /// </summary>
    public int MinDigits { get; init; } = 1;
    
    /// <summary>
    /// Maximum number of digits letters (0 = no limit).
    /// </summary>
    public int MaxDigits { get; init; }
    
    
    
    // ==================== SPECIAL CHARACTERS ====================
    
    /// <summary>
    /// Require special characters.
    /// </summary>
    public bool RequireSpecialChars { get; init; }
    
    /// <summary>
    /// Minimum number of special characters.
    /// </summary>
    public int MinSpecialChars { get; init; } = 1;
    
    /// <summary>
    /// Maximum number of special characters (0 = no limit).
    /// </summary>
    public int MaxSpecialChars { get; init; }
    
    /// <summary>
    /// Allowed special characters (default !@#$%^&amp;*).
    /// </summary>
    public string AllowedSpecialChars { get; init; } = "!@#$%^&*";
    
    
    
    // ==================== WHITESPACE ====================
    
    /// <summary>
    /// Prohibit whitespaces and space characters.
    /// </summary>
    public bool ForbidWhitespace { get; init; }
    
    /// <summary>
    /// Error message for whitespaces.
    /// </summary>
    public string WhitespaceErrorMessage { get; init; } = string.Empty;
    
    
    
    // ==================== ALLOWED CHARACTERS ====================
    
    /// <summary>
    /// Check for allowed characters.
    /// </summary>
    public bool CheckAllowedChars { get; init; }
    
    /// <summary>
    /// Allow only letters (Latin + Cyrillic).
    /// </summary>
    public bool AllowLetters { get; init; } = true;
    
    /// <summary>
    /// Allow digits.
    /// </summary>
    public bool AllowDigits { get; init; } = true;
    
    /// <summary>
    /// Allow special characters from AllowedSpecialChars.
    /// </summary>
    public bool AllowSpecialChars { get; init; } = true;
    
    /// <summary>
    /// Allow white spaces.
    /// </summary>
    public bool AllowWhiteSpace { get; init; } = true;
    
    /// <summary>
    /// Additional permitted characters (added to the basic ones).
    /// </summary>
    public string AdditionalAllowedChars { get; init; } = string.Empty;
    
    /// <summary>
    /// Error message for invalid characters.
    /// </summary>
    public string AllowedCharsErrorMessage { get; init; } = string.Empty;
    
    
    
    // ==================== CUSTOM PATTERN ====================
    
    /// <summary>
    /// Custom regex pattern for additional verification.
    /// </summary>
    public string CustomPattern { get; init; } = string.Empty;
    
    /// <summary>
    /// Invert the result of CustomPattern (characters MUST NOT match).
    /// </summary>
    public bool InvertCustomPattern { get; init; }
    
    /// <summary>
    /// Error message for custom pattern.
    /// </summary>
    public string CustomPatternErrorMessage { get; init; } = string.Empty;
    
    
    
    // ==================== CONSECUTIVE CHARACTERS ====================
    
    /// <summary>
    /// Maximum number of consecutive identical symbols (0 = no limit).
    /// </summary>
    public int MaxConsecutiveChars { get; init; }
    
    
    
    // ==================== SEQUENTIAL CHARACTERS ====================
    
    /// <summary>
    /// Prohibit character sequences (abc, 123, абв, etc.).
    /// </summary>
    public bool ForbidSequentialChars { get; init; }
    
    /// <summary>
    /// Minimum sequence length for verification.
    /// </summary>
    public int SequentialCharsLength { get; init; } = 3;

    
    
    // ==================== LENGTH ====================
    
    /// <summary>
    /// Check string length.
    /// </summary>
    public bool CheckLength { get; init; }
    
    /// <summary>
    /// Minimum string length (0 = no restrictions).
    /// </summary>
    public int MinLength { get; init; }
    
    /// <summary>
    /// Maximum string length (0 = no limit).
    /// </summary>
    public int MaxLength { get; init; }
    
    /// <summary>
    /// Error message for minimum length.
    /// </summary>
    public string MinLengthErrorMessageFormat { get; init; } = string.Empty;
    
    /// <summary>
    /// Error message for maximum length.
    /// </summary>
    public string MaxLengthErrorMessageFormat { get; init; } = string.Empty;
    
    
    // ==================== EMPTY CHECK ====================
    
    /// <summary>
    /// Forbid empty values.
    /// </summary>
    public bool ForbidEmpty { get; init; }
    
    /// <summary>
    /// Error message for empty value.
    /// </summary>
    public string EmptyErrorMessage { get; init; } = string.Empty;
    
    
    [GeneratedRegex(CommonPatterns.Uppercase)]
    private static partial Regex UppercaseRegex();
    
    [GeneratedRegex(CommonPatterns.Lowercase)]
    private static partial Regex LowercaseRegex();
    
    [GeneratedRegex(CommonPatterns.Digits)]
    private static partial Regex DigitsRegex();
    
    [GeneratedRegex(@"\s")]
    private static partial Regex WhitespaceRegex();
    
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        var stringValue = value as string;
        
        // Check for empty value
        if (ForbidEmpty && string.IsNullOrEmpty(stringValue))
            return new ValidationResult(false, EmptyErrorMessage);
        
        if (string.IsNullOrEmpty(stringValue))
            return ValidationResult.ValidResult;
        
        // Length validation
        if (CheckLength)
        {
            if (MinLength > 0 && stringValue.Length < MinLength)
                return new ValidationResult(false, string.Format(MinLengthErrorMessageFormat, 
                    PluralizedHelper.GetSymbolPlural(MinLength)));
            
            if (MaxLength > 0 && stringValue.Length > MaxLength)
                return new ValidationResult(false, string.Format(MaxLengthErrorMessageFormat, 
                    PluralizedHelper.GetSymbolPlural(MaxLength)));
        }

        // Uppercase validation
        if (RequireUppercase)
        {
            var uppercaseMatches = UppercaseRegex().Matches(stringValue);
            var uppercaseCount = uppercaseMatches.Count;

            if (uppercaseCount < MinUppercase)
                return new ValidationResult(false, string.Format(MinimumErrorMessageFormat,
                    PluralizedHelper.GetUppercasePlural(MinUppercase)));

            if (MaxUppercase > 0 && uppercaseCount > MaxUppercase)
                return new ValidationResult(false, string.Format(MaximumErrorMessageFormat,
                    PluralizedHelper.GetUppercasePlural(MaxUppercase)));
        }
        
        // Lowercase validation
        if (RequireLowercase)
        {
            var lowercaseMatches = LowercaseRegex().Matches(stringValue);
            var lowercaseCount = lowercaseMatches.Count;

            if (lowercaseCount < MinLowercase)
                return new ValidationResult(false, string.Format(MinimumErrorMessageFormat, 
                    PluralizedHelper.GetLowercasePlural(MinLowercase)));

            if (MaxLowercase > 0 && lowercaseCount > MaxLowercase)
                return new ValidationResult(false, string.Format(MaximumErrorMessageFormat,
                    PluralizedHelper.GetLowercasePlural(MaxLowercase)));
        }

        // Digits validation
        if (RequireDigits)
        {
            var digitsMatches = DigitsRegex().Matches(stringValue);
            var digitsCount = digitsMatches.Count;

            if (digitsCount < MinDigits)
                return new ValidationResult(false, string.Format(MinimumErrorMessageFormat, 
                    PluralizedHelper.GetDigitPlural(MinDigits)));

            if (MaxDigits > 0 && digitsCount > MaxDigits)
                return new ValidationResult(false, string.Format(MaximumErrorMessageFormat,  
                    PluralizedHelper.GetDigitPlural(MaxDigits)));
        }
        
        // Special chars validation
        if (RequireSpecialChars && !string.IsNullOrEmpty(AllowedSpecialChars))
        {
            var escapedSpecial = Regex.Escape(AllowedSpecialChars);
            var specialPattern = $"[{escapedSpecial}]";
            var specialMatches = Regex.Matches(stringValue, specialPattern);
            var specialCount = specialMatches.Count;

            if (specialCount < MinSpecialChars)
                return new ValidationResult(false,
                    string.Format(MinimumErrorMessageFormat,
                        PluralizedHelper.GetSpecialCharsFormat(MinSpecialChars)));

            if (MaxSpecialChars > 0 && specialCount > MaxSpecialChars)
                return new ValidationResult(false,
                    string.Format(MaximumErrorMessageFormat,
                        PluralizedHelper.GetSpecialCharsFormat(MaxSpecialChars)));
        }

        // Whitespaces validation
        if (ForbidWhitespace)
        {
            if(WhitespaceRegex().IsMatch(stringValue))
                return new ValidationResult(false, WhitespaceErrorMessage);
        }

        // Allowed chars validation
        if (CheckAllowedChars)
        {
            var allowedPattern = BuildAllowedCharsPattern();
            if(!Regex.IsMatch(stringValue, allowedPattern))
                return new ValidationResult(false, AllowedCharsErrorMessage);
        }
        
        // Custom pattern validation
        if (!string.IsNullOrEmpty(CustomPattern))
        {
            var matches = Regex.IsMatch(stringValue, CustomPattern);
            
            if(InvertCustomPattern ? matches : !matches)
                return new ValidationResult(false, CustomPatternErrorMessage);
        }
        
        // Repeat chars validation
        if (MaxConsecutiveChars > 0)
        {
            var consecutivePattern = $@"(.)\1{{{MaxConsecutiveChars},}}";
            if(Regex.IsMatch(stringValue, consecutivePattern))
                return new ValidationResult(false, string.Format(MaximumErrorMessageFormat, 
                    PluralizedHelper.GetConsecutiveFormat(MaxConsecutiveChars)));
        }
        
        // Sequential validation
        if (ForbidSequentialChars)
        {
            if(ContainsSequentialChars(stringValue, SequentialCharsLength))
                return new ValidationResult(false, string.Format(MaximumErrorMessageFormat, 
                    PluralizedHelper.GetSequentialFormat(SequentialCharsLength)));
        }
        
        
        return ValidationResult.ValidResult;
    }

    /// <summary>
    /// Building a pattern of allowed characters.
    /// </summary>
    /// <returns>
    /// Pattern of allowed characters.
    /// </returns>
    private string BuildAllowedCharsPattern()
    {
        var parts = new List<string>();
        
        if(AllowLetters)
            parts.Add("a-zA-Zа-яА-ЯёЁіїєґІЇЄҐ");
        
        if(AllowDigits)
            parts.Add("0-9");
        
        if(AllowSpecialChars && !string.IsNullOrEmpty(AllowedSpecialChars))
            parts.Add(Regex.Escape(AllowedSpecialChars));
        
        if(AllowWhiteSpace)
            parts.Add(@"\s");
        
        if(!string.IsNullOrEmpty(AdditionalAllowedChars))
            parts.Add(Regex.Escape(AdditionalAllowedChars));
        
        return $"^[{string.Join("", parts)}]+$";
    }
    
    /// <summary>
    /// Check for consecutive characters (abc, 123, абв, etc.).
    /// </summary>
    /// <returns>
    /// The result of whether there are matches.
    /// </returns>
    private bool ContainsSequentialChars(string input, int minLength)
    {
        for (var i = 0; i <= input.Length - minLength; i++)
        {
            var isSequential = true;
            
            for (var j = 0; j < minLength - 1; j++)
            {
                if (input[i + j + 1] == input[i + j] + 1) continue;
                isSequential = false;
                break;
            }
            
            if (isSequential)
                return true;
        }
        
        return false;
    }
}