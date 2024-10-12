using System;
using System.Collections.Generic;

namespace twiker_backend.Db.Models
{
    public partial class UserTable
    {
        public Guid UserId { get; set; }

        public string? Firstname { get; set; }

        public string? Lastname { get; set; }

        public string Username { get; set; } = null!;

        public string? Email { get; set; }

        public string? Password { get; set; }

        public string? Profilepic { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<LikeTable> LikeTables { get; set; } = [];

        public virtual ICollection<PostTable> PostTables { get; set; } = [];

        public virtual ICollection<RetweetTable> RetweetTables { get; set; } = [];
    }

    public class UserDbData 
    {
        public Guid UserId { get; set; }
        public string? Firstname { get; set; }

        public string? Lastname { get; set; }

        public string? Username { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public string? Profilepic { get; set; }
    }

    public class RegisterModel
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginModel
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}