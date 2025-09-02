using System;
using System.Collections.Generic;

namespace LecIN_System.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Teacher Instructor { get; set; }
        public List<Student> EnrolledStudents { get; set; } = new List<Student>();
    }
}
