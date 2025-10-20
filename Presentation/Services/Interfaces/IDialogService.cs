using Presentation.Services.Builders;
using Presentation.ViewModels.Common;

namespace Presentation.Services.Interfaces;

public interface IDialogService
{
    DialogBuilder Show<TViewModel>() where TViewModel : ViewModelBase;
}