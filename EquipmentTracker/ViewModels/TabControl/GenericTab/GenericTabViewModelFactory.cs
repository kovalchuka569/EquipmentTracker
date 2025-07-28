
namespace UI.ViewModels.TabControl.GenericTab;

public interface IGenericTabViewModelFactory
{
    GenericTabViewModel Create(Dictionary<string, object> parameters);
}

public class GenericTabViewModelFactory : IGenericTabViewModelFactory
{
    private readonly IRegionManager _parentRegionManager;
    private readonly IContainerExtension _containerExtension;

    public GenericTabViewModelFactory(IRegionManager parentRegionManager, IContainerExtension containerExtension)
    {
        _parentRegionManager = parentRegionManager;
        _containerExtension = containerExtension;
    }

    public GenericTabViewModel Create(Dictionary<string, object> parameters)
    {
        IRegionManager scopedRegionManager = _parentRegionManager.CreateRegionManager();
        IScopedProvider tabScopedServiceProvider = _containerExtension.CreateScope();
        var viewModel = new GenericTabViewModel(scopedRegionManager, tabScopedServiceProvider)
        {
            Parameters = parameters
        };
        return viewModel;
    }
}

