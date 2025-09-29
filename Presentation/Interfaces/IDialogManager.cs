using System.Threading.Tasks;

using Prism.Dialogs;

using Presentation.Contracts;

using Presentation.Enums;

namespace Presentation.Interfaces;

public interface IDialogManager
{
    Task<IDialogResult> ShowDialogAsync(DialogType dialogType, IDialogHost dialogHost, IDialogParameters? parameters = null);
}