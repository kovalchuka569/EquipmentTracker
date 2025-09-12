using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Common.Enums;

namespace Presentation.Converters;

public class MarkedItemsRemovingStepToStyleConverter : IMultiValueConverter
{
    
    public Style NextStyle { get; set; } = null!;
    public Style FinishStyle { get; set; } = null!;
    
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is MarkedItemsRemovingStep step)
        {
            return step switch
            {
                MarkedItemsRemovingStep.RemovingFinish => FinishStyle,
                _ => NextStyle
            };
        }
        return NextStyle;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}