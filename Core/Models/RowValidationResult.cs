namespace Core.Models;

public class RowValidationResult
{
    public bool IsValid { get; set; } = true;
    
    public Dictionary<string, string> ErrorMessages { get; } = new();
}