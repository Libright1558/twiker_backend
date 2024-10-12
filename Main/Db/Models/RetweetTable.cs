using System;
using System.Collections.Generic;

namespace twiker_backend.Db.Models
{
    public partial class RetweetTable
    {
        public Guid PostId { get; set; }

        public string Username { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public virtual PostTable Post { get; set; } = null!;

        public virtual UserTable UsernameNavigation { get; set; } = null!;
    }
}