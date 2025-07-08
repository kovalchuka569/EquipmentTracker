using System.Globalization;
using System.Windows.Data;

namespace EquipmentTracker.Views.Equipment.DataGrid.Converters;

public class NumberFormatConverter : IValueConverter
{
    private static readonly CultureInfo culture = new CultureInfo("uk-UA") 
        { NumberFormat = { NumberGroupSeparator = " " } };

    public object Convert(object value, Type targetType, object parameter, CultureInfo cultureParam)
    {
        if (value == null) return "";
        if (value is IFormattable formattable)
            return formattable.ToString("N2", culture);
        return value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureParam)
    {
        throw new NotImplementedException();
    }
}