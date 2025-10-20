using System;
using Models.Users;

namespace Presentation.ViewModels.Common.Users;

public class QueryUserViewModel : UserViewModel
{
    private DateTime _accessRequestedAt;
    
    public DateTime AccessRequestedAt
    {
        get => _accessRequestedAt;
        set => SetProperty(ref _accessRequestedAt, value);
    }

    public QueryUserViewModel(User domain) : base(domain)
    {
        AccessRequestedAt = domain.AccessRequestedAt;
    }

    protected override User ToDomain()
    {
        var user = base.ToDomain();
        user.AccessRequestedAt = AccessRequestedAt;
        return user;
    }
}