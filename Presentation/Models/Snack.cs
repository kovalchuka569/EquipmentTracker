using Common.Enums;

namespace Presentation.Models;

public class Snack
{
    public required string Message { get; init; }
    public SnackType Type { get; init; }
    public int ShowTime { get; init; }
}