namespace Common.Constraints;

public static class UserConstraints
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 50;
    public const int LoginMinLength = 3;
    public const int LoginMaxLength = 30;
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 64;
    public const int PasswordHashLength = 150;
    public const string PassAllowedSpecialChars = "!@#$%^&*";
    public const int PassMinUppercases = 1;
    public const int PassMinLowercases = 1;
    public const int PassMinDigits = 1;
    public const int PassMinSpecialChars = 1;
    public const int PassMaxConsecutiveChars = 2;
    public const int SequentialCharsLength = 3;
}