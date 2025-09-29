using System.Globalization;
using System.Windows.Data;

namespace Common.Converters;

public class NpgsqlExceptionMessageToLocalizedStringConverter : IValueConverter
{
    public string? InvalidLoginOrPasswordMessage { get; set; }
    public string? InvalidDatabaseNameMessage { get; set; }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is not string exceptionMessage)
            return null;

        if(exceptionMessage.Contains("28P01"))
            return InvalidLoginOrPasswordMessage;
        
        if(exceptionMessage.Contains("3D000"))
            return InvalidDatabaseNameMessage;
        
        return exceptionMessage;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}