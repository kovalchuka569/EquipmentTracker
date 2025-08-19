﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Presentation.Converters;

public class InverseStringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isEmpty = string.IsNullOrEmpty(value as string);
        
        return isEmpty ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}