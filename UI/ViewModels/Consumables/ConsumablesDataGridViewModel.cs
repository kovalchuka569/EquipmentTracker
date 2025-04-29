namespace UI.ViewModels.Consumables
{
    public class ConsumablesDataGridViewModel : BindableBase, INavigationAware
    {
        private string _testText;

        public string TestText
        {
            get => _testText;
            set => SetProperty(ref _testText, value);
        }
        
        public ConsumablesDataGridViewModel()
        {
            
        }
        
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters["parameter"] as string;
            TestText = parameters;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
