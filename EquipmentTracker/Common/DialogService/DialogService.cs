using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using EquipmentTracker.Common.Controls;

namespace EquipmentTracker.Common.DialogService;

public class DialogService : IDialogService
{
    public async Task<bool> ShowDeleteConfirmationAsync(string title, string message)
    {
        var tcs = new TaskCompletionSource<bool>();
        
        var dialog = new RemovalAgreement
        {
            DeleteTitle = title,
            DeleteMessage = message
        };
        
        dialog.DeleteCommand = new DelegateCommand(() => 
        {
            tcs.SetResult(true);
            dialog.Close();
        });
        
        dialog.CancelCommand = new DelegateCommand(() => 
        {
            tcs.SetResult(false);
            dialog.Close();
        });

        dialog.ShowDialog();
        
        return await tcs.Task;
    }

    public async Task<bool> ShowInformationAgreementAsync(string title, string message, string confirmButtonText, string cancelButtonText)
    {
        var tcs = new TaskCompletionSource<bool>();

        var dialog = new InformationAgreement
        {
            InformationTitle = title,
            InformationMessage = message,
            ConfirmButtonText = confirmButtonText,
            CancelButtonText = cancelButtonText,
            CancelButtonVisibility = string.IsNullOrEmpty(cancelButtonText) ? Visibility.Collapsed : Visibility.Visible

        };
        
        dialog.ConfirmCommand = new DelegateCommand(() =>
        {
            tcs.SetResult(true);
            dialog.Close();
        });

        dialog.CancelCommand = new DelegateCommand(() =>
        {
            tcs.SetResult(false);
            dialog.Close();
        });
        
        dialog.ShowDialog();
        
        return await tcs.Task;
    }
}