using System;
using System.Globalization;
using System.Windows.Data;

using Common.Enums;

namespace Presentation.Converters;

public class HeaderToNavDrawerMenuTypeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            "Виробниче обладнання" => MenuType.Prod,
            "Інструменти" => MenuType.Tools,
            "Меблі" => MenuType.Furniture,
            "Офісна техніка" => MenuType.Office,
            "Автопарк" => MenuType.Cars,
            "Розхідні матеріали" => MenuType.Consumables,
            "Історія" => MenuType.History,
            "Календар" => MenuType.Scheduler,
            "Налаштування" => MenuType.Settings,
            _ => Binding.DoNothing
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}