using Common.Enums;
using Presentation.Models;
using Presentation.Services.Defaults;

namespace Presentation.Services.Builders;

public class SnackBuilder
{
    private readonly SnackbarService _service;
    private string _message;
    private SnackType _type;
    private int _showTime;

    internal SnackBuilder(SnackbarService service)
    {
        _service = service;
        _message = SnackDefaults.DefaultMessage;
        _type = SnackDefaults.DefaultType;
        _showTime = SnackDefaults.DefaultShowTime;
    }

    public SnackBuilder WithMessage(string message)
    {
        _message = message;
        return this;
    }

    public SnackBuilder OfType(SnackType type)
    {
        _type = type;
        return this;
    }

    public SnackBuilder During(int milliseconds)
    {
        _showTime = milliseconds;
        return this;
    }
    
    public void Now()
    {
        var snack = new Snack
        {
            Message = _message,
            Type = _type,
            ShowTime = _showTime
        };
        _service.ResolveSnack(snack);
    }
}