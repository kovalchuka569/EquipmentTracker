using System.Globalization;
using System.Windows.Data;

namespace EquipmentTracker.Views.Equipment.DataGrid;

public class StringToUriConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
        {
            // Проверяем, начинается ли строка с протокола, если нет — добавляем "https://"
            if (!stringValue.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !stringValue.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                stringValue = "https://" + stringValue;
            }
            return Uri.TryCreate(stringValue, UriKind.Absolute, out Uri uriResult) ? uriResult : null;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Uri uri ? uri.ToString() : string.Empty;
    }
}