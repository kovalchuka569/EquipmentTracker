using System.Windows.Controls;
using System.Windows.Shapes;

namespace EquipmentTracker.Common.DialogService;

public interface IDialogService
{
    Task<bool> ShowDeleteConfirmationAsync(string title, string message);
    Task<bool> ShowInformationAgreementAsync(string title, string message, string confirmButtonText, string cancelButtonText);
}