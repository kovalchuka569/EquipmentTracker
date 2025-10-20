using Presentation.Services.Builders;

namespace Presentation.Services.Interfaces;

public interface IOverlayService
{
    OverlayBuilder Configure();
}