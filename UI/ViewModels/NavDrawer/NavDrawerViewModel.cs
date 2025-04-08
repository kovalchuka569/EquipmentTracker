using Core.Events.NavDrawer;


namespace UI.ViewModels.NavDrawer
{
    public class NavDrawerViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        
        public DelegateCommand<string> NavigateToTabControlExt { get; }

        public NavDrawerViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            NavigateToTabControlExt = new DelegateCommand<string>(OnNavigateToTabControlExt);
        }
        

        private void OnNavigateToTabControlExt(string parameter)
        {
            _regionManager.RequestNavigate("ContentRegion", "TabControlView");
            switch (parameter)
            {
                case "Виробниче обладнання" or "Меблі" or "Інструменти" or "Офісна техніка":
                    Console.WriteLine(parameter);
                    _eventAggregator.GetEvent<OpenEquipmentTreeTabEvent>().Publish(parameter);
                    break;
                case "Налаштування" or "Календар" or "Розхідні матеріали" or "Облік":
                    Console.WriteLine(parameter);
                    _eventAggregator.GetEvent<OpenOtherTabEvent>().Publish(parameter);
                    break;
            }
            
        }
    }
}