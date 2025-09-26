namespace Lecin.Models
{
    public class AttendanceStats
    {
        public string StudentId { get; set; }
        public int TotalClassesEnrolled { get; set; }
        public int ClassesAttended { get; set; }
        
        public double AttendancePercentage => TotalClassesEnrolled > 0 
            ? Math.Round((double)ClassesAttended / TotalClassesEnrolled * 100, 1) 
            : 0.0;
            
        public string AttendancePercentageDisplay => $"{AttendancePercentage}%";
        
        // for Ui styling
        public string AttendanceStatus
        {
            get
            {
                if (AttendancePercentage >= 90) return "Excellent";
                if (AttendancePercentage >= 80) return "Good"; 
                if (AttendancePercentage >= 60) return "Average";
                if (AttendancePercentage > 0) return "Poor";
                return "No Records";
            }
        }
        
        // Helper property for color coding
        public Color StatusColor
        {
            get
            {
                if (AttendancePercentage >= 80) return Colors.Green;
                if (AttendancePercentage >= 60) return Colors.Orange;
                return Colors.Red;
            }
        }
    }
}