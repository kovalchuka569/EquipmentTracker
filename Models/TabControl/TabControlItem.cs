
namespace Models.TabControl;

public class TabControlItem : BindableBase
{
    private object _genericTab;
    public string Header { get; set; }
    public int TabIndex { get; set; }

    public object GenericTab
    {
        get => _genericTab;
        set => SetProperty(ref _genericTab, value);
    }
    
    public DelegateCommand<TabControlItem> CloseThisCommand { get; set; }
    public DelegateCommand CloseAllCommand { get; set; }
    public DelegateCommand<TabControlItem> CloseAllButThisCommand { get; set; }
}


