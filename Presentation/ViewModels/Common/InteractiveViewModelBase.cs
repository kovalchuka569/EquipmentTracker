using Presentation.Contracts;
using Presentation.Services.Interfaces;
using Prism.Navigation;
using Prism.Navigation.Regions;
using Unity;

namespace Presentation.ViewModels.Common;

public abstract class InteractiveViewModelBase : ViewModelBase, IDialogHost, IOverlayHost, IBusyIndicatorHost, INavigationAware, IDestructible
{
    [Dependency]
    public required IDialogService DialogService { get; init; } = null!; 
    
    [Dependency]
    public required IBusyIndicatorService BusyIndicatorService { get; init; } = null!;
    
    [Dependency]
    public required IRegionManager RegionManager { get; init; } = null!;
    
    private bool _isDialogOpen;
    private object? _dialogContent;
    private bool _isOverlayOpen;
    private object? _overlayContent;
    private bool _isBusy;
    private object? _busyContent;
    
    public virtual bool IsDialogOpen
    {
        get => _isDialogOpen;
        set => SetProperty(ref _isDialogOpen, value);
    }

    public virtual object? DialogContent
    {
        get => _dialogContent;
        set => SetProperty(ref _dialogContent, value);
    }

    public virtual bool IsOverlayOpen
    {
        get => _isOverlayOpen;
        set => SetProperty(ref _isOverlayOpen, value);
    }

    public virtual object? OverlayContent
    {
        get => _overlayContent;
        set => SetProperty(ref _overlayContent, value);
    }

    public virtual bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public virtual object? BusyContent
    {
        get => _busyContent;
        set => SetProperty(ref _busyContent, value);
    }
    
    public virtual void OnNavigatedTo(NavigationContext navigationContext)
    {
    }

    public virtual bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }

    public virtual void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }

    public virtual void Destroy()
    {
    }
}