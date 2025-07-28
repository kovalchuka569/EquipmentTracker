using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Models.Equipment.ColumnSpecificSettings;

namespace Models.Table;

[NotMapped]
[JsonDerivedType(typeof(TextColumnSettings), "text")]
[JsonDerivedType(typeof(BooleanColumnSettings), "boolean")]
[JsonDerivedType(typeof(CurrencyColumnSettings), "currency")]
[JsonDerivedType(typeof(DateColumnSettings), "date")]
[JsonDerivedType(typeof(ListColumnSettings), "list")]
[JsonDerivedType(typeof(NumberColumnSettings), "number")]
public abstract class ColumnSpecificSettingsBase { }