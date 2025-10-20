using Data.Entities;
using Models.Users;

namespace Core.Mappers;

public static class UserMapper
{
    public static UserEntity UserModelToEntity(User user)
    {
        return new UserEntity
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Login = user.Login,
            PasswordHash = user.PasswordHash,
            AccessRequestedAt = user.AccessRequestedAt,
            Status = user.Status,
        };
    }

    public static User? UserEntityToModel(UserEntity? userEntity)
    {
        if (userEntity is null)
            return null;
        
        return new User
        {
            Id = userEntity.Id,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
            Login = userEntity.Login,
            PasswordHash = userEntity.PasswordHash,
            AccessRequestedAt = userEntity.AccessRequestedAt,
            Status = userEntity.Status,
        };
    }
}