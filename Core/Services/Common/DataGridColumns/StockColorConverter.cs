using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.Windows.Data;
using Syncfusion.UI.Xaml.Grid;

namespace Core.Services.Common.DataGridColumns
{
    public class StockColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not decimal currentValue)
                return Brushes.Gray;
            
            var binding = parameter as BindingExpression;
            if (binding?.DataItem is not ExpandoObject expando)
                return Brushes.Gray;
            
            var dict = (IDictionary<string, object>)expando;
            if (!dict.TryGetValue("Мінімальний залишок", out var minLevelObj) || 
                !dict.TryGetValue("Максимальний залишок", out var maxLevelObj))
                return Brushes.Gray;

            if (!decimal.TryParse(minLevelObj?.ToString(), out var minLevel) ||
                !decimal.TryParse(maxLevelObj?.ToString(), out var maxLevel))
                return Brushes.Gray;

            if (maxLevel <= minLevel)
                return Brushes.Gray;
            
            decimal percentage = (currentValue - minLevel) / (maxLevel - minLevel) * 100;
            
            if (percentage > 60)
                return Brushes.Green;
            else if (percentage < 15)
                return Brushes.Red;
            else if (percentage < 45)
                return Brushes.Yellow;
            return Brushes.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
