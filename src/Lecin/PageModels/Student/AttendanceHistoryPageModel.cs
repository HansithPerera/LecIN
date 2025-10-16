using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Lecin.PageModels.Student
{
    public class AttendanceHistoryPageModel : INotifyPropertyChanged
    {
        public ObservableCollection<AttendanceRecord> Records { get; set; }

        public AttendanceHistoryPageModel()
        {
            // Demo data – replace with Supabase fetch later
            Records = new ObservableCollection<AttendanceRecord>
            {
                new AttendanceRecord { Date = "2025-09-20", Status = "Present" },
                new AttendanceRecord { Date = "2025-09-21", Status = "Absent" },
                new AttendanceRecord { Date = "2025-09-22", Status = "Present" }
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class AttendanceRecord
    {
        public required string Date { get; set; }
        public required string Status { get; set; }

        public string StatusText => $"{Date} - {Status}";
        public Color StatusColor => Status == "Present" ? Colors.Green : Colors.Red;
    }
}
