using StackExchange.Redis;
using twiker_backend.Redis.Models;

namespace twiker_backend.Redis
{
    public class PostInfo : IRedisPostData
    {

        private readonly IConnectionMultiplexer _connectionMultiplexer;

        private readonly IDatabase _db;

        private const string PostIdArraySuffix = "_PostIdArray";

        private static Task<string[]> GetResults(Task<RedisValue>[] tasks)
        {
            return Task.WhenAll(tasks).ContinueWith(t => t.Result.Select(x => x.ToString()).ToArray());
        }

        private IEnumerable<Task> WriteField(IBatch batch, Dictionary<RedisKey, RedisValue> field)
        {
            return field.Select(kv => batch.StringSetAsync(kv.Key, kv.Value));
        }

        private Task DeleteKey(string postId, string suffix)
        {
            return _db.KeyDeleteAsync($"{postId}_{suffix}");
        }

        private static string GetKey(string postId, string selfUserId, string field)
        {
            return field switch
            {
                "SelfLike" or "SelfRetweet" => $"{postId}_{selfUserId}_{field}",
                _ => $"{postId}_{field}"
            };
        }

        public PostInfo(IConnectionMultiplexer multiplexer)
        {
            _connectionMultiplexer = multiplexer;
            _db = _connectionMultiplexer.GetDatabase();
        }

        public async Task<RedisPostTable> GetPostInfo(string selfUserId, string[] postIdArray)
        {
            var batch = _db.CreateBatch();
            var tasks = new Dictionary<string, Task<RedisValue>[]>();
            var fields = new[] { "Content", "CreatedAt", "LikeNums", "RetweetNums", "PostOwner", "Firstname", "Lastname", "Profilepic", "SelfLike", "SelfRetweet" };

            foreach (var field in fields)
            {
                tasks[field] = postIdArray.Select(postId => 
                    batch.StringGetAsync(GetKey(postId, selfUserId, field))).ToArray();
            }

            batch.Execute();
            await Task.WhenAll(tasks.Values.SelectMany(t => t));

            return new RedisPostTable
            {
                Content = await GetResults(tasks["Content"]),
                CreatedAt = await GetResults(tasks["CreatedAt"]),
                LikeNums = await GetResults(tasks["LikeNums"]),
                RetweetNums = await GetResults(tasks["RetweetNums"]),
                PostOwner = await GetResults(tasks["PostOwner"]),
                Firstname = await GetResults(tasks["Firstname"]),
                Lastname = await GetResults(tasks["Lastname"]),
                Profilepic = await GetResults(tasks["Profilepic"]),
                SelfLike = await GetResults(tasks["SelfLike"]),
                SelfRetweet = await GetResults(tasks["SelfRetweet"])
            };
        }

        public async Task WritePostInfo(string selfUserId, PostArrayNest postNestObj)
        {
            var batch = _db.CreateBatch();
            var tasks = new List<Task>();

            if (postNestObj.Content.Count > 0)
            {
                tasks.AddRange(WriteField(batch, postNestObj.Content));
            }
            
            if (postNestObj.CreatedAt.Count > 0)
            {
                tasks.AddRange(WriteField(batch, postNestObj.CreatedAt));
            }

            if (postNestObj.LikeNums.Count > 0)
            {
                tasks.AddRange(WriteField(batch, postNestObj.LikeNums));
            }

            if (postNestObj.RetweetNums.Count > 0)
            {
                tasks.AddRange(WriteField(batch, postNestObj.RetweetNums));
            }

            if (postNestObj.PostOwner.Count > 0)
            {
                tasks.AddRange(WriteField(batch, postNestObj.PostOwner));
            }

            if (postNestObj.Firstname.Count > 0)
            {
                tasks.AddRange(WriteField(batch, postNestObj.Firstname));
            }

            if (postNestObj.Lastname.Count > 0)
            {
                tasks.AddRange(WriteField(batch, postNestObj.Lastname));
            }

            if (postNestObj.Profilepic.Count > 0)
            {
                tasks.AddRange(WriteField(batch, postNestObj.Profilepic));
            }

            if (postNestObj.SelfLike.Count > 0)
            {
                tasks.AddRange(WriteField(batch, postNestObj.SelfLike));
            }

            if (postNestObj.SelfRetweet.Count > 0)
            {
                tasks.AddRange(WriteField(batch, postNestObj.SelfRetweet));
            }
            
            batch.Execute();
            await Task.WhenAll(tasks);
        }

        public Task DeletePostInfo(string selfUserId, string postId)
        {
            var keys = new[]
            {
                "Content", "CreatedAt", "LikeNums", "RetweetNums", "PostOwner",
                "Firstname", "Lastname", "Profilepic", $"{selfUserId}_SelfLike", $"{selfUserId}_SelfRetweet"
            }.Select(suffix => $"{postId}_{suffix}");

            return _db.KeyDeleteAsync(keys.Select(key => (RedisKey)key).ToArray());
        }

        public Task DeletePostLikeNums(string postId) => DeleteKey(postId, "LikeNums");
        public Task DeletePostRetweetNums(string postId) => DeleteKey(postId, "RetweetNums");
        public Task DeletePostSelfLike(string selfUserId, string postId) => DeleteKey(postId, $"{selfUserId}_SelfLike");
        public Task DeletePostSelfRetweet(string selfUserId, string postId) => DeleteKey(postId, $"{selfUserId}_SelfRetweet");

        public Task DeletePostPersonalData(string postId)
        {
            var keys = new[] { "PostOwner", "Firstname", "Lastname", "Profilepic" }
                .Select(suffix => $"{postId}_{suffix}");
            return _db.KeyDeleteAsync(keys.Select(key => (RedisKey)key).ToArray());
        }

        public async Task<string[]> GetPostIdArray(string userId)
        {
            var result = await _db.ListRangeAsync(userId + PostIdArraySuffix);
            return result.Select(x => x.ToString()).ToArray();
        }

        public Task WritePostIdArray(string userId, string[] member)
        {
            return _db.ListRightPushAsync(userId + PostIdArraySuffix, member.Select(x => (RedisValue)x).ToArray());
        }

        public Task DeletePostIdArray(string userId)
        {
            return _db.KeyDeleteAsync(userId + PostIdArraySuffix);
        }

        public async Task SetPostInfoExp(string selfUserId, string[] postIdArray, int times)
        {
            var batch = _db.CreateBatch();
            var tasks = new List<Task>();
            var expiry = TimeSpan.FromSeconds(times);

            foreach (var postId in postIdArray)
            {
                tasks.AddRange(new[]
                {
                    "PostOwner", "Content", "CreatedAt", "LikeNums", "RetweetNums",
                    $"{selfUserId}_SelfLike", $"{selfUserId}_SelfRetweet",
                    "Firstname", "Lastname", "Profilepic"
                }.Select(suffix => batch.KeyExpireAsync($"{postId}_{suffix}", expiry, ExpireWhen.HasNoExpiry)));
            }

            tasks.Add(batch.KeyExpireAsync(selfUserId + PostIdArraySuffix, expiry, ExpireWhen.HasNoExpiry));
            
            batch.Execute();
            await Task.WhenAll(tasks);
        }
    }
}