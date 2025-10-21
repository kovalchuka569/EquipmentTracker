using Presentation.Services.Builders;

namespace Presentation.Services.Interfaces;

public interface ISnackbarService
{
    SnackBuilder Show();
}