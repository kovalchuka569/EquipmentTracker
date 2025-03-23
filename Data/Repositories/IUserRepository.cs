using Data.Entities;

namespace Data.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByLoginAsync(string login);
    }
}
