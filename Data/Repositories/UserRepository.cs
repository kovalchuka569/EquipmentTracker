using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class UserRepository: IUserRepository
    {
        private readonly AppDbContext.AppDbContext _context;

        public UserRepository(AppDbContext.AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<User> GetUserByLoginAsync(string login)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Login == login);
        }
    }
}
