using System.Windows.Controls;

namespace Core.Models.TabControl;

public class TabControlModel : BindableBase
{
    private string _header;
    public string Header
    {
        get => _header;
        set => SetProperty(ref _header, value);
    }
    
    private object _content;

    public object Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }
    
    private string _parameter;

    public string Parameter
    {
        get => _parameter;
        set => SetProperty(ref _parameter, value);
    }
}
