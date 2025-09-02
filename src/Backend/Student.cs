using System.Collections.Generic;

namespace LecIN_System.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string StudentEmail { get; set; }
        public string StudentId { get; set; }
        public string FacialDataReference { get; set; }
        public List<Attendance> AttendanceHistory { get; set; } = new List<Attendance>();
    }
}
