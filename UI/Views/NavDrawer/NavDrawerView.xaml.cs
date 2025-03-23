using System.Windows.Controls;

namespace UI.Views.NavDrawer;

public partial class NavDrawerView : UserControl
{
    public NavDrawerView(IRegionManager regionManager)
    {
        InitializeComponent();
        
        RegionManager.SetRegionManager(cc, regionManager);
        RegionManager.SetRegionName(cc, "ContentRegion");
    }
}