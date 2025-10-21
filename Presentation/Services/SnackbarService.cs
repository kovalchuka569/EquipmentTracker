using Presentation.EventArgs;
using Presentation.Events;
using Presentation.Models;
using Presentation.Services.Builders;
using Presentation.Services.Interfaces;
using Prism.Events;
using Unity;

namespace Presentation.Services;

public class SnackbarService : ISnackbarService
{
    [Dependency] public required IEventAggregator EventAggregator = null!;

    public SnackBuilder Show() => new(this);

    internal void ResolveSnack(Snack snack)
    {
        var showSnackEventArgs = new ShowSnackEventArgs
        {
            Snack = snack
        };
        
        EventAggregator.GetEvent<ShowSnackEvent>().Publish(showSnackEventArgs);
    }
}