using System;

namespace LecIN_System.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public Student Student { get; set; }
        public Course Course { get; set; }
        public DateTime Date { get; set; }
        public AttendanceStatus Status { get; set; }
    }

    public enum AttendanceStatus
    {
        Present,
        Absent,
        Excused,
        Late
    }
}
