namespace Core.Common.EquipmentSheetValidation.RowValidator;

public class RowValidationResult
{
    public bool IsValid { get; set; } = true;
    public Dictionary<string, string> ErrorMessages { get; } = new();
}