using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Lecin.Models
{
    [Table("Enrollments")]
    public class Enrollment : BaseModel
    {
        [PrimaryKey("Id")]
        public int Id { get; set; }
        
        [Column("StudentId")]
        public string StudentId { get; set; } = string.Empty;
        
        [Column("ClassId")]
        public string ClassId { get; set; } = string.Empty;
        
        [Column("EnrollmentDate")]
        public DateTime EnrollmentDate { get; set; }
    }
}