using System;
using System.Windows.Data;
using System.Windows.Markup;
using Presentation.ValidationRules;
using Resources.Localization;
using Binding = System.Windows.Data.Binding;

namespace Presentation.MarkupExtensions;

public class FieldValidationExtension : MarkupExtension
{
    public string Path { get; set; } = string.Empty;
    
    // ==================== COMMON ERROR MESSAGES =============

    public string MinimumErrorMessageFormat { get; set; } = ValidationUIMessages.Minimum_Format;
    public string MaximumErrorMessageFormat { get; set; } = ValidationUIMessages.Maximum_Format;
    
    
    
    // ==================== EMPTY & LENGTH ====================
    
    public bool ForbidEmpty { get; set; }
    public string EmptyErrorMessage { get; set; } = ValidationUIMessages.FieldCannotBeEmpty;
    public bool CheckLength { get; set; }
    public int MinLength { get; set; }
    public int MaxLength { get; set; }
    public string MinLengthErrorMessageFormat { get; set; } = ValidationUIMessages.MinLength_Format;
    public string MaxLengthErrorMessageFormat { get; set; } = ValidationUIMessages.MaxLength_Format;
    
    
    // ==================== UPPERCASE ====================
    
    public bool RequireUppercase { get; set; }
    public int MinUppercase { get; set; } = 1;
    public int MaxUppercase { get; set; }
    
    
    // ==================== LOWERCASE ====================
    
    public bool RequireLowercase { get; set; }
    public int MinLowercase { get; set; } = 1;
    public int MaxLowercase { get; set; }
    
    
    // ==================== DIGITS ====================
    
    public bool RequireDigits { get; set; }
    public int MinDigits { get; set; } = 1;
    public int MaxDigits { get; set; } = 0;
    
    
    // ==================== SPECIAL CHARACTERS ====================

    public bool RequireSpecialChars { get; set; }
    public int MinSpecialChars { get; set; } = 1;
    public int MaxSpecialChars { get; set; } = 0;
    public string AllowedSpecialChars { get; set; } = "!@#$%^&*";
    
    
    // ==================== WHITESPACE ====================
    
    public bool ForbidWhitespace { get; set; }
    public string WhitespaceErrorMessage { get; set; } = ValidationUIMessages.FieldCannotContaintSpaces;
    
    
    // ==================== ALLOWED CHARACTERS ====================
    
    public bool CheckAllowedChars { get; set; }
    public bool AllowLetters { get; set; } = true;
    public bool AllowDigits { get; set; } = true;
    public bool AllowSpecialChars { get; set; } = true;
    public bool AllowWhitespace { get; set; }
    public string AdditionalAllowedChars { get; set; } = string.Empty;
    public string AllowedCharsErrorMessage { get; set; } = ValidationUIMessages.FiledHaveProhibitedChars;
    
    
    // ==================== CUSTOM PATTERN ====================
    
    public string CustomPattern { get; set; } = string.Empty;
    public bool InvertCustomPattern { get; set; }
    public string CustomPatternErrorMessage { get; set; } = ValidationUIMessages.FieldDoesNotMatchFormat;
    
    
    // ==================== CONSECUTIVE & SEQUENTIAL ====================
    
    public int MaxConsecutiveChars { get; set; }
    public bool ForbidSequentialChars { get; set; }
    public int SequentialCharsLength { get; set; } = 3;
    
    
    
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding(Path)
        {
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            NotifyOnValidationError = true,
            ValidatesOnDataErrors = true,
            ValidatesOnNotifyDataErrors = true
        };
        
        // Создаем и настраиваем ValidationRule
        var validationRule = new RegexValidationRule
        {
            
            // Common messages
            MinimumErrorMessageFormat = MinimumErrorMessageFormat,
            MaximumErrorMessageFormat = MaximumErrorMessageFormat,
            
            // Empty & Length
            ForbidEmpty = ForbidEmpty,
            EmptyErrorMessage = EmptyErrorMessage,
            CheckLength = CheckLength,
            MinLength = MinLength,
            MaxLength = MaxLength,
            MinLengthErrorMessageFormat = MinLengthErrorMessageFormat,
            MaxLengthErrorMessageFormat = MaxLengthErrorMessageFormat,
            
            // Uppercase
            RequireUppercase = RequireUppercase,
            MinUppercase = MinUppercase,
            MaxUppercase = MaxUppercase,
            
            // Lowercase
            RequireLowercase = RequireLowercase,
            MinLowercase = MinLowercase,
            MaxLowercase = MaxLowercase,
            
            // Digits
            RequireDigits = RequireDigits,
            MinDigits = MinDigits,
            MaxDigits = MaxDigits,
            
            // Special Characters
            RequireSpecialChars = RequireSpecialChars,
            MinSpecialChars = MinSpecialChars,
            MaxSpecialChars = MaxSpecialChars,
            AllowedSpecialChars = AllowedSpecialChars,
            
            // Whitespace
            ForbidWhitespace = ForbidWhitespace,
            WhitespaceErrorMessage = WhitespaceErrorMessage,
            
            // Allowed Characters
            CheckAllowedChars = CheckAllowedChars,
            AllowLetters = AllowLetters,
            AllowDigits = AllowDigits,
            AllowSpecialChars = AllowSpecialChars,
            AllowWhiteSpace = AllowWhitespace,
            AdditionalAllowedChars = AdditionalAllowedChars,
            AllowedCharsErrorMessage = AllowedCharsErrorMessage,
            
            // Custom Pattern
            CustomPattern = CustomPattern,
            InvertCustomPattern = InvertCustomPattern,
            CustomPatternErrorMessage = CustomPatternErrorMessage,
            
            // Consecutive & Sequential
            MaxConsecutiveChars = MaxConsecutiveChars,
            ForbidSequentialChars = ForbidSequentialChars,
            SequentialCharsLength = SequentialCharsLength,
        };
        
        binding.ValidationRules.Add(validationRule);

        return binding.ProvideValue(serviceProvider);
    }
}