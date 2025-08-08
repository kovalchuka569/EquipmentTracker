namespace Models.Common.Table.ColumnSpecificSettings;

public class ComboBoxColumnSpecificSettings : IColumnSpecificSettings
{
    public List<string> ListValues { get; set; } = new();
}