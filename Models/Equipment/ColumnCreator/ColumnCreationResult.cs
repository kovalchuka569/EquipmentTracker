using Models.Equipment.ColumnSettings;

namespace Models.Equipment.ColumnCreator;

public class ColumnCreationResult
{
    public bool IsSuccessful { get; set; }
    public ColumnSettingsDisplayModel ColumnSettings { get; set; }
}