using System;
using Common.Logging;
using Notification.Wpf;
using Prism.Navigation.Regions;

namespace Presentation.ViewModels;

public class PivotSheetViewModel : BaseViewModel<PivotSheetViewModel>, INavigationAware
{
    private Guid _pivotSheetId;
    
    private bool _isInitialized;
    
    #region Constructor
    
    public PivotSheetViewModel(NotificationManager notificationManager, 
        IAppLogger<PivotSheetViewModel> logger) : base(notificationManager, logger)
    {
    }
    
    #endregion

    #region Navigation 
    
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(!_isInitialized)
         return;
        
        _pivotSheetId = navigationContext.Parameters.GetValue<Guid>("PivotSheetId");
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
    
    #endregion
}