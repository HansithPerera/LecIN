namespace AttendanceCheckInBackend.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsPresent { get; set; }
    }
}