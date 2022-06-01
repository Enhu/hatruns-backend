using HatCommunityWebsite.DB;
using Microsoft.EntityFrameworkCore;

namespace HatCommunityWebsite.Repo
{
    public interface IUserRepository
    {
        Task<User> GetUserById(int id);

        Task<User> GetUserByUsername(string username);

        Task<User> GetUserByEmail(string email);

        Task<User> GetUserByResetPasswordToken(string token);

        Task<User> GetUserByVerificationToken(string token);

        Task<User> GetUserByRefreshToken(string token);

        Task<List<User>> GetAllUsers();

        Task SaveUser(User user);

        Task UpdateUser(User user);

        Task DeleteUser(User user);

        Task<bool> UserExistsByEmail(string email);

        Task<bool> UserExistsByUsername(string username);

        Task<bool> UserResetPasswordTokenIsUnique(string token);

        Task<bool> UserVerificationTokenIsUnique(string token);
    }

    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserById(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetUserByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User> GetUserByResetPasswordToken(string token)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.ResetPasswordToken == token && x.ResetPasswordTokenExpires > DateTime.UtcNow);
        }

        public async Task<User> GetUserByVerificationToken(string token)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.VerificationToken == token);
        }

        public async Task<User> GetUserByRefreshToken(string token)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.RefreshToken == token);
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task SaveUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UserExistsByEmail(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email == email);
        }

        public async Task<bool> UserExistsByUsername(string username)
        {
            return await _context.Users.AnyAsync(x => x.Username == username);
        }

        public async Task<bool> UserVerificationTokenIsUnique(string token)
        {
            return await _context.Users.AnyAsync(x => x.VerificationToken == token);
        }

        public async Task<bool> UserResetPasswordTokenIsUnique(string token)
        {
            return await _context.Users.AnyAsync(x => x.ResetPasswordToken == token);
        }

        public async Task UpdateUser(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUser(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}