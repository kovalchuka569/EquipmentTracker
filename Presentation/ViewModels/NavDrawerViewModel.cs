using System.Collections.Generic;

using Prism.Events;
using Prism.Mvvm;
using Prism.Commands;

using Core.Events.TabControl;

using Common.Enums;


namespace Presentation.ViewModels;

public class NavDrawerViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;
    
    public DelegateCommand<object> NavigateToTabControlExt { get; }

    public NavDrawerViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        
        NavigateToTabControlExt = new DelegateCommand<object>(OnNavigateToTabControlExt);
    }

    private void OnNavigateToTabControlExt(object menuType)
    {
        if (menuType is MenuType type)
        {
            string viewName = GetViewName(type);
            string header = GetHeader(type);
            var parameters = new Dictionary<string, object>()
            {
                { "ViewNameToShow", viewName },
                { "MainTreeView.MenuType", type }
            };
            
            _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
            {
                Header = header,
                Parameters = parameters
            });
        }
    }
    
    private string GetHeader(MenuType type)
    {
        return type switch
        {
            MenuType.Prod => "Виробниче обладнання",
            MenuType.Tools => "Інструменти",
            MenuType.Furniture => "Меблі",
            MenuType.Office => "Офісна техніка",
            MenuType.Cars => "Автопарк",
            MenuType.Consumables => "Розхідні матеріали",
            MenuType.History => "Історія",
            MenuType.Scheduler => "Календар",
            MenuType.Settings => "Налаштування",
            _ => "Невідомо"
        };
    }
    
    private string GetViewName(MenuType type)
    {
        return type switch
        {
            MenuType.Prod or
                MenuType.Tools or
                MenuType.Furniture or
                MenuType.Office or
                MenuType.Cars => "MainTreeView",

            MenuType.Consumables => "ConsumablesTreeView",
            MenuType.History => "AccountingView",
            MenuType.Scheduler => "SchedulerView",
            MenuType.Settings => "SettingsView",
            _ => "UnknownView"
        };
    }
}