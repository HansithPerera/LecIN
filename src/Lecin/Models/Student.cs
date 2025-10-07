using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Lecin.Models
{
    [Table("Students")]
    public class Student : BaseModel
    {
        [PrimaryKey("StudentId")]
        public string StudentId { get; set; } = string.Empty;
        
        [Column("Name")]
        public string Name { get; set; } = string.Empty;
        
        [Column("Email")]
        public string Email { get; set; } = string.Empty;
    }
}