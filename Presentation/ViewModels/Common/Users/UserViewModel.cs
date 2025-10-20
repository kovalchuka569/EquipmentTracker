using System;
using Common.Enums;
using Models.Users;
using Prism.Mvvm;

namespace Presentation.ViewModels.Common.Users;

public class UserViewModel : BindableBase
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _login = string.Empty;
    private UserStatus _status = UserStatus.None;

    public Guid Id { get; set; }
    
    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    public UserStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    protected UserViewModel(User domain)
    {
        Id = domain.Id;
        FirstName = domain.FirstName;
        LastName = domain.LastName;
        Login = domain.Login;
        Status = domain.Status;
    }

    protected virtual User ToDomain()
    {
        return new User
        {
            Id = Id,
            FirstName = FirstName,
            LastName = LastName,
            Login = Login,
            Status = Status
        };
    }
}