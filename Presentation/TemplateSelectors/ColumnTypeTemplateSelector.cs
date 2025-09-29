using System.Windows;
using System.Windows.Controls;
using Models.Equipment;

namespace Presentation.TemplateSelectors;

public class ColumnTypeTemplateSelector : DataTemplateSelector
{
    public DataTemplate TextTemplate { get; set; }
    public DataTemplate NumberTemplate { get; set; }
    public DataTemplate DateTemplate { get; set; }
    public DataTemplate ListTemplate { get; set; }
    public DataTemplate CurrencyTemplate { get; set; }
    public DataTemplate HyperlinkTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is ColumnDataType columnDataType)
        {
            switch (columnDataType)
            {
                case ColumnDataType.Text:
                    return TextTemplate;
                case ColumnDataType.Number:
                    return NumberTemplate;
                case ColumnDataType.Date:
                    return DateTemplate;
                case ColumnDataType.List:
                    return ListTemplate;
                case ColumnDataType.Currency:
                    return CurrencyTemplate;
                case ColumnDataType.Hyperlink:
                    return HyperlinkTemplate;
            }
        }
        return base.SelectTemplate(item, container);
    }
}