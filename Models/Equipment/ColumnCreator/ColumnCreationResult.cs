using Models.Common.Table;
using Models.Equipment.ColumnSettings;

namespace Models.Equipment.ColumnCreator;

public class ColumnCreationResult
{
    public bool IsSuccessful { get; set; }
    public ColumnModel ColumnModel { get; set; }
}