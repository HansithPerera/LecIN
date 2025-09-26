using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Lecin.Models
{
    [Table("Attendance")]
    public class Attendance : BaseModel
    {
        [PrimaryKey("Id")]
        public int Id { get; set; }
        
        [Column("ClassId")]
        public string ClassId { get; set; } = string.Empty;
        
        [Column("StudentId")]
        public string StudentId { get; set; } = string.Empty;
        
        [Column("Timestamp")]
        public DateTime Timestamp { get; set; }
        
        [Column("Reason")]
        public string Reason { get; set; } = string.Empty;
    }
}