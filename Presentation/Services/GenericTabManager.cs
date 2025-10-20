using System;
using System.Collections.Generic;

using Prism.Ioc;
using Prism.Navigation.Regions;

using Presentation.Interfaces;
using Presentation.ViewModels;

namespace Presentation.Services;


public class GenericTabManager(IRegionManager parentRegionManager, IContainerExtension containerExtension) : IGenericTabManager
{
    public GenericTabViewModel CreateViewModel(Dictionary<string, object> parameters)
    {
        IRegionManager scopedRegionManager = parentRegionManager.CreateRegionManager();
        IScopedProvider tabScopedServiceProvider = containerExtension.CreateScope();
        
        var viewModel = new GenericTabViewModel(scopedRegionManager, tabScopedServiceProvider)
        {
            Parameters = parameters
        };
        
        return viewModel;
    }
}

