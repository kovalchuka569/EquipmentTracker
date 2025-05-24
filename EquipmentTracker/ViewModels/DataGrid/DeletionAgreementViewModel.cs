using Core.Events.DataGrid;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace EquipmentTracker.ViewModels.DataGrid;

public class DeletionAgreementViewModel : BindableBase, INavigationAware, IDeletionAgreementResult
{
    public bool? Result { get; private set; }
    public Action<IDeletionAgreementResult> Callback { get; set; }
    
    public DelegateCommand AgreeCommand { get; }
    public DelegateCommand DisagreeCommand { get; }

    public DeletionAgreementViewModel()
    {
        AgreeCommand = new DelegateCommand(OnAgree);
        DisagreeCommand = new DelegateCommand(OnDisagree);
    }

    private void OnAgree()
    {
        Result = true;
        Callback?.Invoke(this);
    }

    private void OnDisagree()
    {
        Result = false;
        Callback?.Invoke(this);
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters["Callback"] is Action<IDeletionAgreementResult> result)
        {
            Callback = result;
        }
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext){ Callback = null; }
}