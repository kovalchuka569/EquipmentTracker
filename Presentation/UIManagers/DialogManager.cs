using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

using Prism.Commands;
using Prism.Dialogs;
using Prism.Ioc;

using Core.Interfaces;

using Presentation.Enums;
using Presentation.Interfaces;
using Presentation.ViewModels;
using Presentation.Views;

namespace Presentation.UIManagers;

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