using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace EquipmentTracker.ViewModels.DataGrid;

public class SearchHighlightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        string text = values[0] as string;
        string search = values[1] as string;

        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(search))
            return new TextBlock { Text = text };

        var textBlock = new TextBlock();

        int index = text.IndexOf(search, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            textBlock.Inlines.Add(new Run(text.Substring(0, index)));
            textBlock.Inlines.Add(new Run(text.Substring(index, search.Length)) { Background = Brushes.Orange });
            textBlock.Inlines.Add(new Run(text.Substring(index + search.Length)));
        }
        else
        {
            textBlock.Text = text;
        }

        return textBlock;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}