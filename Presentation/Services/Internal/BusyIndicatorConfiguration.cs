using System;
using Presentation.Contracts;
using Presentation.Services.Builders;
using Presentation.Services.Defaults;
using Syncfusion.Windows.Controls.Notification;

namespace Presentation.Services.Internal;

internal sealed class BusyIndicatorConfiguration
{
    public required IBusyIndicatorHost Host { get; init; }
    public string? Header { get; init; }
    public AnimationTypes AnimationType { get; init; } = BusyIndicatorDefaults.DefaultAnimationType;
    public string Color { get; init; } = BusyIndicatorDefaults.DefaultColor;
    public double Delay { get; init; } = BusyIndicatorDefaults.DefaultDelay;
    public bool WithOverlay { get; init; } 
    public Func<OverlayBuilder, OverlayBuilder>? OverlayBuilder { get; init; }
}