using System;
using System.Collections.Generic;

namespace twiker_backend.Db.Models
{
    public partial class PinnedTable
    {
        public Guid PostId { get; set; }

        public virtual PostTable Post { get; set; } = null!;
    }
}