using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Lecin.Models
{
    [Table("Classes")]
    public class Class : BaseModel
    {
        [PrimaryKey("ClassId")]
        public string ClassId { get; set; } = string.Empty;
        
        [Column("CourseCode")]
        public string CourseCode { get; set; } = string.Empty;
        
        [Column("ClassName")]
        public string ClassName { get; set; } = string.Empty;
    }
}