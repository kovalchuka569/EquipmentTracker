using System;
using System.Threading.Tasks;
using Presentation.Contracts;
using Presentation.Services.Defaults;
using Presentation.Services.Internal;

namespace Presentation.Services.Builders;

public sealed class OverlayBuilder
{
    private readonly OverlayService _service;
    private readonly IOverlayHost? _host;
    private readonly string _color;
    private readonly double _opacity;
    
    internal OverlayBuilder(OverlayService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _color = OverlayDefaults.DefaultColor;
        _opacity = OverlayDefaults.DefaultOpacity;
    }
    
    private OverlayBuilder(
        OverlayService service,
        IOverlayHost? host,
        string color,
        double opacity)
    {
        _service = service;
        _host = host;
        _color = color;
        _opacity = opacity;
    }
    
    /// <summary>
    /// Specifies the host where overlay will be displayed
    /// </summary>
    public OverlayBuilder InHost(IOverlayHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        return new OverlayBuilder(_service, host, _color, _opacity);
    }
    
    /// <summary>
    /// Sets overlay color (hex format)
    /// </summary>
    public OverlayBuilder WithColor(string color)
    {
        ArgumentNullException.ThrowIfNull(color);

        return new OverlayBuilder(_service, _host, color, _opacity);
    }
    
    /// <summary>
    /// Sets overlay opacity (0.0 to 1.0)
    /// </summary>
    public OverlayBuilder WithOpacity(double opacity)
    {
        if (opacity is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(opacity), @"Opacity must be between 0 and 1");

        return new OverlayBuilder(_service, _host, _color, opacity);
    }
    
    /// <summary>
    /// Executes action with overlay and returns result
    /// </summary>
    public Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        if (_host == null)
            throw new InvalidOperationException("Host must be specified using InHost() method");

        var config = new OverlayConfiguration
        {
            Host = _host,
            Color = _color,
            Opacity = _opacity
        };

        return _service.ExecuteAsync(config, action);
    }

    /// <summary>
    /// Executes action with overlay (without return value)
    /// </summary>
    public Task ExecuteAsync(Func<Task> action)
    {
        return ExecuteAsync(async () =>
        {
            await action();
            return (object)null!;
        });
    }
}