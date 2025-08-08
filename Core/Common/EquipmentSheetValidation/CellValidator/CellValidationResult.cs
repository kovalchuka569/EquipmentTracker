namespace Core.Common.EquipmentSheetValidation.CellValidator;

public class CellValidationResult
{
    public bool IsValid { get; set; } = true;
    public string? ErrorMessage { get; set; }
}