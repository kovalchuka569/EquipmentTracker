using Models.Common.Table;

namespace Models.Equipment.ColumnCreator;

public class ColumnEditingResult
{
    public bool IsSuccessful { get; set; }
    public ColumnModel EditedColumn { get; set; }
}