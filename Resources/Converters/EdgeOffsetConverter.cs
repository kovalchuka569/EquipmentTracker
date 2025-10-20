using System;
using System.Globalization;
using System.Windows.Data;

namespace Resources.Converters;

/// <summary>
/// Converts container size to offset for positioning elements from the edge.<br/>
/// Formula: containerSize - popupSize - margin<br/>
/// Use for positioning Popups or elements at the right/bottom edge with margin.<br/>
/// </summary>
public class EdgeOffsetConverter : IValueConverter
{
    /// <summary>
    /// Distance from the container edge (default: 12)
    /// </summary>
    public double Margin { get; set; } = 12;

    /// <summary>
    /// Size of the element being positioned (width or height, default: 200)
    /// </summary>
    public double Size { get; set; } = 200;
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double containerSize)
            return containerSize - Size - Margin;
        
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}