using Common.Enums;
using Data.ApplicationDbContext;
using Data.Entities;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task AddAsync(UserEntity user, CancellationToken ct)
    {
        await context.Users.AddAsync(user, ct);
    }

    public async Task ChangeUserStatusAsync(Guid userId, UserStatus status, CancellationToken ct)
    {
        await context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(f => f
                .SetProperty(s => s.Status, status), cancellationToken: ct);
    }

    public Task<bool> LoginExistsAsync(string login, CancellationToken ct = default)
    {
        return context.Users.AnyAsync(u => u.Login == login, ct);
    }

    public Task<UserEntity?> GetUserEntityAsync(string login, CancellationToken ct = default)
    {
        return context.Users.Where(u => u.Login == login)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    public Task<int> GetAwaitConfirmationUsersCountAsync(CancellationToken ct = default)
    {
        return context.Users.Where(u => u.Status == UserStatus.AwaitingConfirmation)
            .AsNoTracking()
            .CountAsync(ct);
    }

    public Task<List<UserEntity>> GetAllAsync(UserStatus? status, CancellationToken ct = default)
    {
        var query = context.Users.AsNoTracking();
        
        if (status is not null)
            query = query.Where(u => u.Status == status.Value);

        return query.ToListAsync(ct);
    }
}