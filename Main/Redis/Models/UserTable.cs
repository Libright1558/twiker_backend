using System;
using System.Collections.Generic;

namespace twiker_backend.Redis.Models
{
    public class RedisUserData
    {
        public string? Firstname { get; set; }

        public string? Lastname { get; set; }

        public string? Username { get; set; }

        public string? Email { get; set; }

        public string? Profilepic { get; set; }
    }
}