using System;
using System.Collections.Generic;
using StackExchange.Redis;

namespace twiker_backend.Redis.Models
{
    public class RedisPostTable
    {
        public string?[] Content { get; set; } = null!;

        public string?[] CreatedAt { get; set; } = null!;

        public string?[] LikeNums { get; set; } = null!;

        public string?[] RetweetNums { get; set; } = null!;

        public string?[] SelfLike { get; set; } = null!;

        public string?[] SelfRetweet { get; set; } = null!;

        public string?[] PostOwner { get; set; } = null!;

        public string?[] Firstname { get; set; } = null!;

        public string?[] Lastname { get; set; } = null!;

        public string?[] Profilepic { get; set; } = null!;
    }


    public class PostArrayNest
    {
        public Dictionary<RedisKey, RedisValue> Content { get; set; } = null!;
        public Dictionary<RedisKey, RedisValue> CreatedAt { get; set; } = null!;
        public Dictionary<RedisKey, RedisValue> LikeNums { get; set; } = null!;
        public Dictionary<RedisKey, RedisValue> RetweetNums { get; set; } = null!;
        public Dictionary<RedisKey, RedisValue> SelfLike { get; set; } = null!;
        public Dictionary<RedisKey, RedisValue> SelfRetweet { get; set; } = null!;
        public Dictionary<RedisKey, RedisValue> PostOwner { get; set; } = null!;
        public Dictionary<RedisKey, RedisValue> Firstname { get; set; } = null!;
        public Dictionary<RedisKey, RedisValue> Lastname { get; set; } = null!;
        public Dictionary<RedisKey, RedisValue> Profilepic { get; set; } = null!;
    }
}