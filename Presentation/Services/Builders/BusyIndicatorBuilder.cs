using System;
using System.Threading;
using System.Threading.Tasks;
using Presentation.Contracts;
using Presentation.Services.Defaults;
using Presentation.Services.Internal;
using Syncfusion.Windows.Controls.Notification;

namespace Presentation.Services.Builders;

public sealed class BusyIndicatorBuilder
{
    private readonly BusyIndicatorService _service;
    private readonly IBusyIndicatorHost? _host;
    private readonly string? _header;
    private readonly AnimationTypes _animationType;
    private readonly string _color;
    private readonly double _delay;
    private readonly bool _withOverlay;
    private readonly Func<OverlayBuilder, OverlayBuilder>? _overlayBuilder;

    internal BusyIndicatorBuilder(BusyIndicatorService service)
    {
        _service = service;
        _animationType = BusyIndicatorDefaults.DefaultAnimationType;
        _color = BusyIndicatorDefaults.DefaultColor;
        _delay = BusyIndicatorDefaults.DefaultDelay;
    }

    private BusyIndicatorBuilder(
        BusyIndicatorService service,
        IBusyIndicatorHost? host,
        string? header,
        AnimationTypes animationType,
        string color,
        double delay,
        bool withOverlay,
        Func<OverlayBuilder, OverlayBuilder>? overlayBuilder
    )
    {
        _service = service;
        _host = host;
        _header = header;
        _animationType = animationType;
        _color = color;
        _delay = delay;
        _withOverlay = withOverlay;
        _overlayBuilder = overlayBuilder;
    }

    public BusyIndicatorBuilder In(IBusyIndicatorHost? host)
    {
        ArgumentNullException.ThrowIfNull(host);

        return new BusyIndicatorBuilder(
            _service,
            host,
            _header,
            _animationType,
            _color,
            _delay,
            _withOverlay,
            _overlayBuilder
        );
    }
    
    public BusyIndicatorBuilder WithHeader(string? header)
    {
        return new BusyIndicatorBuilder(
            _service,
            _host,
            header,
            _animationType,
            _color,
            _delay,
            _withOverlay,
            _overlayBuilder
        );
    }
    
    public BusyIndicatorBuilder WithAnimation(AnimationTypes animationType)
    {
        return new BusyIndicatorBuilder(
            _service,
            _host,
            _header,
            animationType,
            _color,
            _delay,
            _withOverlay,
            _overlayBuilder
        );
    }
    
    public BusyIndicatorBuilder WithColor(string color)
    {
        return new BusyIndicatorBuilder(
            _service,
            _host,
            _header,
            _animationType,
            color,
            _delay,
            _withOverlay,
            _overlayBuilder
        );
    }
    
    public BusyIndicatorBuilder WithDelay(double delay)
    {
        return new BusyIndicatorBuilder(
            _service,
            _host,
            _header,
            _animationType,
            _color,
            delay,
            _withOverlay,
            _overlayBuilder
        );
    }
    
    public BusyIndicatorBuilder WithOverlay(Func<OverlayBuilder, OverlayBuilder>? overlayBuilder = null)
    {
        return new BusyIndicatorBuilder(
            _service,
            _host,
            _header,
            _animationType,
            _color,
            _delay,
            true,
            overlayBuilder
        );
    }

    public Task Await(CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(_host);

        var config = new BusyIndicatorConfiguration
        {
            Host = _host,
            AnimationType = _animationType,
            Color = _color,
            Delay = _delay,
            Header = _header,
            WithOverlay = _withOverlay,
            OverlayBuilder = _overlayBuilder
        };

        return _service.ResolveBusyIndicatorAsync(config);
    }
    
    /// <summary>
    /// Executes action with busy indicator and returns result
    /// </summary>
    public Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        if (_host == null)
            throw new InvalidOperationException("Host must be specified using InHost() method");

        var config = new BusyIndicatorConfiguration
        {
            Host = _host,
            Header = _header,
            Color = _color,
            AnimationType = _animationType,
            Delay = _delay,
            WithOverlay = _withOverlay,
            OverlayBuilder = _overlayBuilder
        };

        return _service.ExecuteAsync(config, action);
    }
    
    public Task ExecuteAsync(Func<Task> action)
    {
        return ExecuteAsync(async () =>
        {
            await action();
            return (object)null!;
        });
    }
}