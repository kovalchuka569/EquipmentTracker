using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Presentation.Contracts;
using Presentation.Services.Builders;
using Presentation.Services.Interfaces;
using Presentation.Services.Internal;
using Presentation.ViewModels.Common;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Ioc;
using Unity;
using IDialogService = Presentation.Services.Interfaces.IDialogService;

namespace Presentation.Services;

public class DialogService : IDialogService
{
    private const string FailedToResolveViewExceptionMessage = "Failed to resolve view : {0}";
    private const string ViewForViewModelNotFoundExceptionMessage = "View for {0} not found";

    [Dependency] public required IOverlayService OverlayService { get; init; } = null!;

    [Dependency] public required IContainerProvider ContainerProvider { get; init; } = null!;
    
    public DialogBuilder Show<TViewModel>() where TViewModel : ViewModelBase => new(this, typeof(TViewModel));

    internal async Task<IDialogResult> ResolveDialogAsync(DialogConfiguration config, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(config.Host);

        if (config is { WithOverlay: true, Host: IOverlayHost overlayHost })
            return await ShowDialogWithOverlayAsync(config, overlayHost, ct);

        return await ShowDialogInternalAsync(config, ct);
    }

    private async Task<IDialogResult> ShowDialogWithOverlayAsync(
        DialogConfiguration config,
        IOverlayHost overlayHost,
        CancellationToken ct)
    {
        var overlayBuilder = OverlayService
            .Configure()
            .InHost(overlayHost);
        
        if (config.OverlayBuilder != null)
        {
            overlayBuilder = config.OverlayBuilder(overlayBuilder);
        }

        return await overlayBuilder.ExecuteAsync(
            async () => await ShowDialogInternalAsync(config, ct)
        );
    }

    private async Task<IDialogResult> ShowDialogInternalAsync(
        DialogConfiguration config,
        CancellationToken ct)
    {
        var viewType = FindViewType(config.ViewModelType);

        if (ContainerProvider.Resolve(viewType) is not FrameworkElement view)
            throw new InvalidOperationException(
                string.Format(FailedToResolveViewExceptionMessage, viewType.Name)
            );

        var viewModel = ContainerProvider.Resolve(config.ViewModelType);
        view.DataContext = viewModel;

        var tcs = new TaskCompletionSource<IDialogResult>();
        
        using var ctr = ct.Register(() =>
        {
            CleanupDialog(config.Host);
            tcs.TrySetCanceled(ct);
        });

        try
        {
            SetupDialogViewModel(viewModel, config, tcs);

            config.Host.DialogContent = view;
            config.Host.IsDialogOpen = true;

            return await tcs.Task;
        }
        catch
        {
            CleanupDialog(config.Host);
            throw;
        }
    }

    private void SetupDialogViewModel(
        object viewModel,
        DialogConfiguration config,
        TaskCompletionSource<IDialogResult> tcs)
    {
        if (viewModel is not IClosableDialog closableDialogViewModel)
            return;

        var closeCommand = new DelegateCommand<IDialogResult>(result =>
        {
            try
            {
                CleanupDialog(config.Host);
                tcs.TrySetResult(result);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        closableDialogViewModel.SetCloseCommand(closeCommand);

        if (viewModel is IDialogAware dialogAwareViewModel && config.Parameters != null)
            dialogAwareViewModel.OnDialogOpened(config.Parameters);
    }

    private void CleanupDialog(IDialogHost? host)
    {
        if (host is null) return;

        host.IsDialogOpen = false;
        host.DialogContent = null;
    }

    private Type FindViewType(Type viewModelType)
    {
        var viewTypeName = GetViewNameFromViewModel(viewModelType.Name);

        var viewType = viewModelType.Assembly.GetTypes()
            .FirstOrDefault(t => t.Name == viewTypeName);

        if (viewType == null)
            throw new InvalidOperationException(
                string.Format(ViewForViewModelNotFoundExceptionMessage, viewTypeName)
            );

        return viewType;
    }

    private static string GetViewNameFromViewModel(string viewModelName)
    {
        if (viewModelName.EndsWith("ViewModel", StringComparison.Ordinal)) return viewModelName[..^9] + "View";

        return viewModelName.Replace("ViewModel", "View");
    }
}