using System.Collections.Generic;

using Presentation.ViewModels;
using Presentation.Views;

namespace Presentation.Interfaces;

public interface IGenericTabManager
{
    GenericTabViewModel CreateViewModel(Dictionary<string, object> parameters);
    
}