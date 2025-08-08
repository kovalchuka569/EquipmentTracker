using Models.Common.Table.ColumnValidationRules;
using Newtonsoft.Json.Linq;

namespace Core.Common.JsonConverters;

public class ColumnValidationRulesConverter : JsonCreationConverter<IColumnValidationRules>
{
    protected override IColumnValidationRules Create(Type objectType, JObject jsonObject)
    {
        if (jsonObject.ContainsKey("MinValue") || jsonObject.ContainsKey("MaxValue"))
        {
            return new NumericColumnValidationRules();
        }
        if (jsonObject.ContainsKey("MinLength") || jsonObject.ContainsKey("MaxLength"))
        {
            return new TextColumnValidationRules();
        }
        return new DefaultColumnValidationRules();
    }
}