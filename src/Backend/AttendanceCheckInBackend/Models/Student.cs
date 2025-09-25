namespace AttendanceCheckInBackend.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StudentId { get; set; }
        public string FaceEncoding { get; set; } // Encoded face data for recognition
    }
}