namespace Presentation.Enums;

public class EnumDisplay<T>(T value, string display)
{
    public T Enum { get; set; } = value;

    public string Display { get; set; } = display;

    public override string ToString() => Display;
}