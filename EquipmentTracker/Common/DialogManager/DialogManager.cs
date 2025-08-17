using System.Windows;
using Core.Interfaces;
using EquipmentTracker.Common.Controls;
using EquipmentTracker.ViewModels.Dialogs;
using EquipmentTracker.Views.Dialogs;
using Core.Common.Enums;
using Core.Contracts;

namespace EquipmentTracker.Common.DialogManager;

public class DialogManager : IDialogManager
{
    private readonly IContainerProvider _container;
    private readonly Dictionary<DialogType, (Type ViewType, Type ViewModelType)> _dialogMappings = new();
    
    public DialogManager(IContainerProvider container)
    {
        _container = container;
        BuildDialogMappings();
    }
    
    private void BuildDialogMappings()
    {
        _dialogMappings[DialogType.ExcelImportConfigurator] = (typeof(ExcelImportConfiguratorView), typeof(ExcelImportConfiguratorViewModel));
        
        _dialogMappings[DialogType.DialogBox] = (typeof(DialogBoxView), typeof(DialogBoxViewModel));
    }
    
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

    public Task<IDialogResult> ShowDialogAsync(DialogType dialogType, IDialogHost dialogHost, IDialogParameters? parameters)
    {
        
        var viewModel = _container.Resolve(_dialogMappings[dialogType].ViewModelType);
        var view = _container.Resolve(_dialogMappings[dialogType].ViewType) as FrameworkElement;
        
        if (view == null)
            throw new InvalidOperationException($"Could not create view for {dialogType}");
        
        view.DataContext = viewModel;
        
        var tcs = new TaskCompletionSource<IDialogResult>();
        
        if (viewModel is IClosableDialog closableDialogViewModel)
        {
            var closeCommand = new DelegateCommand<IDialogResult>(result =>
            {
                dialogHost.IsDialogOpen = false;
                dialogHost.DialogContent = null;
                tcs.SetResult(result);
            });
            
            closableDialogViewModel.SetCloseCommand(closeCommand);

            if (viewModel is IDialogAware dialogAwareViewModel)
            {
                if (parameters is not null)
                {
                    dialogAwareViewModel.OnDialogOpened(parameters);
                }
            }
        }
        
        dialogHost.DialogContent = view;
        dialogHost.IsDialogOpen = true;

        return tcs.Task;
    }
}