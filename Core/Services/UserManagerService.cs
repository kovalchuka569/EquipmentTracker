using Common.Enums;
using Common.Logging;
using Core.Interfaces;
using Core.Mappers;
using Core.Services.Base;
using Data.UnitOfWork;
using Models.Users;

namespace Core.Services;

public class UserManagerService(IUnitOfWork unitOfWork, IAppLogger<UserManagerService> logger)
    : DatabaseService<UserManagerService>(unitOfWork, logger), IUserManagerService
{
    public Task<List<User>> GetActiveUsersAsync(CancellationToken ct = default)
        => GetUsersByStatusAsync(UserStatus.Active, ct);

    public Task<List<User>> GetAwaitConfirmationUsersAsync(CancellationToken ct = default)
        => GetUsersByStatusAsync(UserStatus.AwaitingConfirmation, ct);

    public async Task<int> GetAwaitConfirmationUsersCountAsync(CancellationToken ct)
    {
        return await ExecuteInLoggerAsync(
            async () => await UnitOfWork.UserRepository.GetAwaitConfirmationUsersCountAsync(ct),
            nameof(GetAwaitConfirmationUsersCountAsync), ct);
    }
    
    public async Task ApproveQueryUserAsync(Guid userId, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            await UnitOfWork.UserRepository.ChangeUserStatusAsync(userId, UserStatus.Active, ct);
        }, nameof(ApproveQueryUserAsync), cancellationToken: ct);
    }

    public async Task RejectQueryUserAsync(Guid userId, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            await UnitOfWork.UserRepository.ChangeUserStatusAsync(userId, UserStatus.Rejected, ct);
        }, nameof(RejectQueryUserAsync), cancellationToken: ct);
    }
    
    private async Task<List<User>> GetUsersByStatusAsync(UserStatus status, CancellationToken ct)
    {
        return await ExecuteInLoggerAsync(async () =>
        {
            var entities = await UnitOfWork.UserRepository.GetAllAsync(status, ct);
            return entities.Select(UserMapper.UserEntityToModel)
                .OfType<User>()
                .ToList();
        }, nameof(GetUsersByStatusAsync), ct);
    }
    
}