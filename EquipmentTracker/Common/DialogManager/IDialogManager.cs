using Core.Common.Enums;
using Core.Contracts;

namespace EquipmentTracker.Common.DialogManager;

public interface IDialogManager
{
    Task<bool> ShowDeleteConfirmationAsync(string title, string message);

    Task<bool> ShowInformationAgreementAsync(string title, string message, string confirmButtonText, string cancelButtonText);

    Task<IDialogResult> ShowDialogAsync(DialogType dialogType, IDialogHost dialogHost, IDialogParameters? parameters = null);
}