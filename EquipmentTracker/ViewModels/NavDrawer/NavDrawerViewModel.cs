using Core.Events.NavDrawer;
using Core.Events.TabControl;
using Core.Events.Themes;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;


namespace UI.ViewModels.NavDrawer
{
    public class NavDrawerViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        
        public DelegateCommand<string> NavigateToTabControlExt { get; }

        public NavDrawerViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            
            NavigateToTabControlExt = new DelegateCommand<string>(OnNavigateToTabControlExt);
        }

        private void OnNavigateToTabControlExt(string parameter)
        {
            switch (parameter)
            {
                case "Виробниче обладнання":
                    _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
                    {
                        Header = "Виробниче обладнання",
                        Parameters = new Dictionary<string, object>
                        {
                            {"ViewNameToShow", "EquipmentTreeView"},
                            {"EquipmentTreeView.MenuType", "Виробниче обладнання"}
                        }
                    });
                    break;
                
                case "Інструменти":
                    _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
                    {
                        Header = "Інструменти",
                        Parameters = new Dictionary<string, object>
                        {
                            {"ViewNameToShow", "EquipmentTreeView"},
                            {"EquipmentTreeView.MenuType", "Інструменти"}
                        }
                    });
                    break;
                
                case "Меблі":
                    _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
                    {
                        Header = "Меблі",
                        Parameters = new Dictionary<string, object>
                        {
                            {"ViewNameToShow", "EquipmentTreeView"},
                            {"EquipmentTreeView.MenuType", "Меблі"}
                        }
                    });
                    break;
                
                case "Офісна техніка":
                    _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
                    {
                        Header = "Офісна техніка",
                        Parameters = new Dictionary<string, object>
                        {
                            {"ViewNameToShow", "EquipmentTreeView"},
                            {"EquipmentTreeView.MenuType", "Офісна техніка"}
                        }
                    });
                    break;
                
                case "Розхідні матеріали":
                    _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
                    {
                        Header = "Розхідні матеріали",
                        Parameters = new Dictionary<string, object>
                        {
                            {"ViewNameToShow", "ConsumablesTreeView"},
                        }
                    });
                    break;
                
                case "Облік":
                    _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
                    {
                        Header = "Облік",
                        Parameters = new Dictionary<string, object>
                        {
                            {"ViewNameToShow", "AccountingView"},
                        }
                    });
                    break;
                
                case "Налаштування":
                    _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
                    {
                        Header = "Облік",
                        Parameters = new Dictionary<string, object>
                        {
                            {"ViewNameToShow", "SettingsView"},
                        }
                    });
                    break;
                
                case "Календар":
                    _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
                    {
                        Header = "Календар",
                        Parameters = new Dictionary<string, object>
                        {
                            {"ViewNameToShow", "SchedulerView"},
                        }
                    });
                    break;
            }
        }
    }
}