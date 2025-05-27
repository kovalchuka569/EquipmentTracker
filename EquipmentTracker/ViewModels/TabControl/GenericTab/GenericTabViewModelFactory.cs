using Prism.Events;
using Prism.Regions;

namespace UI.ViewModels.TabControl.GenericTab;

public interface IGenericTabViewModelFactory
{
    GenericTabViewModel Create(Dictionary<string, object> parameters);
}

public class GenericTabViewModelFactory : IGenericTabViewModelFactory
{
    private readonly IRegionManager _parentRegionManager;

    public GenericTabViewModelFactory(IRegionManager parentRegionManager)
    {
        _parentRegionManager = parentRegionManager;
    }

    public GenericTabViewModel Create(Dictionary<string, object> parameters)
    {
        IRegionManager scopedRegionManager = _parentRegionManager.CreateRegionManager();
        return new GenericTabViewModel(scopedRegionManager) { Parameters = parameters };
    }
}

