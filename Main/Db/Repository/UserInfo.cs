using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using Npgsql;
using twiker_backend.Models.DatabaseContext;
using twiker_backend.Db.Models;

namespace twiker_backend.Db.Repository
{
    public class DbUserInfo(TwikerContext context) : IDbUserInfo
    {
        private readonly TwikerContext _context = context;

        public async Task<UserDbData?> GetUserData(Guid UserId) 
        {
            try
            {
                var user = await _context.UserTables
                    .Where(u => u.UserId == UserId)
                    .Select(u => new { u.Firstname, u.Lastname, u.Username, u.Email, u.Profilepic })
                    .FirstOrDefaultAsync();

                return user == null ? null : new UserDbData {
                    Firstname = user.Firstname!,
                    Lastname = user.Lastname!,
                    Username = user.Username!,
                    Email = user.Email!,
                    Profilepic = user.Profilepic!
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<int> WriteUserData(UserTable UserInfo)
        {
            try
            {
                var user = new UserTable {
                    Firstname = UserInfo.Firstname!,
                    Lastname = UserInfo.Lastname!,
                    Username = UserInfo.Username!,
                    Email = UserInfo.Email!,
                    Password = UserInfo.Password!,
                    Profilepic = UserInfo.Profilepic!
                };

                _context.UserTables.Add(user);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task DeleteUserData(Guid UserId)
        {
            try
            {
                var user = await _context.UserTables
                .FirstOrDefaultAsync(u => u.UserId == UserId);

                if (user != null)
                {
                    _context.UserTables.Remove(user);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<UserDbData?> FindOneUser(string UsernameOrEmail)
        {
            await using var connection = new NpgsqlConnection(DbConnectManager.DbConnectionString);

            try
            {
                var user = await _context.UserTables
                    .Where(u => u.Username == UsernameOrEmail || u.Email == UsernameOrEmail)
                    .Select(u => new { u.UserId, u.Username, u.Email, u.Password })
                    .FirstOrDefaultAsync();
                
                return user == null ? null : new UserDbData {
                    UserId = user.UserId!,
                    Username = user.Username!,
                    Email = user.Email!,
                    Password = user.Password!
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}