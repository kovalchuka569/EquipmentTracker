using System.Collections.ObjectModel;
using Models.Table;

namespace Models.Equipment.ColumnSpecificSettings;

public class ListColumnSettings : ColumnSpecificSettingsBase
{
    public List<string> ListValues { get; set; }
    public string DefaultValue { get; set; }
}