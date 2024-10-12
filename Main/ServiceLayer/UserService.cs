using System;
using System.Threading.Tasks;
using twiker_backend.Redis.Models;
using twiker_backend.Redis;
using twiker_backend.Db.Repository;
using twiker_backend.Db.Models;
using Microsoft.Extensions.Logging;

namespace twiker_backend.ServiceLayer
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IRedisUserData _redisUserInfo;
        private readonly IDbUserInfo _dbUserInfo;

        public UserService (IRedisUserData redisUserInfo, IDbUserInfo dbUserInfo, ILogger<UserService> logger)
        {
            _redisUserInfo = redisUserInfo;
            _dbUserInfo = dbUserInfo;
            _logger = logger;
        }

        public async Task<RedisUserData?> GetThePersonalData (Guid userId)
        {
            try {
                RedisUserData result = await _redisUserInfo.GetUserInfoAsync(userId.ToString());

                if (result == null || result!.Username == null)
                {
                    UserDbData? Response = await _dbUserInfo.GetUserData(userId);
                    if (Response != null)
                    {
                        result = new RedisUserData
                        {
                            Firstname = Response.Firstname,
                            Lastname = Response.Lastname,
                            Username = Response.Username,
                            Email = Response.Email,
                            Profilepic = Response.Profilepic
                        };
                    }

                    if (result != null)
                    {
                        await _redisUserInfo.WriteUserInfoAsync(userId.ToString(), result, 900);
                    }
                    await _redisUserInfo.SetUserInfoExp(userId.ToString(), 900);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetThePersonalData Error");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}