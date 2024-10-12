using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using twiker_backend.Redis.Models;
using twiker_backend.Db.Models;

namespace twiker_backend.ServiceLayer
{
    public partial class PostService : IPostService
    {
        private class MissingDataLists
        {
            public List<Guid> ContentAbsent { get; set; } = [];
            public List<Guid> CreatedAtAbsent { get; set; } = [];
            public List<Guid> LikeNumsAbsent { get; set; } = [];
            public List<Guid> RetweetNumsAbsent { get; set; } = [];
            public List<Guid> SelfLikeAbsent { get; set; } = [];
            public List<Guid> SelfRetweetAbsent { get; set; } = [];
            public List<Guid> PostOwnerAbsent { get; set; } = [];
            public List<Guid> FirstnameAbsent { get; set; } = [];
            public List<Guid> LastnameAbsent { get; set; } = [];
            public List<Guid> ProfilepicAbsent { get; set; } = [];
        }

        private class FilledDataResults
        {
            public PostFetch[]? ContentAbsentResult { get; set; }
            public PostFetch[]? CreatedAtAbsentResult { get; set; }
            public PostFetch[]? LikeNumsAbsentResult { get; set; }
            public PostFetch[]? RetweetNumsAbsentResult { get; set; }
            public PostFetch[]? SelfLikeAbsentResult { get; set; }
            public PostFetch[]? SelfRetweetAbsentResult { get; set; }
            public PostFetch[]? PostOwnerAbsentResult { get; set; }
            public PostFetch[]? FirstnameAbsentResult { get; set; }
            public PostFetch[]? LastnameAbsentResult { get; set; }
            public PostFetch[]? ProfilepicAbsentResult { get; set; }
        }

        private async Task<PostFetch?[]> GetPostsFromDbAndCacheInRedis(Guid userId, string username)
        {
            var postsFromDb = (await _dbPostInfo.GetPostsByUser(username)).ToArray();
            var postArrayNest = CreatePostArrayNest(postsFromDb!, userId);

            await _redisPostInfo.WritePostInfo(userId.ToString(), postArrayNest);
            await _redisPostInfo.WritePostIdArray(userId.ToString(), postsFromDb.Select(p => p!.PostId.ToString()).ToArray());

            return postsFromDb;
        }

        private async Task<PostFetch?[]> GetPostsFromRedisAndFillMissingData(Guid userId, string username, string[] postIdArray, RedisPostTable redisPost)
        {
            var postGuidArray = postIdArray.Select(Guid.Parse).ToArray();
            var missingData = GetMissingDataLists(postGuidArray, redisPost);

            var filledData = await FillMissingData(missingData, username);
            var postArrayNest = CreatePostArrayNestByFilledDataResults(filledData, userId);

            await _redisPostInfo.WritePostInfo(userId.ToString(), postArrayNest);
            
            return CombineRedisAndFilledData(postIdArray, redisPost, filledData);
        }

        private static MissingDataLists GetMissingDataLists(Guid[] postGuidArray, RedisPostTable redisPost)
        {
            var missingData = new MissingDataLists();

            for (int i = 0; i < postGuidArray.Length; i++)
            {
                if (redisPost.Content?[i] == null) missingData.ContentAbsent.Add(postGuidArray[i]);
                if (redisPost.CreatedAt?[i] == null) missingData.CreatedAtAbsent.Add(postGuidArray[i]);
                if (redisPost.LikeNums?[i] == null) missingData.LikeNumsAbsent.Add(postGuidArray[i]);
                if (redisPost.RetweetNums?[i] == null) missingData.RetweetNumsAbsent.Add(postGuidArray[i]);
                if (redisPost.SelfLike?[i] == null) missingData.SelfLikeAbsent.Add(postGuidArray[i]);
                if (redisPost.SelfRetweet?[i] == null) missingData.SelfRetweetAbsent.Add(postGuidArray[i]);
                if (redisPost.PostOwner?[i] == null) missingData.PostOwnerAbsent.Add(postGuidArray[i]);
                if (redisPost.Firstname?[i] == null) missingData.FirstnameAbsent.Add(postGuidArray[i]);
                if (redisPost.Lastname?[i] == null) missingData.LastnameAbsent.Add(postGuidArray[i]);
                if (redisPost.Profilepic?[i] == null) missingData.ProfilepicAbsent.Add(postGuidArray[i]);
            }

            return missingData;
        }

        private async Task<FilledDataResults> FillMissingData(MissingDataLists missingData, string username)
        {
            var Content = await _dbPostInfo.FetchPostContentAsync([.. missingData.ContentAbsent]);
            var PostTime = await _dbPostInfo.FetchPostPostTimeAsync([.. missingData.CreatedAtAbsent]);
            var LikeNum = await _dbPostInfo.FetchLikeNumAsync([.. missingData.LikeNumsAbsent]);
            var RetweetNum = await _dbPostInfo.FetchRetweetNumAsync([.. missingData.RetweetNumsAbsent]);
            var SelfLike = await _dbPostInfo.FetchSelfLikeAsync([.. missingData.SelfLikeAbsent], username);
            var SelfRetweet = await _dbPostInfo.FetchSelfRetweetAsync([.. missingData.SelfRetweetAbsent], username);
            var PostOwner = await _dbPostInfo.FetchPostOwnerAsync([.. missingData.PostOwnerAbsent]);
            var Firstname = await _dbPostInfo.FetchPostFirstnameAsync([.. missingData.FirstnameAbsent]);
            var Lastname = await _dbPostInfo.FetchPostLastnameAsync([.. missingData.LastnameAbsent]);
            var Profilepic = await _dbPostInfo.FetchPostProfilepicAsync([.. missingData.ProfilepicAbsent]);

            return new FilledDataResults
            {
                ContentAbsentResult = [.. Content],
                CreatedAtAbsentResult = [.. PostTime],
                LikeNumsAbsentResult = [.. LikeNum],
                RetweetNumsAbsentResult = [.. RetweetNum],
                SelfLikeAbsentResult = [.. SelfLike],
                SelfRetweetAbsentResult = [.. SelfRetweet],
                PostOwnerAbsentResult = [.. PostOwner],
                FirstnameAbsentResult = [.. Firstname],
                LastnameAbsentResult = [.. Lastname],
                ProfilepicAbsentResult = [.. Profilepic]
            };
        }

        private static PostArrayNest CreatePostArrayNest(PostFetch[]? posts, Guid userId)
        {
            var postArrayNest = new PostArrayNest
            {
                Content = [],
                CreatedAt = [],
                LikeNums = [],
                RetweetNums = [],
                SelfLike = [],
                SelfRetweet = [],
                PostOwner = [],
                Firstname = [],
                Lastname = [],
                Profilepic = []
            };
            
            if (posts != null)
            {
                foreach (var post in posts)
                {
                    postArrayNest.Content.Add((RedisKey)$"{post.PostId}_Content", (RedisValue)post.Content);
                    postArrayNest.CreatedAt.Add((RedisKey)$"{post.PostId}_CreatedAt", (RedisValue)post.CreatedAt.ToString());
                    postArrayNest.LikeNums.Add((RedisKey)$"{post.PostId}_LikeNums", (RedisValue)post.LikeNum.ToString());
                    postArrayNest.RetweetNums.Add((RedisKey)$"{post.PostId}_RetweetNums", (RedisValue)post.RetweetNum.ToString());
                    postArrayNest.SelfLike.Add((RedisKey)$"{post.PostId}_{userId}_SelfLike", (RedisValue)post.SelfLike.ToString());
                    postArrayNest.SelfRetweet.Add((RedisKey)$"{post.PostId}_{userId}_SelfRetweet", (RedisValue)post.SelfRetweet.ToString());
                    postArrayNest.PostOwner.Add((RedisKey)$"{post.PostId}_PostOwner", (RedisValue)post.Postby);
                    postArrayNest.Firstname.Add((RedisKey)$"{post.PostId}_Firstname", (RedisValue)post.Firstname);
                    postArrayNest.Lastname.Add((RedisKey)$"{post.PostId}_Lastname", (RedisValue)post.Lastname);
                    postArrayNest.Profilepic.Add((RedisKey)$"{post.PostId}_Profilepic", (RedisValue)post.Profilepic);
                }
            }
            
            return postArrayNest;
        }

        private static PostArrayNest CreatePostArrayNestByFilledDataResults(FilledDataResults filledData, Guid userId)
        {
            var postArrayNest = new PostArrayNest
            {
                Content = [],
                CreatedAt = [],
                LikeNums = [],
                RetweetNums = [],
                SelfLike = [],
                SelfRetweet = [],
                PostOwner = [],
                Firstname = [],
                Lastname = [],
                Profilepic = []
            };

            foreach (var x in filledData.ContentAbsentResult!)
            {
                postArrayNest.Content.Add((RedisKey)$"{x.PostId}_Content", (RedisValue)x.Content);
            }

            foreach (var x in filledData.CreatedAtAbsentResult!)
            {
                postArrayNest.CreatedAt.Add((RedisKey)$"{x.PostId}_CreatedAt", (RedisValue)x.CreatedAt.ToString());
            }

            foreach (var x in filledData.LikeNumsAbsentResult!)
            {
                postArrayNest.LikeNums.Add((RedisKey)$"{x.PostId}_LikeNums", (RedisValue)x.LikeNum.ToString());
            }

            foreach (var x in filledData.RetweetNumsAbsentResult!)
            {
                postArrayNest.RetweetNums.Add((RedisKey)$"{x.PostId}_RetweetNums", (RedisValue)x.RetweetNum.ToString());
            }

            foreach (var x in filledData.SelfLikeAbsentResult!)
            {
                postArrayNest.SelfLike.Add((RedisKey)$"{x.PostId}_{userId}_SelfLike", (RedisValue)x.SelfLike.ToString());
            }

            foreach (var x in filledData.SelfRetweetAbsentResult!)
            {
                postArrayNest.SelfRetweet.Add((RedisKey)$"{x.PostId}_{userId}_SelfRetweet", (RedisValue)x.SelfRetweet.ToString());
            }

            foreach (var x in filledData.PostOwnerAbsentResult!)
            {
                postArrayNest.PostOwner.Add((RedisKey)$"{x.PostId}_PostOwner", (RedisValue)x.Postby);
            }

            foreach (var x in filledData.FirstnameAbsentResult!)
            {
                postArrayNest.Firstname.Add((RedisKey)$"{x.PostId}_Firstname", (RedisValue)x.Firstname);
            }

            foreach (var x in filledData.LastnameAbsentResult!)
            {
                postArrayNest.Lastname.Add((RedisKey)$"{x.PostId}_Lastname", (RedisValue)x.Lastname);
            }

            foreach (var x in filledData.ProfilepicAbsentResult!)
            {
                postArrayNest.Profilepic.Add((RedisKey)$"{x.PostId}_Profilepic", (RedisValue)x.Profilepic);
            }

            return postArrayNest;
        }

        private static PostFetch[] CombineRedisAndFilledData(string[] postIdArray, RedisPostTable redisPost, FilledDataResults filledData)
        {
            var result = new PostFetch[postIdArray.Length];
            var filledDataIterators = new Dictionary<string, int>();

            for (int i = 0; i < postIdArray.Length; i++)
            {
                string ContentValue = GetValueOrFilled(redisPost.Content!, i, filledData.ContentAbsentResult!, "Content", filledDataIterators);
                string CreatedAtValue = GetValueOrFilled(redisPost.CreatedAt!, i, filledData.CreatedAtAbsentResult!, "CreatedAt", filledDataIterators);
                string LikeNumValue = GetValueOrFilled(redisPost.LikeNums!, i, filledData.LikeNumsAbsentResult!, "LikeNum", filledDataIterators);
                string RetweetNumValue = GetValueOrFilled(redisPost.RetweetNums!, i, filledData.RetweetNumsAbsentResult!, "RetweetNum", filledDataIterators);
                string SelfLikeValue = GetValueOrFilled(redisPost.SelfLike!, i, filledData.SelfLikeAbsentResult!, "SelfLike", filledDataIterators);
                string SelfRetweetValue = GetValueOrFilled(redisPost.SelfRetweet!, i, filledData.SelfRetweetAbsentResult!, "SelfRetweet", filledDataIterators);
                string PostbyValue = GetValueOrFilled(redisPost.PostOwner!, i, filledData.PostOwnerAbsentResult!, "Postby", filledDataIterators);
                string FirstnameValue = GetValueOrFilled(redisPost.Firstname!, i, filledData.FirstnameAbsentResult!, "Firstname", filledDataIterators);
                string LastnameValue = GetValueOrFilled(redisPost.Lastname!, i, filledData.LastnameAbsentResult!, "Lastname", filledDataIterators);
                string ProfilepicValue = GetValueOrFilled(redisPost.Profilepic!, i, filledData.ProfilepicAbsentResult!, "Profilepic", filledDataIterators);

                result[i] = new PostFetch
                {
                    PostId = new Guid(postIdArray[i]),
                    Content = ContentValue,
                    CreatedAt = string.IsNullOrEmpty(CreatedAtValue) ? DateTime.Now : DateTime.Parse(CreatedAtValue),
                    LikeNum = string.IsNullOrEmpty(LikeNumValue) ? 0 : int.Parse(LikeNumValue),
                    RetweetNum = string.IsNullOrEmpty(RetweetNumValue) ? 0 : int.Parse(RetweetNumValue),
                    SelfLike = !string.IsNullOrEmpty(SelfLikeValue) && bool.Parse(SelfLikeValue),
                    SelfRetweet = !string.IsNullOrEmpty(SelfRetweetValue) && bool.Parse(SelfRetweetValue),
                    Postby = PostbyValue,
                    Firstname = FirstnameValue,
                    Lastname = LastnameValue,
                    Profilepic = ProfilepicValue
                };
            }

            return result;
        }

        private static string GetValueOrFilled<T>(string[] redisValues, int index, T[] filledValues, string propertyName, Dictionary<string, int> iterators)
        {
            if (redisValues?[index] != null)
            {
                return redisValues[index];
            }
            
            if (!iterators.ContainsKey(propertyName))
            {
                iterators[propertyName] = 0;
            }

            var value = filledValues[iterators[propertyName]++];
            return value!.GetType()!.GetProperty(propertyName)!.GetValue(value, null)!.ToString()!;
        }
    }
}