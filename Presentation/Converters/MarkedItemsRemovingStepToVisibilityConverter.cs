using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Common.Enums;

namespace Presentation.Converters;

public class MarkedItemsRemovingStepToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is not MarkedItemsRemovingStep step)
            return Visibility.Visible;

        return step switch
        {
            MarkedItemsRemovingStep.RemovingProcess => Visibility.Collapsed,
            MarkedItemsRemovingStep.RemovingTypeSelection or MarkedItemsRemovingStep.RemovingItemsSelection
                or MarkedItemsRemovingStep.RemovingFinish => Visibility.Visible,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => throw new NotImplementedException();
}