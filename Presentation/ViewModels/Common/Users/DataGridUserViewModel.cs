using Models.Users;

namespace Presentation.ViewModels.Common.Users;

public class DataGridUserViewModel : UserViewModel
{
    public DataGridUserViewModel(User domain) : base(domain)
    {
    }
}