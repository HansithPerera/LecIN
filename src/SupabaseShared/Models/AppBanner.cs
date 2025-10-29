using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupabaseShared.Models
{
    [Table("app_banners")]
    public class AppBanner : BaseModel
    {
        [PrimaryKey("id")] public Guid Id { get; set; }
        [Column("is_active")] public bool IsActive { get; set; }
        [Column("message")] public string Message { get; set; } = string.Empty;
        [Column("updated_at")] public DateTime? UpdatedAt { get; set; }
    }
}