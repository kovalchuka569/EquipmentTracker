
using Core.Events.TabControl;
using Models.Enums;


namespace UI.ViewModels.NavDrawer
{
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
                    { "EquipmentTreeView.MenuType", type }
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
                    MenuType.Cars => "EquipmentTreeView",

                MenuType.Consumables => "ConsumablesTreeView",
                MenuType.History => "AccountingView",
                MenuType.Scheduler => "SchedulerView",
                MenuType.Settings => "SettingsView",
                _ => "UnknownView"
            };
        }
    }
}