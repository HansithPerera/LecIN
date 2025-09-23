using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Lecin.PageModels.Student
{
    public partial class AttendanceHistoryPageModel : ObservableObject
    {
        // A list of attendance records
        [ObservableProperty]
        private ObservableCollection<string> _attendanceRecords = new();

        public AttendanceHistoryPageModel()
        {
            LoadFakeData();
        }

        // Command to refresh data (can be linked to pull-to-refresh later)
        [RelayCommand]
        private void Refresh()
        {
            LoadFakeData();
        }

        private void LoadFakeData()
        {
            AttendanceRecords.Clear();
            AttendanceRecords.Add("2025-09-20 - Present");
            AttendanceRecords.Add("2025-09-21 - Absent");
            AttendanceRecords.Add("2025-09-22 - Present");
            AttendanceRecords.Add("2025-09-23 - Present");
        }
    }
}
