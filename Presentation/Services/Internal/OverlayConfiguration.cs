using Presentation.Contracts;
using Presentation.Services.Defaults;

namespace Presentation.Services.Internal;

internal sealed class OverlayConfiguration
{
    public required IOverlayHost Host { get; init; }
    public string Color { get; init; } = OverlayDefaults.DefaultColor;
    public double Opacity { get; init; } = OverlayDefaults.DefaultOpacity;
}