using Presentation.Services.Builders;

namespace Presentation.Services.Interfaces;

public interface IBusyIndicatorService
{
    BusyIndicatorBuilder ShowBusyIndicator();
}