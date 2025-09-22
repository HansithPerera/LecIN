
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lecin.Models
{
    [Table("Admins")]
    public class Admin : BaseModel
    {
        [PrimaryKey("Id")]
        public Guid Id { get; set; }

        [Column("FirstName")]
        public string FirstName { get; set; } = string.Empty;

        [Column("LastName")]
        public string LastName { get; set; } = string.Empty;

        [Column("Permissions")]
        public long Permissions { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
}
