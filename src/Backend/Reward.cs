namespace LecIN_System.Models
{
    public class Reward
    {
        public int Id { get; set; }
        public Student Student { get; set; }
        public string RewardType { get; set; } // e.g., "5 Day Streak", "Perfect Attendance"
        public DateTime DateAwarded { get; set; }
    }
}
