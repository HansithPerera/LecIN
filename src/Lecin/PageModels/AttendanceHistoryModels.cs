namespace Lecin.Models
{
    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late,
        Excused
    }

    public class AttendanceHistoryItem
    {
        public string StudentId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Time { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; }
        public string RawReason { get; set; } = string.Empty;

        // UI Helper Properties
        public string DisplayDate => Date.ToString("MMM dd, yyyy");
        public string StatusText => Status.ToString();
        public string StatusIcon
        {
            get
            {
                return Status switch
                {
                    AttendanceStatus.Present => "✓",
                    AttendanceStatus.Absent => "✗",
                    AttendanceStatus.Late => "⏰",
                    AttendanceStatus.Excused => "📝",
                    _ => "?"
                };
            }
        }
        
        public Color StatusColor
        {
            get
            {
                return Status switch
                {
                    AttendanceStatus.Present => Colors.Green,
                    AttendanceStatus.Absent => Colors.Red,
                    AttendanceStatus.Late => Colors.Orange,
                    AttendanceStatus.Excused => Colors.Blue,
                    _ => Colors.Gray
                };
            }
        }

        public Color BackgroundColor
        {
            get
            {
                return Status switch
                {
                    AttendanceStatus.Present => Color.FromArgb("#E8F5E8"),
                    AttendanceStatus.Absent => Color.FromArgb("#FFF2F2"),
                    AttendanceStatus.Late => Color.FromArgb("#FFF8E1"),
                    AttendanceStatus.Excused => Color.FromArgb("#E3F2FD"),
                    _ => Color.FromArgb("#F5F5F5")
                };
            }
        }
    }

    public class AttendanceDetailItem
    {
        public string StudentId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Time { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string ClassTime { get; set; } = string.Empty;
        public AttendanceStatus AttendanceType { get; set; }
        public string Notes { get; set; } = string.Empty;

        // UI Helper Properties
        public string DisplayDate => Date.ToString("MMMM dd, yyyy");
        public string StatusText => Status.ToString();
        public string StatusIcon
        {
            get
            {
                return Status switch
                {
                    AttendanceStatus.Present => "✓",
                    AttendanceStatus.Absent => "✗", 
                    AttendanceStatus.Late => "⏰",
                    AttendanceStatus.Excused => "📝",
                    _ => "?"
                };
            }
        }

        public Color StatusColor
        {
            get
            {
                return Status switch
                {
                    AttendanceStatus.Present => Colors.Green,
                    AttendanceStatus.Absent => Colors.Red,
                    AttendanceStatus.Late => Colors.Orange,
                    AttendanceStatus.Excused => Colors.Blue,
                    _ => Colors.Gray
                };
            }
        }
    }
}