using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using twiker_backend.Redis.Models;

namespace twiker_backend.Redis
{
    public class UserInfo : IRedisUserData
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        
        private readonly IDatabase _db;

        private static readonly string[] _userFields = ["Firstname", "Lastname", "Username", "Email", "Profilepic"];

        private static string GetKey(string userId, string field) => $"{userId}_{field}";

        private static Task<bool> SetUserField(IBatch batch, string userId, string field, string value, TimeSpan expiry)
        {
            return batch.StringSetAsync(GetKey(userId, field), value, expiry, When.NotExists);
        }

        private static Task<bool> DeleteUserField(IBatch batch, string userId, string field)
        {
            return batch.KeyDeleteAsync(GetKey(userId, field));
        }

        public UserInfo(IConnectionMultiplexer multiplexer)
        {
            _connectionMultiplexer = multiplexer;
            _db = _connectionMultiplexer.GetDatabase();
        }

        public async Task<RedisUserData> GetUserInfoAsync(string userId)
        {
            var batch = _db.CreateBatch();
            var tasks = new Dictionary<string, Task<RedisValue>>();

            foreach (var field in _userFields)
            {
                tasks[field] = batch.StringGetAsync(GetKey(userId, field));
            }

            batch.Execute();
            await Task.WhenAll(tasks.Values);

            return new RedisUserData
            {
                Firstname = tasks["Firstname"].Result,
                Lastname = tasks["Lastname"].Result,
                Username = tasks["Username"].Result,
                Email = tasks["Email"].Result,
                Profilepic = tasks["Profilepic"].Result
            };
        }

        public async Task WriteUserInfoAsync(string userId, RedisUserData userInfo, int expirySeconds)
        {
            var batch = _db.CreateBatch();
            var tasks = new List<Task>();
            var expiry = TimeSpan.FromSeconds(expirySeconds);

            tasks.Add(SetUserField(batch, userId, "Firstname", userInfo.Firstname!, expiry));
            tasks.Add(SetUserField(batch, userId, "Lastname", userInfo.Lastname!, expiry));
            tasks.Add(SetUserField(batch, userId, "Username", userInfo.Username!, expiry));
            tasks.Add(SetUserField(batch, userId, "Email", userInfo.Email!, expiry));
            tasks.Add(SetUserField(batch, userId, "Profilepic", userInfo.Profilepic!, expiry));

            batch.Execute();
            await Task.WhenAll(tasks);
        }

        public async Task SetUserInfoExp(string userId, int expirySeconds)
        {
            var batch = _db.CreateBatch();
            var tasks = new List<Task>();
            var expiry = TimeSpan.FromSeconds(expirySeconds);

            foreach (var field in _userFields)
            {
                tasks.Add(batch.KeyExpireAsync(GetKey(userId, field), expiry));
            }

            batch.Execute();
            await Task.WhenAll(tasks);
        }

        public async Task DeleteUserInfo(string userId)
        {
            var batch = _db.CreateBatch();
            var tasks = new List<Task>();

            foreach (var field in _userFields)
            {
                tasks.Add(DeleteUserField(batch, userId, field));
            }

            batch.Execute();
            await Task.WhenAll(tasks);
        }
    }
}