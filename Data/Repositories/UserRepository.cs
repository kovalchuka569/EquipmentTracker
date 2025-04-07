using Data.Entities;
using Microsoft.EntityFrameworkCore;
using DbContext = Data.AppDbContext.DbContext;

namespace Data.Repositories
{
    public class UserRepository: IUserRepository
    {
        private readonly DbContext _context;

        public UserRepository(DbContext context)
        {
            _context = context;
        }
        
        public async Task<User> GetUserByLoginAsync(string login)
        {
            Console.WriteLine("GetUserByLoginAsync started...");
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Login == login);
            Console.WriteLine("GetUserByLoginAsync completed!");
            return user;
        }
    }
}
