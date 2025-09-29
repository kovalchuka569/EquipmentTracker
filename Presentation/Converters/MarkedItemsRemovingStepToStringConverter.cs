using System;
using System.Globalization;
using System.Windows.Data;
using Common.Enums;

namespace Presentation.Converters;

public class MarkedItemsRemovingStepToStringConverter : IValueConverter
{
    
    public string RemovingTypeSelectionString { get; set; } = string.Empty;
    public string RemovingItemsSelectionString { get; set; } = string.Empty;
    public string RemovingProcessString { get; set; } = string.Empty;
    public string RemovingFinishString { get; set; } = string.Empty;
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is not MarkedItemsRemovingStep step)
            return null;

        return step switch
        {
            MarkedItemsRemovingStep.RemovingTypeSelection => RemovingTypeSelectionString,
            MarkedItemsRemovingStep.RemovingItemsSelection => RemovingItemsSelectionString,
            MarkedItemsRemovingStep.RemovingProcess => RemovingProcessString,
            MarkedItemsRemovingStep.RemovingFinish => RemovingFinishString,
            _ => (object?)null
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}