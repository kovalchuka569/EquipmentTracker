using System;
using Presentation.Contracts;
using Presentation.Services.Builders;
using Prism.Dialogs;

namespace Presentation.Services.Internal;

internal sealed class DialogConfiguration
{
    public required Type ViewModelType { get; init; }
    public required IDialogHost Host { get; init; }
    public IDialogParameters? Parameters { get; init; }
    public bool WithOverlay { get; init; }
    public Func<OverlayBuilder, OverlayBuilder>? OverlayBuilder { get; init; }
}