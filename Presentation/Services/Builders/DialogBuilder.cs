using System;
using System.Threading;
using System.Threading.Tasks;
using Presentation.Contracts;
using Presentation.Services.Internal;
using Prism.Dialogs;

namespace Presentation.Services.Builders;

public sealed class DialogBuilder
{
    private readonly DialogService _service;
    private readonly Type _dialogViewModelType;
    private readonly IDialogHost? _dialogHost;
    private readonly IDialogParameters? _dialogParameters;
    private readonly bool _withOverlay;
    private readonly Func<OverlayBuilder, OverlayBuilder>? _overlayBuilder;
    
    internal DialogBuilder(DialogService service, Type dialogViewModelType)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _dialogViewModelType = dialogViewModelType ?? throw new ArgumentNullException(nameof(dialogViewModelType));
    }
    
    private DialogBuilder(
        DialogService service,
        Type dialogViewModelType,
        IDialogHost? dialogHost,
        IDialogParameters? dialogParameters,
        bool withOverlay,
        Func<OverlayBuilder, OverlayBuilder>? overlayBuilder)
    {
        _service = service;
        _dialogViewModelType = dialogViewModelType;
        _dialogHost = dialogHost;
        _dialogParameters = dialogParameters;
        _withOverlay = withOverlay;
        _overlayBuilder = overlayBuilder;
    }
    
    public DialogBuilder In(IDialogHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        return new DialogBuilder(
            _service,
            _dialogViewModelType,
            host,
            _dialogParameters,
            _withOverlay,
            _overlayBuilder
        );
    }
    
    public DialogBuilder WithParameters(IDialogParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        return new DialogBuilder(
            _service,
            _dialogViewModelType,
            _dialogHost,
            parameters,
            _withOverlay,
            _overlayBuilder
        );
    }
    
    public DialogBuilder WithOverlay(Func<OverlayBuilder, OverlayBuilder>? overlayBuilder = null)
    {
        return new DialogBuilder(
            _service,
            _dialogViewModelType,
            _dialogHost,
            _dialogParameters,
            true,
            overlayBuilder
        );
    }

    public Task<IDialogResult> Await(CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(_dialogHost);
        
        var config = new DialogConfiguration
        {
            ViewModelType = _dialogViewModelType,
            Host = _dialogHost,
            Parameters = _dialogParameters,
            WithOverlay = _withOverlay,
            OverlayBuilder = _overlayBuilder
        };
        
        return _service.ResolveDialogAsync(config, ct);
    }
}