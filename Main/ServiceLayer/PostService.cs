using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using twiker_backend.Redis.Models;
using twiker_backend.Db.Repository;
using twiker_backend.Redis;
using twiker_backend.Db.Models;
using Microsoft.Extensions.Logging;

namespace twiker_backend.ServiceLayer
{
    public partial class PostService : IPostService
    {
        private readonly ILogger<PostService> _logger;
        private readonly IRedisPostData _redisPostInfo;
        private readonly IDbPostInfo _dbPostInfo;


        public PostService(IRedisPostData redisPostInfo, IDbPostInfo dbPostInfo, ILogger<PostService> logger)
        {
            _redisPostInfo = redisPostInfo;
            _dbPostInfo = dbPostInfo;
            _logger = logger;
        }

        public async Task<PostFetch?[]> GetPost (Guid userId, string username)
        {
            try
            {
                string[] PostIdArray = await _redisPostInfo.GetPostIdArray(userId.ToString());
                RedisPostTable redisPost = await _redisPostInfo.GetPostInfo(userId.ToString(), PostIdArray);

                if (PostIdArray.Length == 0 || redisPost.CreatedAt?.Length == 0)
                {
                    return await GetPostsFromDbAndCacheInRedis(userId, username);
                }
                else 
                {
                    return await GetPostsFromRedisAndFillMissingData(userId, username, PostIdArray, redisPost);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPost Error");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<PostFetch> WritePost (Guid userId, string postBy, string content)
        {
            try
            {
                PostFetch WritePostResult = await _dbPostInfo.AppendPost(postBy, content);
                await _redisPostInfo.DeletePostIdArray(userId.ToString());
                return WritePostResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WritePost Error");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task UpdateLike (Guid postId, Guid userId, string username)
        {
            try
            {
                bool IsSelfLike = await _dbPostInfo.FetchSingleSelfLikeAsync(postId, username);

                if (IsSelfLike)
                {
                    await _dbPostInfo.DeleteLike(postId, username);
                }
                else 
                {
                    await _dbPostInfo.AddLike(postId, username);
                }

                await _redisPostInfo.DeletePostLikeNums(postId.ToString());
                await _redisPostInfo.DeletePostSelfLike(userId.ToString(), postId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateLike Error");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task UpdateRetweet (Guid postId, Guid userId, string username)
        {
            try
            {
                bool IsSelfRetweet = await _dbPostInfo.FetchSingleSelfRetweetAsync(postId, username);

                if (IsSelfRetweet)
                {
                    await _dbPostInfo.DeleteRetweet(postId, username);
                }
                else {
                    await _dbPostInfo.AddRetweet(postId, username);
                }

                await _redisPostInfo.DeletePostRetweetNums(postId.ToString());
                await _redisPostInfo.DeletePostSelfRetweet(userId.ToString(), postId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateRetweet Error");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task DeletePost (Guid userId, Guid postId)
        {
            try
            {
                var DbDeletePostTask = _dbPostInfo.DeletePost(postId);
                var RedisDeletePostTask = _redisPostInfo.DeletePostInfo(userId.ToString(), postId.ToString());
                await Task.WhenAll(DbDeletePostTask, RedisDeletePostTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeletePost Error");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}