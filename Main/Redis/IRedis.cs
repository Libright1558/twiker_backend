using twiker_backend.Redis.Models;

namespace twiker_backend.Redis
{
    public interface IRedisPostData
    {
        Task<RedisPostTable> GetPostInfo(string selfUserId, string[] postIdArray);

        Task WritePostInfo(string selfUserId, PostArrayNest postNestObj);

        Task DeletePostInfo(string selfUserId, string postId);

        Task DeletePostLikeNums(string postId);

        Task DeletePostRetweetNums(string postId);

        Task DeletePostSelfLike(string selfUserId, string postId);

        Task DeletePostSelfRetweet(string selfUserId, string postId);

        Task DeletePostPersonalData(string postId);

        Task<string[]> GetPostIdArray(string userId);

        Task WritePostIdArray(string userId, string[] member);

        Task DeletePostIdArray(string userId);

        Task SetPostInfoExp(string selfUserId, string[] postIdArray, int times);
    }

    public interface IRedisUserData
    {
        Task<RedisUserData> GetUserInfoAsync(string userId);

        Task WriteUserInfoAsync(string userId, RedisUserData userInfo, int expirySeconds);

        Task SetUserInfoExp(string userId, int expirySeconds);

        Task DeleteUserInfo(string userId);
    }
}