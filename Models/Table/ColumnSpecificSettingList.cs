namespace Models.Table;

public class ColumnSpecificSettingList : ColumnSpecificSettingsBase
{
    public List<string> ListValues { get; set; }
    public string DefaultValue { get; set; }
}