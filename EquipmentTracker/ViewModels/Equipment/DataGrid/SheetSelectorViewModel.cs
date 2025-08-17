using System.Collections.ObjectModel;

namespace UI.ViewModels.Equipment.DataGrid;

public class SheetSelectorViewModel : BindableBase, INavigationAware
{
    private IRegionManager _scopedRegionManager;
    private Action<string?, int, int, bool> _callback;

    public ObservableCollection<string> SheetNames { get; } = new();

    private string? _selectedSheet;
    public int HeaderRow { get; set; } = 1;
    private string _headerColInput = "A";
    public string HeaderColInput
    {
        get => _headerColInput;
        set => SetProperty(ref _headerColInput, value);
    }
    public string? SelectedSheet
    {
        get => _selectedSheet; set => SetProperty(ref _selectedSheet, value);
    }

    public DelegateCommand OkCommand { get; }
    public DelegateCommand CancelCommand { get; }

    public SheetSelectorViewModel()
    {
        OkCommand = new DelegateCommand(OnOk);
        CancelCommand = new DelegateCommand(() => _callback?.Invoke(null, 0, 0, false));
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        _scopedRegionManager = (IRegionManager)navigationContext.Parameters["ScopedRegionManager"];
        if (navigationContext.Parameters["SheetNames"] is IEnumerable<string> sheets)
        {
            SheetNames.Clear();
            foreach (var s in sheets) SheetNames.Add(s);
            SelectedSheet = SheetNames.FirstOrDefault();
        }
        _callback = (Action<string?, int, int, bool>)navigationContext.Parameters["SelectionCallback"];
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    private void OnOk()
    {
        int col = ParseColumn(HeaderColInput);
        if (col <= 0) col = 1;
        _callback?.Invoke(SelectedSheet, HeaderRow, col, true);
    }

    private static int ParseColumn(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return 0;
        input = input.Trim();
        // if numeric
        if (int.TryParse(input, out int num)) return num;
        int res = 0;
        foreach (char ch in input.ToUpper())
        {
            if (ch < 'A' || ch > 'Z') return 0;
            res = res * 26 + (ch - 'A' + 1);
        }
        return res;
    }
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
}
