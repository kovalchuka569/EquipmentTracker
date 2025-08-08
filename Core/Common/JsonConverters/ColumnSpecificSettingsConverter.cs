using Models.Common.Table.ColumnSpecificSettings;
using Newtonsoft.Json.Linq;

namespace Core.Common.JsonConverters;

public class ColumnSpecificSettingsConverter : JsonCreationConverter<IColumnSpecificSettings>
{
    protected override IColumnSpecificSettings Create(Type objectType, JObject jsonObject)
    {
        if (jsonObject.ContainsKey("DefaultValue"))
        {
            return new CheckBoxColumnSpecificSettings();
        }
        if (jsonObject.ContainsKey("ListValues"))
        {
            return new ComboBoxColumnSpecificSettings();
        }
        if (jsonObject.ContainsKey("CurrencySymbol") || jsonObject.ContainsKey("CurrencyPosition"))
        {
            return new CurrencyColumnSpecificSettings();
        }
        if (jsonObject.ContainsKey("DateFormat"))
        {
            return new DateColumnSpecificSettings();
        }
        if (jsonObject.ContainsKey("NumberDecimalDigits"))
        {
            return new NumericColumnSpecificSettings();
        }

        return new DefaultColumnSpecificSettings();
    }
}