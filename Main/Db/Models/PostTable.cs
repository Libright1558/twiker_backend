using System;
using System.Collections.Generic;

namespace twiker_backend.Db.Models
{
    public partial class PostTable
    {
        public Guid PostId { get; set; }

        public string Postby { get; set; } = null!;

        public string? Content { get; set; }

        public DateTime? CreatedAt { get; set; }

        public virtual LikeTable? LikeTable { get; set; }

        public virtual PinnedTable? PinnedTable { get; set; }

        public virtual UserTable PostbyNavigation { get; set; } = null!;

        public virtual RetweetTable? RetweetTable { get; set; }
    }

    public class PostFetch
    {
        public Guid PostId { get; set; }

        public string? Postby { get; set; }

        public string? Content { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int? LikeNum { get; set; }

        public int? RetweetNum { get; set; }

        public bool? SelfLike { get; set; }

        public bool? SelfRetweet { get; set; }

        public string? Firstname { get; set; }

        public string? Lastname { get; set; }

        public string? Profilepic { get; set; }
    }
}