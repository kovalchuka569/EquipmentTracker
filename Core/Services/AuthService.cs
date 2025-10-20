using Common.Enums;
using Common.Logging;
using Core.Interfaces;
using Core.Mappers;
using Core.Services.Base;
using Data.UnitOfWork;
using Models.Users;

namespace Core.Services;

public class AuthService(IUnitOfWork unitOfWork, IAppLogger<AuthService> logger)
    : DatabaseService<AuthService>(unitOfWork, logger), IAuthService
{
    public async Task AddUserAsync(User user, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            await ExecuteInTransactionAsync(async () =>
            {
                await UnitOfWork.UserRepository.AddAsync(UserMapper.UserModelToEntity(user), ct);
            }, cancellationToken: ct);
        }, operationName: nameof(AddUserAsync), cancellationToken: ct);
    }

    public async Task<bool> IsLoginExistAsync(string login, CancellationToken ct)
    {
       return await ExecuteInLoggerAsync(async () => await UnitOfWork.UserRepository
           .LoginExistsAsync(login, ct), operationName: nameof(IsLoginExistAsync), cancellationToken: ct);
    }

    public async Task<User?> GetUserAsync(string login, CancellationToken ct)
    {
        return await ExecuteInLoggerAsync(async () =>
        {
            var userEntity = await UnitOfWork.UserRepository
                .GetUserEntityAsync(login, ct);

            return UserMapper.UserEntityToModel(userEntity);
        }, nameof(GetUserAsync), ct);
    }
}